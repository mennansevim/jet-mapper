#pragma warning disable CS0168 // Variable declared but never used (only used in DEBUG mode)
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JetMapper
{
    /// <summary>
    /// ULTRA-PERFORMANS JetMapper - AutoMapper'dan daha hızlı!
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

        // ULTRA-FAST: Pre-compiled simple mappers for common types
        private static readonly ConcurrentDictionary<long, Delegate> _simpleMappers = new();
        
        // ULTRA-FAST: Lazy initialization flag
        private static volatile bool _isInitialized = false;
        private static readonly object _initLock = new object();

        /// <summary>
        /// ULTRA-FAST mapping - AutoMapper'dan daha hızlı!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTarget FastMapTo<TTarget>(this object source) where TTarget : new()
        {
            if (source == null) return default(TTarget);
            
            // ULTRA-FAST: Lazy initialization
            if (!_isInitialized)
            {
                lock (_initLock)
                {
                    if (!_isInitialized)
                    {
                        InitializeJetMappers();
                        _isInitialized = true;
                    }
                }
            }
            
            var sourceType = source.GetType();
            var targetType = typeof(TTarget);
            var key = GetTypeKey(sourceType, targetType);
            
            // ULTRA-FAST: Check for pre-compiled simple mapper first
            if (_simpleMappers.TryGetValue(key, out var simpleMapper))
            {
                return ((Func<object, TTarget>)simpleMapper)(source);
            }
            
            // Get or create TYPED mapper - NO boxing!
            var mapper = (Func<object, TTarget>)_typedMappers.GetOrAdd(key, 
                _ => CreateUltraFastTypedMapper<TTarget>(sourceType, targetType));
       
            TTarget result;
            
            #if DEBUG
            var startTime = DateTime.UtcNow;
            #endif
            
            try
            {
                result = mapper(source);
                
                // Diagnostic kayıt - only in debug mode
                #if DEBUG
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, true);
                #endif
            }
            catch (Exception ex)
            {
                // Diagnostic kayıt - only in debug mode
                #if DEBUG
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, false, ex);
                #endif
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
            
            // ULTRA-FAST: Lazy initialization
            if (!_isInitialized)
            {
                lock (_initLock)
                {
                    if (!_isInitialized)
                    {
                        InitializeJetMappers();
                        _isInitialized = true;
                    }
                }
            }
            
            var sourceType = source.GetType();
            var targetType = typeof(TTarget);
            var key = GetTypeKey(sourceType, targetType) | unchecked((long)0x8000000000000000);
            
            // Get or create in-place mapper
            var mapper = (Action<object, TTarget>)_typedMappers.GetOrAdd(key, 
                _ => CreateUltraFastInPlaceMapper<TTarget>(sourceType, targetType));
            
            #if DEBUG
            var startTime = DateTime.UtcNow;
            #endif
     
            try
            {
                mapper(source, target);
                
                // Diagnostic kayıt - only in debug mode
                #if DEBUG
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, true);
                #endif
            }
            catch (Exception ex)
            {
                // Diagnostic kayıt - only in debug mode
                #if DEBUG
                DiagnosticMapper.RecordMapping<object, TTarget>(
                    DateTime.UtcNow - startTime, false, ex);
                #endif
                throw;
            }
        }

        /// <summary>
        /// ULTRA-FAST collection mapping - Generic overload
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TTarget> FastMapToList<TSource, TTarget>(this IEnumerable<TSource> sources) 
            where TTarget : new()
        {
            if (sources == null) return new List<TTarget>();
            
            return sources.Cast<object>().FastMapToList<TTarget>();
        }

        /// <summary>
        /// ULTRA-FAST collection mapping
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<TTarget> FastMapToList<TTarget>(this IEnumerable<object> sources) where TTarget : new()
        {
            if (sources == null) return new List<TTarget>();
            
            // ULTRA-FAST: Lazy initialization
            if (!_isInitialized)
            {
                lock (_initLock)
                {
                    if (!_isInitialized)
                    {
                        InitializeJetMappers();
                        _isInitialized = true;
                    }
                }
            }
            
            var list = new List<TTarget>();
            var mapper = default(Func<object, TTarget>);
            var sourceType = default(Type);
            
            foreach (var source in sources)
            {
                if (source == null) continue;
                
                // ULTRA-FAST: Cache mapper for same type
                if (mapper == null || source.GetType() != sourceType)
                {
                    sourceType = source.GetType();
                    var targetType = typeof(TTarget);
                    var key = GetTypeKey(sourceType, targetType);
                    
                    // Check for pre-compiled simple mapper first
                    if (_simpleMappers.TryGetValue(key, out var simpleMapper))
                    {
                        mapper = (Func<object, TTarget>)simpleMapper;
                    }
                    else
                    {
                        mapper = (Func<object, TTarget>)_typedMappers.GetOrAdd(key, 
                            _ => CreateUltraFastTypedMapper<TTarget>(sourceType, targetType));
                    }
                }
                
                list.Add(mapper(source));
            }
            
            return list;
        }

        /// <summary>
        /// ULTRA-FAST: Pre-compile simple mappers for common scenarios
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeJetMappers()
        {
            // Pre-compile common simple mappings
            PreCompileSimpleMappers();
        }

        /// <summary>
        /// ULTRA-FAST: Pre-compile simple mappers for common types
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void PreCompileSimpleMappers()
        {
            // Pre-compile common simple mappings
            // Bu kısım runtime'da otomatik olarak genişletilecek
        }

        /// <summary>
        /// ULTRA-FAST: Create optimized mapper for simple types
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<object, TTarget> CreateUltraFastTypedMapper<TTarget>(Type sourceType, Type targetType) where TTarget : new()
        {
            // ULTRA-FAST: Check if this is a simple mapping that can be pre-compiled
            if (IsSimpleMapping(sourceType, targetType))
            {
                return CreateSimpleTypedMapper<TTarget>(sourceType, targetType);
            }
            
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var sourceCast = Expression.Convert(sourceParam, sourceType);
            var targetParam = Expression.Parameter(targetType, "target");
            
            var propertyAccessors = _propertyAccessors.GetOrAdd(GetTypeKey(sourceType, targetType),
                _ => CreatePropertyAccessors(sourceType, targetType));
            
            var expressions = new List<Expression>();
            
            // ULTRA-FAST: Direct property mapping with NO boxing
            foreach (var (getter, setter) in propertyAccessors)
            {
                if (getter != null && setter != null)
                {
                    var getterCall = Expression.Invoke(Expression.Constant(getter), sourceCast);
                    var setterCall = Expression.Invoke(Expression.Constant(setter), targetParam, getterCall);
                    expressions.Add(setterCall);
                }
            }
            
            // ULTRA-FAST: Create target instance
            var newTarget = Expression.New(targetType);
            expressions.Insert(0, Expression.Assign(targetParam, newTarget));
            
            // ULTRA-FAST: Return the target instance
            expressions.Add(targetParam);
            
            var block = Expression.Block(new[] { targetParam }, expressions);
            var lambda = Expression.Lambda<Func<object, TTarget>>(block, sourceParam);
            
            return lambda.Compile();
        }

        /// <summary>
        /// ULTRA-FAST: Check if this is a simple mapping
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSimpleMapping(Type sourceType, Type targetType)
        {
            // Simple mapping: primitive types, strings, basic DTOs
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // ULTRA-FAST: Quick check for simple scenarios
            if (sourceProps.Length <= 5 && targetProps.Length <= 5)
            {
                foreach (var sourceProp in sourceProps)
                {
                    if (!sourceProp.CanRead) continue;
                    
                    var targetProp = targetProps.FirstOrDefault(p => 
                        p.CanWrite && 
                        string.Equals(p.Name, sourceProp.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (targetProp == null) continue;
                    
                    // ULTRA-FAST: Check if types are directly compatible
                    if (!IsDirectlyCompatible(sourceProp.PropertyType, targetProp.PropertyType))
                    {
                        return false;
                    }
                }
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// ULTRA-FAST: Check if types are directly compatible
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirectlyCompatible(Type sourceType, Type targetType)
        {
            // ULTRA-FAST: Direct type compatibility check
            if (sourceType == targetType) return true;
            if (sourceType.IsPrimitive && targetType.IsPrimitive) return true;
            if (sourceType == typeof(string) && targetType == typeof(string)) return true;
            if (sourceType == typeof(DateTime) && targetType == typeof(DateTime)) return true;
            if (sourceType == typeof(Guid) && targetType == typeof(Guid)) return true;
            
            // ULTRA-FAST: Nullable type compatibility
            if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingSourceType = Nullable.GetUnderlyingType(sourceType);
                if (underlyingSourceType == targetType) return true;
            }
            
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingTargetType = Nullable.GetUnderlyingType(targetType);
                if (sourceType == underlyingTargetType) return true;
            }
            
            return false;
        }

        /// <summary>
        /// ULTRA-FAST: Create simple typed mapper for basic scenarios
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<object, TTarget> CreateSimpleTypedMapper<TTarget>(Type sourceType, Type targetType) where TTarget : new()
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var sourceCast = Expression.Convert(sourceParam, sourceType);
            
            // ULTRA-FAST: Create target instance
            var newTarget = Expression.New(targetType);
            var targetVar = Expression.Variable(targetType, "target");
            
            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(targetVar, newTarget));
            
            // ULTRA-FAST: Direct property mapping for simple types
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead) continue;
                
                var targetProp = targetProps.FirstOrDefault(p => 
                    p.CanWrite && 
                    string.Equals(p.Name, sourceProp.Name, StringComparison.OrdinalIgnoreCase));
                
                if (targetProp == null) continue;
                
                // ULTRA-FAST: Safe type conversion with compatibility check
                Expression assignment;
                
                if (sourceProp.PropertyType == targetProp.PropertyType)
                {
                    // Direct assignment for same types
                    var sourceProperty = Expression.Property(sourceCast, sourceProp);
                    var targetProperty = Expression.Property(targetVar, targetProp);
                    assignment = Expression.Assign(targetProperty, sourceProperty);
                    expressions.Add(assignment);
                }
                else if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    // Direct assignment for compatible types
                    var sourceProperty = Expression.Property(sourceCast, sourceProp);
                    var targetProperty = Expression.Property(targetVar, targetProp);
                    assignment = Expression.Assign(targetProperty, sourceProperty);
                    expressions.Add(assignment);
                }
                else if (IsConvertible(sourceProp.PropertyType, targetProp.PropertyType))
                {
                    // Convert assignment for convertible types
                    var sourceProperty = Expression.Property(sourceCast, sourceProp);
                    var targetProperty = Expression.Property(targetVar, targetProp);
                    var converted = Expression.Convert(sourceProperty, targetProp.PropertyType);
                    assignment = Expression.Assign(targetProperty, converted);
                    expressions.Add(assignment);
                }
                // Skip incompatible properties
            }
            
            // Return the target
            expressions.Add(targetVar);
            
            var block = Expression.Block(new[] { targetVar }, expressions);
            var lambda = Expression.Lambda<Func<object, TTarget>>(block, sourceParam);
            
            return lambda.Compile();
        }

        /// <summary>
        /// ULTRA-FAST: Create optimized in-place mapper for simple types
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Action<object, TTarget> CreateUltraFastInPlaceMapper<TTarget>(Type sourceType, Type targetType)
        {
            // ULTRA-FAST: Check if this is a simple mapping that can be optimized
            if (IsSimpleMapping(sourceType, targetType))
            {
                return CreateSimpleInPlaceMapper<TTarget>(sourceType, targetType);
            }
            
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
        /// ULTRA-FAST: Create simple in-place mapper for basic scenarios
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Action<object, TTarget> CreateSimpleInPlaceMapper<TTarget>(Type sourceType, Type targetType)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var targetParam = Expression.Parameter(typeof(TTarget), "target");
            var sourceCast = Expression.Convert(sourceParam, sourceType);
            
            var expressions = new List<Expression>();
            
            // ULTRA-FAST: Direct property mapping for simple types
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var sourceProp in sourceProps)
            {
                if (!sourceProp.CanRead) continue;
                
                var targetProp = targetProps.FirstOrDefault(p => 
                    p.CanWrite && 
                    string.Equals(p.Name, sourceProp.Name, StringComparison.OrdinalIgnoreCase));
                
                if (targetProp == null) continue;
                
                // ULTRA-FAST: Safe type conversion with compatibility check
                Expression assignment;
                
                if (sourceProp.PropertyType == targetProp.PropertyType)
                {
                    // Direct assignment for same types
                    var sourceProperty = Expression.Property(sourceCast, sourceProp);
                    var targetProperty = Expression.Property(targetParam, targetProp);
                    assignment = Expression.Assign(targetProperty, sourceProperty);
                    expressions.Add(assignment);
                }
                else if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    // Direct assignment for compatible types
                    var sourceProperty = Expression.Property(sourceCast, sourceProp);
                    var targetProperty = Expression.Property(targetParam, targetProp);
                    assignment = Expression.Assign(targetProperty, sourceProperty);
                    expressions.Add(assignment);
                }
                else if (IsConvertible(sourceProp.PropertyType, targetProp.PropertyType))
                {
                    // Convert assignment for convertible types
                    var sourceProperty = Expression.Property(sourceCast, sourceProp);
                    var targetProperty = Expression.Property(targetParam, targetProp);
                    var converted = Expression.Convert(sourceProperty, targetProp.PropertyType);
                    assignment = Expression.Assign(targetProperty, converted);
                    expressions.Add(assignment);
                }
                // Skip incompatible properties
            }
            
            var body = expressions.Count > 0 ? Expression.Block(expressions) : (Expression)Expression.Empty();
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
                    
                    // ULTRA-FAST: Enhanced type compatibility check
                    if (IsDirectlyCompatible(sourceProp.PropertyType, targetProp.PropertyType))
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
                    // Skip incompatible properties to avoid runtime errors
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
            else if (IsEnumListType(targetType, out var enumElementType))
            {
                // String/JArray to List<TEnum> or IEnumerable<TEnum>
                if (sourceProp.PropertyType == typeof(string))
                {
                    var method = typeof(MapperExtensions)
                        .GetMethod(nameof(ParseEnumListFromString), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(enumElementType);
                    var call = Expression.Call(method, propAccess);
                    conversion = call.Type != targetType ? Expression.Convert(call, targetType) : (Expression)call;
                }
                else if (sourceProp.PropertyType == typeof(JArray))
                {
                    var method = typeof(MapperExtensions)
                        .GetMethod(nameof(ParseEnumListFromJArray), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(enumElementType);
                    var call = Expression.Call(method, propAccess);
                    conversion = call.Type != targetType ? Expression.Convert(call, targetType) : (Expression)call;
                }
                else
                {
                    // Fallback: try string conversion via ToString
                    var toStringMethod = typeof(object).GetMethod("ToString");
                    var asString = Expression.Call(propAccess, toStringMethod);
                    var method = typeof(MapperExtensions)
                        .GetMethod(nameof(ParseEnumListFromString), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(enumElementType);
                    var call = Expression.Call(method, asString);
                    conversion = call.Type != targetType ? Expression.Convert(call, targetType) : (Expression)call;
                }
            }
            else if (targetType.IsArray && targetType.GetElementType() != null && targetType.GetElementType().IsEnum)
            {
                var arrayEnumElementType = targetType.GetElementType();
                if (sourceProp.PropertyType == typeof(string))
                {
                    var method = typeof(MapperExtensions)
                        .GetMethod(nameof(ParseEnumArrayFromString), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(arrayEnumElementType);
                    conversion = Expression.Call(method, propAccess);
                }
                else if (sourceProp.PropertyType == typeof(JArray))
                {
                    var method = typeof(MapperExtensions)
                        .GetMethod(nameof(ParseEnumArrayFromJArray), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(arrayEnumElementType);
                    conversion = Expression.Call(method, propAccess);
                }
                else
                {
                    var toStringMethod = typeof(object).GetMethod("ToString");
                    var asString = Expression.Call(propAccess, toStringMethod);
                    var method = typeof(MapperExtensions)
                        .GetMethod(nameof(ParseEnumArrayFromString), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(arrayEnumElementType);
                    conversion = Expression.Call(method, asString);
                }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEnumListType(Type type, out Type enumElementType)
        {
            enumElementType = null;
            if (!type.IsGenericType) return false;
            var genDef = type.GetGenericTypeDefinition();
            if (genDef != typeof(List<>) && genDef != typeof(IList<>) && genDef != typeof(IEnumerable<>))
                return false;
            var elem = type.GetGenericArguments()[0];
            if (!elem.IsEnum) return false;
            enumElementType = elem;
            return true;
        }

        // Helpers: JSON/String → Enum / Enum List / Enum Array
        private static bool TryParseEnumToken<TEnum>(string token, out TEnum value) where TEnum : struct
        {
            value = default;
            if (string.IsNullOrWhiteSpace(token)) return false;
            token = token.Trim();
            // direct
            if (Enum.TryParse<TEnum>(token, true, out value)) return true;
            // normalize simple separators
            var normalized = token.Replace("-", "_").Replace(" ", "");
            return Enum.TryParse<TEnum>(normalized, true, out value);
        }

        private static TEnum ParseEnumStrict<TEnum>(string token) where TEnum : struct
        {
            if (TryParseEnumToken<TEnum>(token, out var parsed))
            {
                return parsed;
            }
            throw new InvalidOperationException($"Invalid enum value '{token}' for {typeof(TEnum).Name}");
        }

        private static List<TEnum> ParseEnumListFromString<TEnum>(string input) where TEnum : struct
        {
            var list = new List<TEnum>();
            if (string.IsNullOrWhiteSpace(input)) return list;
            try
            {
                if (input.TrimStart().StartsWith("["))
                {
                    var items = JsonConvert.DeserializeObject<List<string>>(input) ?? new List<string>();
                    foreach (var s in items)
                    {
                        list.Add(ParseEnumStrict<TEnum>(s));
                    }
                    return list;
                }
            }
            catch { /* ignore and try CSV */ }
            // CSV path
            foreach (var part in input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                list.Add(ParseEnumStrict<TEnum>(part));
            }
            return list;
        }

        private static List<TEnum> ParseEnumListFromJArray<TEnum>(JArray arr) where TEnum : struct
        {
            var list = new List<TEnum>();
            if (arr == null) return list;
            foreach (var token in arr)
            {
                var s = token?.ToString();
                list.Add(ParseEnumStrict<TEnum>(s));
            }
            return list;
        }

        private static TEnum[] ParseEnumArrayFromString<TEnum>(string input) where TEnum : struct
        {
            return ParseEnumListFromString<TEnum>(input).ToArray();
        }

        private static TEnum[] ParseEnumArrayFromJArray<TEnum>(JArray arr) where TEnum : struct
        {
            return ParseEnumListFromJArray<TEnum>(arr).ToArray();
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