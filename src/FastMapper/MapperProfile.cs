using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastMapper
{
    /// <summary>
    /// ULTRA-PERFORMANS MapperProfile - Zero overhead caching, unsafe operations
    /// </summary>
    public static class MapperProfile
    {
        // Ultra-fast compiled mapping cache - never regenerate
        private static readonly ConcurrentDictionary<long, object> _compiledMappingCache = new();
        
        // Lightning-fast property metadata cache
        private static readonly ConcurrentDictionary<Type, UltraPropertyInfo[]> _propertyInfoCache = new();
        
        // Type metadata cache for instant lookups
        private static readonly ConcurrentDictionary<Type, UltraTypeInfo> _typeInfoCache = new();
        
        // Pre-compiled constructor cache
        private static readonly ConcurrentDictionary<Type, Func<object>> _constructorCache = new();
        
        // Custom type mapping rules
        private static readonly ConcurrentDictionary<long, Delegate> _customTypeMappers = new();

        private struct UltraPropertyInfo
        {
            public string Name;
            public Type PropertyType;
            public int NameHash;
            public Func<object, object> UltraGetter;
            public Action<object, object> UltraSetter;
            public bool CanRead;
            public bool CanWrite;
            public bool IsValueType;
            public bool IsNullable;
            public bool IsPrimitive;
            public byte PropertyIndex; // For ultra-fast array indexing
        }

        private struct UltraTypeInfo
        {
            public Type Type;
            public Func<object> UltraConstructor;
            public bool IsValueType;
            public bool IsNullable;
            public bool IsPrimitive;
            public bool IsString;
            public bool IsEnum;
            public bool IsCollection;
            public Type ElementType; // For collections
            public int PropertyCount;
            public long TypeHash; // For ultra-fast type comparison
        }

        /// <summary>
        /// Configure custom mapping between types
        /// </summary>
        public static void CreateMap<TSource, TTarget>()
            where TTarget : new()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            var typeKey = GetUltraTypeKey(sourceType, targetType);
            
            if (!_compiledMappingCache.ContainsKey(typeKey))
            {
                var mapper = CreateUltraCompiledMapper<TSource, TTarget>();
                _compiledMappingCache[typeKey] = mapper;
            }
        }

        /// <summary>
        /// Configure custom mapping with expression
        /// </summary>
        public static void CreateMap<TSource, TTarget>(Expression<Func<TSource, TTarget>> mappingExpression)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            var typeKey = GetUltraTypeKey(sourceType, targetType);
            
            var compiledMapper = mappingExpression.Compile();
            _compiledMappingCache[typeKey] = compiledMapper;
        }

        /// <summary>
        /// Add custom type converter for specific property types
        /// </summary>
        public static void AddTypeConverter<TSource, TTarget>(Func<TSource, TTarget> converter)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            var typeKey = GetUltraTypeKey(sourceType, targetType);
            
            _customTypeMappers[typeKey] = converter;
        }

        /// <summary>
        /// Get ultra-fast compiled mapper for types
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<TSource, TTarget> GetUltraCompiledMapper<TSource, TTarget>()
            where TTarget : new()
        {
            var typeKey = GetUltraTypeKey(typeof(TSource), typeof(TTarget));
            
            if (_compiledMappingCache.TryGetValue(typeKey, out var cached))
            {
                return (Func<TSource, TTarget>)cached;
            }
            
            var mapper = CreateUltraCompiledMapper<TSource, TTarget>();
            _compiledMappingCache[typeKey] = mapper;
            return mapper;
        }

        /// <summary>
        /// Create ultra-compiled mapper with maximum performance optimizations
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<TSource, TTarget> CreateUltraCompiledMapper<TSource, TTarget>()
            where TTarget : new()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            
            // Get ultra-fast type metadata
            var sourceInfo = GetOrCreateUltraTypeInfo(sourceType);
            var targetInfo = GetOrCreateUltraTypeInfo(targetType);
            var sourceProps = GetOrCreateUltraPropertyInfo(sourceType);
            var targetProps = GetOrCreateUltraPropertyInfo(targetType);
            
            // Build ultra-fast property mapping lookup table
            var mappingTable = CreateUltraFastMappingTable(sourceProps, targetProps);
            
            // Create ultra-optimized mapper function
            return (TSource source) =>
            {
                if (source == null) return default(TTarget);
                
                // Ultra-fast object creation
                var target = (TTarget)targetInfo.UltraConstructor();
                
                // Ultra-fast property mapping with lookup table
                var sourceObj = (object)source;
                var targetObj = (object)target;
                
                // Optimized tight loop with pre-calculated mappings
                for (int i = 0; i < mappingTable.Length; i++)
                {
                    var mapping = mappingTable[i];
                    if (mapping.IsValid)
                    {
                        // Ultra-fast property access with pre-compiled delegates
                        var value = mapping.SourceGetter(sourceObj);
                        
                        if (value != null || !mapping.TargetIsValueType)
                        {
                            // Handle type conversion if needed
                            if (mapping.RequiresConversion)
                            {
                                value = UltraFastConvert(value, mapping.SourceType, mapping.TargetType);
                            }
                            
                            mapping.TargetSetter(targetObj, value);
                        }
                    }
                }
                
                return target;
            };
        }

        private struct UltraMapping
        {
            public bool IsValid;
            public bool RequiresConversion;
            public bool TargetIsValueType;
            public Type SourceType;
            public Type TargetType;
            public Func<object, object> SourceGetter;
            public Action<object, object> TargetSetter;
        }

        /// <summary>
        /// Create ultra-fast mapping table for maximum performance
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static UltraMapping[] CreateUltraFastMappingTable(
            UltraPropertyInfo[] sourceProps, 
            UltraPropertyInfo[] targetProps)
        {
            // Create hash lookup for target properties - O(1) access
            var targetLookup = new Dictionary<int, UltraPropertyInfo>(targetProps.Length);
            for (int i = 0; i < targetProps.Length; i++)
            {
                var prop = targetProps[i];
                if (prop.CanWrite)
                    targetLookup[prop.NameHash] = prop;
            }
            
            var mappings = new List<UltraMapping>(sourceProps.Length);
            
            // Build mapping table with ultra-fast lookups
            for (int i = 0; i < sourceProps.Length; i++)
            {
                var sourceProp = sourceProps[i];
                if (!sourceProp.CanRead) continue;
                
                if (targetLookup.TryGetValue(sourceProp.NameHash, out var targetProp) &&
                    sourceProp.Name == targetProp.Name) // Double-check for hash collisions
                {
                    mappings.Add(new UltraMapping
                    {
                        IsValid = true,
                        RequiresConversion = sourceProp.PropertyType != targetProp.PropertyType,
                        TargetIsValueType = targetProp.IsValueType,
                        SourceType = sourceProp.PropertyType,
                        TargetType = targetProp.PropertyType,
                        SourceGetter = sourceProp.UltraGetter,
                        TargetSetter = targetProp.UltraSetter
                    });
                }
            }
            
            return mappings.ToArray();
        }

        /// <summary>
        /// Get or create ultra-fast type information
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static UltraTypeInfo GetOrCreateUltraTypeInfo(Type type)
        {
            if (_typeInfoCache.TryGetValue(type, out var cached))
                return cached;
            
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var info = new UltraTypeInfo
            {
                Type = type,
                UltraConstructor = GetOrCreateUltraConstructor(type),
                IsValueType = type.IsValueType,
                IsNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>),
                IsPrimitive = type.IsPrimitive,
                IsString = type == typeof(string),
                IsEnum = type.IsEnum,
                IsCollection = IsCollectionType(type),
                ElementType = GetCollectionElementType(type),
                PropertyCount = properties.Length,
                TypeHash = (long)type.GetHashCode()
            };
            
            _typeInfoCache[type] = info;
            return info;
        }

        /// <summary>
        /// Get or create ultra-fast property information
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static UltraPropertyInfo[] GetOrCreateUltraPropertyInfo(Type type)
        {
            if (_propertyInfoCache.TryGetValue(type, out var cached))
                return cached;
            
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propInfos = new UltraPropertyInfo[properties.Length];
            
            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                propInfos[i] = new UltraPropertyInfo
                {
                    Name = prop.Name,
                    NameHash = prop.Name.GetHashCode(),
                    PropertyType = prop.PropertyType,
                    PropertyIndex = (byte)i,
                    CanRead = prop.CanRead,
                    CanWrite = prop.CanWrite,
                    IsValueType = prop.PropertyType.IsValueType,
                    IsNullable = prop.PropertyType.IsGenericType && 
                                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>),
                    IsPrimitive = prop.PropertyType.IsPrimitive,
                    UltraGetter = prop.CanRead ? CreateUltraGetter(prop) : null,
                    UltraSetter = prop.CanWrite ? CreateUltraSetter(prop) : null
                };
            }
            
            _propertyInfoCache[type] = propInfos;
            return propInfos;
        }

        /// <summary>
        /// Get or create ultra-fast constructor
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<object> GetOrCreateUltraConstructor(Type type)
        {
            if (_constructorCache.TryGetValue(type, out var cached))
                return cached;
            
            var newExpression = Expression.New(type);
            var lambda = Expression.Lambda<Func<object>>(newExpression);
            var compiled = lambda.Compile();
            
            _constructorCache[type] = compiled;
            return compiled;
        }

        /// <summary>
        /// Create ultra-fast property getter with aggressive optimization
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Func<object, object> CreateUltraGetter(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var typedInstance = Expression.Convert(instance, property.DeclaringType);
            var propertyAccess = Expression.Property(typedInstance, property);
            var converted = Expression.Convert(propertyAccess, typeof(object));
            
            var lambda = Expression.Lambda<Func<object, object>>(converted, instance);
            return lambda.Compile();
        }

        /// <summary>
        /// Create ultra-fast property setter with aggressive optimization
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Action<object, object> CreateUltraSetter(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");
            
            var typedInstance = Expression.Convert(instance, property.DeclaringType);
            var typedValue = Expression.Convert(value, property.PropertyType);
            var propertySet = Expression.Call(typedInstance, property.GetSetMethod(), typedValue);
            
            var lambda = Expression.Lambda<Action<object, object>>(propertySet, instance, value);
            return lambda.Compile();
        }

        /// <summary>
        /// Ultra-fast type conversion with optimized paths
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object UltraFastConvert(object value, Type sourceType, Type targetType)
        {
            if (value == null) return null;
            if (sourceType == targetType) return value;
            if (targetType.IsAssignableFrom(sourceType)) return value;
            
            // Check for custom converter first
            var converterKey = GetUltraTypeKey(sourceType, targetType);
            if (_customTypeMappers.TryGetValue(converterKey, out var customConverter))
            {
                return ((Func<object, object>)customConverter)(value);
            }
            
            // Handle nullable types
            var targetNullable = Nullable.GetUnderlyingType(targetType);
            if (targetNullable != null)
            {
                return UltraFastConvert(value, sourceType, targetNullable);
            }
            
            // Fast paths for common conversions
            if (targetType == typeof(string))
                return value.ToString();
            
            if (sourceType == typeof(string) && targetType.IsPrimitive)
            {
                return Convert.ChangeType(value, targetType);
            }
            
            // Default conversion
            return Convert.ChangeType(value, targetType);
        }

        /// <summary>
        /// Ultra-fast type key generation for maximum performance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetUltraTypeKey(Type sourceType, Type targetType)
        {
            return ((long)sourceType.GetHashCode() << 32) | (uint)targetType.GetHashCode();
        }

        /// <summary>
        /// Check if type is a collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) && 
                   typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// Get collection element type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Type GetCollectionElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            
            var genericArgs = type.GetGenericArguments();
            return genericArgs.Length > 0 ? genericArgs[0] : typeof(object);
        }

        /// <summary>
        /// Clear all caches - for testing and memory management
        /// </summary>
        public static void ClearAllCaches()
        {
            _compiledMappingCache.Clear();
            _propertyInfoCache.Clear();
            _typeInfoCache.Clear();
            _constructorCache.Clear();
            _customTypeMappers.Clear();
        }

        /// <summary>
        /// Pre-warm caches for specific types to eliminate cold start
        /// </summary>
        public static void WarmUpCache<TSource, TTarget>()
            where TTarget : new()
        {
            CreateMap<TSource, TTarget>();
            GetOrCreateUltraTypeInfo(typeof(TSource));
            GetOrCreateUltraTypeInfo(typeof(TTarget));
            GetOrCreateUltraPropertyInfo(typeof(TSource));
            GetOrCreateUltraPropertyInfo(typeof(TTarget));
        }

        /// <summary>
        /// Get cache statistics for monitoring
        /// </summary>
        public static (int MappingCache, int PropertyCache, int TypeCache, int ConstructorCache) GetCacheStats()
        {
            return (
                _compiledMappingCache.Count,
                _propertyInfoCache.Count,
                _typeInfoCache.Count,
                _constructorCache.Count
            );
        }
    }
} 