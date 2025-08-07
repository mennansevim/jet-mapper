using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

namespace FastMapper
{
    /// <summary>
    /// ULTRA-PERFORMANS FastMapper - AutoMapper'dan daha hızlı!
    /// </summary>
    public static class MapperExtensions
    {
        // ULTRA-FAST: Direct typed mappers - NO boxing, NO Convert.ChangeType
        private static readonly ConcurrentDictionary<long, object> _typedMappers = new();
        
        // ULTRA-FAST: Property accessors with ZERO allocation
        private static readonly ConcurrentDictionary<long, (Delegate getter, Delegate setter)[]> _propertyAccessors = new();
        
        // Custom mappings ve type converters
        private static readonly ConcurrentDictionary<string, Delegate> _customMappings = new();
        private static readonly ConcurrentDictionary<long, Delegate> _typeConverters = new();

        /// <summary>
        /// ULTRA-FAST mapping - AutoMapper'dan daha hızlı!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTarget FastMapTo<TTarget>(this object source) where TTarget : new()
        {
            if (source == null) return default(TTarget);
            
            var sourceType = source.GetType();
            var targetType = typeof(TTarget);
            var key = GetTypeKey(sourceType, targetType);
            
            // Get or create TYPED mapper - NO boxing!
            var mapper = (Func<object, TTarget>)_typedMappers.GetOrAdd(key, 
                _ => CreateUltraFastTypedMapper<TTarget>(sourceType, targetType));
            
            var startTime = DateTime.UtcNow;
            TTarget result;
            
            try
            {
                result = mapper(source);
                
                // Diagnostic kayıt
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, true);
            }
            catch (Exception ex)
            {
                // Diagnostic kayıt
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, false, ex);
                throw;
            }
            
            return result;
        }

        /// <summary>
        /// ULTRA-FAST in-place mapping - ZERO allocation!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastMapTo<TTarget>(this object source, TTarget target)
        {
            if (source == null || target == null) return;
            
            var sourceType = source.GetType();
            var targetType = typeof(TTarget);
            var key = GetTypeKey(sourceType, targetType) | unchecked((long)0x8000000000000000);
            
            // Get or create in-place mapper
            var mapper = (Action<object, TTarget>)_typedMappers.GetOrAdd(key, 
                _ => CreateUltraFastInPlaceMapper<TTarget>(sourceType, targetType));
            
            var startTime = DateTime.UtcNow;
            
            try
            {
                mapper(source, target);
                
                // Diagnostic kayıt
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, true);
            }
            catch (Exception ex)
            {
                // Diagnostic kayıt
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, false, ex);
                throw;
            }
        }

        /// <summary>
        /// ULTRA-FAST collection mapping
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TTarget> FastMapToList<TTarget>(this IEnumerable<object> sources) where TTarget : new()
        {
            if (sources == null) return new List<TTarget>();
            
            var sourceList = sources as IList<object> ?? sources.ToList();
            var result = new List<TTarget>(sourceList.Count);
            
            if (sourceList.Count == 0) return result;
            
            // Get mapper for first item type
            var sourceType = sourceList[0]?.GetType();
            if (sourceType == null) return result;
            
            var targetType = typeof(TTarget);
            var key = GetTypeKey(sourceType, targetType);
            var mapper = (Func<object, TTarget>)_typedMappers.GetOrAdd(key, 
                _ => CreateUltraFastTypedMapper<TTarget>(sourceType, targetType));
            
            // ULTRA-FAST bulk mapping with pre-allocated list
            for (int i = 0; i < sourceList.Count; i++)
            {
                if (sourceList[i] != null)
                {
                    result.Add(mapper(sourceList[i]));
                }
                else
                {
                    result.Add(default(TTarget));
                }
            }
            
            return result;
        }

        /// <summary>
        /// Creates ULTRA-FAST typed mapper with ZERO boxing
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<object, TTarget> CreateUltraFastTypedMapper<TTarget>(Type sourceType, Type targetType) where TTarget : new()
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var typedSource = Expression.Convert(sourceParam, sourceType);
            
            // Create new target instance
            var newTarget = Expression.New(targetType);
            var targetVar = Expression.Variable(targetType, "target");
            var assignTarget = Expression.Assign(targetVar, newTarget);
            
            // Get property accessors
            var key = GetTypeKey(sourceType, targetType);
            var accessors = _propertyAccessors.GetOrAdd(key, _ => CreatePropertyAccessors(sourceType, targetType));
            
            var assignments = new List<Expression> { assignTarget };
            
            // ULTRA-FAST direct property assignments - NO Convert.ChangeType!
            foreach (var (getter, setter) in accessors)
            {
                if (getter != null && setter != null)
                {
                    var getterCall = Expression.Invoke(Expression.Constant(getter), typedSource);
                    var setterCall = Expression.Invoke(Expression.Constant(setter), targetVar, getterCall);
                    assignments.Add(setterCall);
                }
            }
            
            assignments.Add(targetVar);
            var body = Expression.Block(new[] { targetVar }, assignments);
            
            var lambda = Expression.Lambda<Func<object, TTarget>>(body, sourceParam);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates ULTRA-FAST in-place mapper
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Action<object, TTarget> CreateUltraFastInPlaceMapper<TTarget>(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var targetParam = Expression.Parameter(typeof(TTarget), "target");
            var typedSource = Expression.Convert(sourceParam, sourceType);
            
            // Get property accessors
            var key = GetTypeKey(sourceType, targetType);
            var accessors = _propertyAccessors.GetOrAdd(key, _ => CreatePropertyAccessors(sourceType, targetType));
            
            var assignments = new List<Expression>();
            
            // ULTRA-FAST direct property assignments
            foreach (var (getter, setter) in accessors)
            {
                if (getter != null && setter != null)
                {
                    var getterCall = Expression.Invoke(Expression.Constant(getter), typedSource);
                    var setterCall = Expression.Invoke(Expression.Constant(setter), targetParam, getterCall);
                    assignments.Add(setterCall);
                }
            }
            
            var body = assignments.Count > 0 ? Expression.Block(assignments) : (Expression)Expression.Empty();
            var lambda = Expression.Lambda<Action<object, TTarget>>(body, sourceParam, targetParam);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates ULTRA-FAST property accessors with type safety
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (Delegate getter, Delegate setter)[] CreatePropertyAccessors(Type sourceType, Type targetType)
        {
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p);
                
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);
            
            var accessors = new List<(Delegate getter, Delegate setter)>();
            
            foreach (var targetProp in targetProps)
            {
                if (sourceProps.TryGetValue(targetProp.Name, out var sourceProp))
                {
                    // Check for custom mapping first
                    var customKey = $"{sourceType.FullName}.{sourceProp.Name}->{targetType.FullName}.{targetProp.Name}";
                    if (_customMappings.TryGetValue(customKey, out var customMapping))
                    {
                        var customGetter = CreateCustomPropertyGetter(sourceProp, customMapping);
                        var setter = CreatePropertySetter(targetProp);
                        accessors.Add((customGetter, setter));
                        continue;
                    }
                    
                    // Check type compatibility
                    if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    {
                        // Direct assignment - FASTEST path
                        var getter = CreatePropertyGetter(sourceProp);
                        var setter = CreatePropertySetter(targetProp);
                        accessors.Add((getter, setter));
                    }
                    else if (IsConvertible(sourceProp.PropertyType, targetProp.PropertyType))
                    {
                        // Type conversion needed
                        var converterKey = GetTypeKey(sourceProp.PropertyType, targetProp.PropertyType);
                        if (_typeConverters.TryGetValue(converterKey, out var converter))
                        {
                            var convertingGetter = CreateConvertingPropertyGetter(sourceProp, converter);
                            var setter = CreatePropertySetter(targetProp);
                            accessors.Add((convertingGetter, setter));
                        }
                        else
                        {
                            // Built-in type conversion
                            var convertingGetter = CreateBuiltInConvertingGetter(sourceProp, targetProp.PropertyType);
                            var setter = CreatePropertySetter(targetProp);
                            accessors.Add((convertingGetter, setter));
                        }
                    }
                }
            }
            
            return accessors.ToArray();
        }

        /// <summary>
        /// Creates property getter delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Delegate CreatePropertyGetter(PropertyInfo prop)
        {
            var param = Expression.Parameter(prop.DeclaringType, "obj");
            var propAccess = Expression.Property(param, prop);
            var lambda = Expression.Lambda(propAccess, param);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates property setter delegate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Delegate CreatePropertySetter(PropertyInfo prop)
        {
            var objParam = Expression.Parameter(prop.DeclaringType, "obj");
            var valueParam = Expression.Parameter(prop.PropertyType, "value");
            var propAccess = Expression.Property(objParam, prop);
            var assign = Expression.Assign(propAccess, valueParam);
            var lambda = Expression.Lambda(assign, objParam, valueParam);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates custom property getter with custom mapping
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Delegate CreateCustomPropertyGetter(PropertyInfo sourceProp, Delegate customMapping)
        {
            var param = Expression.Parameter(sourceProp.DeclaringType, "obj");
            var propAccess = Expression.Property(param, sourceProp);
            var customCall = Expression.Invoke(Expression.Constant(customMapping), propAccess);
            var lambda = Expression.Lambda(customCall, param);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates converting property getter with type converter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Delegate CreateConvertingPropertyGetter(PropertyInfo sourceProp, Delegate converter)
        {
            var param = Expression.Parameter(sourceProp.DeclaringType, "obj");
            var propAccess = Expression.Property(param, sourceProp);
            var convertCall = Expression.Invoke(Expression.Constant(converter), propAccess);
            var lambda = Expression.Lambda(convertCall, param);
            return lambda.Compile();
        }

        /// <summary>
        /// Creates built-in converting getter - SAFE type conversion
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Delegate CreateBuiltInConvertingGetter(PropertyInfo sourceProp, Type targetType)
        {
            var param = Expression.Parameter(sourceProp.DeclaringType, "obj");
            var propAccess = Expression.Property(param, sourceProp);
            
            Expression conversion;
            if (targetType.IsEnum && sourceProp.PropertyType == typeof(string))
            {
                // String to enum conversion
                var parseMethod = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });
                conversion = Expression.Call(parseMethod, 
                    Expression.Constant(targetType), 
                    propAccess, 
                    Expression.Constant(true));
                conversion = Expression.Convert(conversion, targetType);
            }
            else if (sourceProp.PropertyType.IsEnum && targetType == typeof(string))
            {
                // Enum to string conversion
                var toStringMethod = typeof(object).GetMethod("ToString");
                conversion = Expression.Call(propAccess, toStringMethod);
            }
            else
            {
                // Direct convert
                conversion = Expression.Convert(propAccess, targetType);
            }
            
            var lambda = Expression.Lambda(conversion, param);
            return lambda.Compile();
        }

        /// <summary>
        /// Check if types are convertible safely
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsConvertible(Type sourceType, Type targetType)
        {
            if (sourceType == targetType) return true;
            if (targetType.IsAssignableFrom(sourceType)) return true;
            
            // Numeric conversions
            if (IsNumericType(sourceType) && IsNumericType(targetType)) return true;
            
            // String conversions
            if (sourceType == typeof(string) || targetType == typeof(string)) return true;
            
            // Enum conversions
            if (sourceType.IsEnum || targetType.IsEnum) return true;
            
            // Nullable conversions
            var underlyingSource = Nullable.GetUnderlyingType(sourceType);
            var underlyingTarget = Nullable.GetUnderlyingType(targetType);
            if (underlyingSource != null || underlyingTarget != null) return true;
            
            return false;
        }

        /// <summary>
        /// Check if type is numeric
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNumericType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return typeCode >= TypeCode.SByte && typeCode <= TypeCode.Decimal;
        }

        /// <summary>
        /// ULTRA-FAST type key generation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetTypeKey(Type sourceType, Type targetType)
        {
            return ((long)sourceType.GetHashCode() << 32) | (uint)targetType.GetHashCode();
        }

        // Public API methods
        public static void AddCustomMapping<TSource, TTarget>(string sourceProperty, string targetProperty, Func<object, object> mapping)
        {
            var key = $"{typeof(TSource).FullName}.{sourceProperty}->{typeof(TTarget).FullName}.{targetProperty}";
            _customMappings.TryAdd(key, mapping);
        }

        /// <summary>
        /// Hedef property adı ve lambda ile özel mapping tanımla
        /// </summary>
        public static void AddCustomMapping<TSource, TTarget>(string targetProperty, Func<TSource, object> mapping)
        {
            var key = $"{typeof(TSource).FullName}.->{typeof(TTarget).FullName}.{targetProperty}";
            _customMappings.TryAdd(key, mapping);
        }

        public static void AddTypeConverter<TSource, TTarget>(Func<TSource, TTarget> converter)
        {
            var key = GetTypeKey(typeof(TSource), typeof(TTarget));
            _typeConverters.TryAdd(key, converter);
        }

        /// <summary>
        /// Gelişmiş in-place mapping: skipIfNull ve skipProperties desteği
        /// </summary>
        public static void FastMapTo<TTarget>(this object source, TTarget target, bool skipIfNull = false, string[] skipProperties = null)
        {
            if (source == null || target == null) return;
            var sourceType = source.GetType();
            var targetType = typeof(TTarget);
     
            // Özelleştirilmiş alan güncelleme
            var sourceProps = sourceType.GetProperties();
            var targetProps = targetType.GetProperties();
            foreach (var tProp in targetProps)
            {
                if (skipProperties != null && skipProperties.Contains(tProp.Name))
                    continue;
                var sProp = sourceProps.FirstOrDefault(x => x.Name == tProp.Name);
                if (sProp == null || !sProp.CanRead || !tProp.CanWrite)
                    continue;
                var value = sProp.GetValue(source);
                if (skipIfNull && value == null)
                    continue;
                tProp.SetValue(target, value);
            }
        }

        public static void ClearAllCaches()
        {
            _typedMappers.Clear();
            _propertyAccessors.Clear();
            _customMappings.Clear();
            _typeConverters.Clear();
        }

        public static void ClearAllCustomMappings()
        {
            _customMappings.Clear();
        }
    }
} 