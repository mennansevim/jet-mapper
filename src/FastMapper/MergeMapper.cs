using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastMapper
{
    /// <summary>
    /// Partial Merge ve Merge Strategy özellikleri
    /// </summary>
    public static class MergeMapper
    {
        private static readonly ConcurrentDictionary<Type, PropertyMergeInfo[]> _propertyMergeCache = new();
        private static readonly ConcurrentDictionary<long, object> _mergeStrategyCache = new();

        /// <summary>
        /// Merge stratejisi
        /// </summary>
        public enum MergeStrategy
        {
            Replace,        // Tüm değerleri değiştir
            Merge,          // Null olmayan değerlerle güncelle
            Append,         // Mevcut değerlere ekle
            Conditional,    // Koşullu güncelleme
            DeepMerge,      // Derinlemesine merge
            Selective       // Seçici merge
        }

        /// <summary>
        /// Merge sonucu
        /// </summary>
        public class MergeResult<T>
        {
            public T MergedObject { get; set; }
            public int UpdatedProperties { get; set; }
            public int SkippedProperties { get; set; }
            public List<string> UpdatedPropertyNames { get; set; } = new();
            public List<string> SkippedPropertyNames { get; set; } = new();
            public TimeSpan MergeTime { get; set; }
            public Dictionary<string, object> MergeMetrics { get; set; } = new();
        }

        /// <summary>
        /// Property merge bilgisi
        /// </summary>
        private class PropertyMergeInfo
        {
            public string PropertyName { get; set; }
            public Type PropertyType { get; set; }
            public Func<object, object> Getter { get; set; }
            public Action<object, object> Setter { get; set; }
            public bool IsCollection { get; set; }
            public bool IsComplexType { get; set; }
            public bool IsNullable { get; set; }
            public Type ElementType { get; set; }
        }

        /// <summary>
        /// Merge konfigürasyonu
        /// </summary>
        public class MergeConfiguration
        {
            public MergeStrategy Strategy { get; set; } = MergeStrategy.Merge;
            public string[] IncludeProperties { get; set; }
            public string[] ExcludeProperties { get; set; }
            public Dictionary<string, Func<object, object, object>> CustomMergeRules { get; set; } = new();
            public Func<object, object, bool> Condition { get; set; }
            public bool DeepMerge { get; set; }
            public int MaxDepth { get; set; } = 5;
        }

        /// <summary>
        /// Nesneleri merge et
        /// </summary>
        public static MergeResult<T> Merge<T>(T target, object source, MergeConfiguration config = null)
        {
            if (target == null || source == null)
                return CreateEmptyMergeResult<T>();

            var startTime = DateTime.UtcNow;
            var result = new MergeResult<T>();

            try
            {
                config ??= new MergeConfiguration();

                var targetType = typeof(T);
                var sourceType = source.GetType();
                var propertyInfos = GetPropertyMergeInfo(targetType);

                foreach (var propInfo in propertyInfos)
                {
                    if (ShouldSkipProperty(propInfo, config))
                    {
                        result.SkippedProperties++;
                        result.SkippedPropertyNames.Add(propInfo.PropertyName);
                        continue;
                    }

                    var sourceValue = GetSourceValue(source, propInfo);
                    var targetValue = propInfo.Getter(target);

                    if (ShouldUpdateProperty(sourceValue, targetValue, config))
                    {
                        var mergedValue = MergePropertyValue(sourceValue, targetValue, propInfo, config);
                        propInfo.Setter(target, mergedValue);

                        result.UpdatedProperties++;
                        result.UpdatedPropertyNames.Add(propInfo.PropertyName);
                    }
                    else
                    {
                        result.SkippedProperties++;
                        result.SkippedPropertyNames.Add(propInfo.PropertyName);
                    }
                }

                result.MergedObject = target;
                result.MergeTime = DateTime.UtcNow - startTime;
                result.MergeMetrics["Strategy"] = config.Strategy.ToString();
                result.MergeMetrics["DeepMerge"] = config.DeepMerge;
                result.MergeMetrics["MaxDepth"] = config.MaxDepth;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Merge işlemi başarısız: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// Kısmi merge (sadece belirli property'ler)
        /// </summary>
        public static MergeResult<T> PartialMerge<T>(T target, object source, params string[] propertyNames)
        {
            var config = new MergeConfiguration
            {
                Strategy = MergeStrategy.Replace,
                IncludeProperties = propertyNames
            };

            return Merge(target, source, config);
        }

        /// <summary>
        /// Koşullu merge
        /// </summary>
        public static MergeResult<T> ConditionalMerge<T>(T target, object source, Func<object, object, bool> condition)
        {
            var config = new MergeConfiguration
            {
                Strategy = MergeStrategy.Conditional,
                Condition = condition
            };

            return Merge(target, source, config);
        }

        /// <summary>
        /// Deep merge
        /// </summary>
        public static MergeResult<T> DeepMerge<T>(T target, object source, int maxDepth = 5)
        {
            var config = new MergeConfiguration
            {
                Strategy = MergeStrategy.DeepMerge,
                DeepMerge = true,
                MaxDepth = maxDepth
            };

            return Merge(target, source, config);
        }

        /// <summary>
        /// Append merge (koleksiyonlar için)
        /// </summary>
        public static MergeResult<T> AppendMerge<T>(T target, object source)
        {
            var config = new MergeConfiguration
            {
                Strategy = MergeStrategy.Append
            };

            return Merge(target, source, config);
        }

        /// <summary>
        /// Seçici merge
        /// </summary>
        public static MergeResult<T> SelectiveMerge<T>(T target, object source, 
            Dictionary<string, Func<object, object, object>> customRules)
        {
            var config = new MergeConfiguration
            {
                Strategy = MergeStrategy.Selective,
                CustomMergeRules = customRules
            };

            return Merge(target, source, config);
        }

        /// <summary>
        /// Property'yi atla mı kontrol et
        /// </summary>
        private static bool ShouldSkipProperty(PropertyMergeInfo propInfo, MergeConfiguration config)
        {
            // Include properties kontrolü
            if (config.IncludeProperties != null && config.IncludeProperties.Length > 0)
            {
                if (!config.IncludeProperties.Contains(propInfo.PropertyName, StringComparer.OrdinalIgnoreCase))
                    return true;
            }

            // Exclude properties kontrolü
            if (config.ExcludeProperties != null && config.ExcludeProperties.Length > 0)
            {
                if (config.ExcludeProperties.Contains(propInfo.PropertyName, StringComparer.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Property'yi güncelle mı kontrol et
        /// </summary>
        private static bool ShouldUpdateProperty(object sourceValue, object targetValue, MergeConfiguration config)
        {
            switch (config.Strategy)
            {
                case MergeStrategy.Replace:
                    return true;

                case MergeStrategy.Merge:
                    return sourceValue != null;

                case MergeStrategy.Conditional:
                    return config.Condition?.Invoke(sourceValue, targetValue) ?? true;

                case MergeStrategy.Append:
                    return sourceValue != null && targetValue != null;

                case MergeStrategy.DeepMerge:
                    return sourceValue != null;

                case MergeStrategy.Selective:
                    return sourceValue != null;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Property değerini merge et
        /// </summary>
        private static object MergePropertyValue(object sourceValue, object targetValue, 
            PropertyMergeInfo propInfo, MergeConfiguration config, int depth = 0)
        {
            if (depth >= config.MaxDepth)
                return targetValue;

            // Custom merge rule kontrolü
            if (config.CustomMergeRules.TryGetValue(propInfo.PropertyName, out var customRule))
            {
                return customRule(sourceValue, targetValue);
            }

            switch (config.Strategy)
            {
                case MergeStrategy.Replace:
                    return sourceValue;

                case MergeStrategy.Merge:
                    return sourceValue ?? targetValue;

                case MergeStrategy.Append:
                    return AppendValues(sourceValue, targetValue, propInfo);

                case MergeStrategy.DeepMerge:
                    return DeepMergeValues(sourceValue, targetValue, propInfo, config, depth + 1);

                case MergeStrategy.Selective:
                    return sourceValue ?? targetValue;

                default:
                    return sourceValue;
            }
        }

        /// <summary>
        /// Değerleri ekle (koleksiyonlar için)
        /// </summary>
        private static object AppendValues(object sourceValue, object targetValue, PropertyMergeInfo propInfo)
        {
            if (!propInfo.IsCollection)
                return sourceValue ?? targetValue;

            if (sourceValue == null) return targetValue;
            if (targetValue == null) return sourceValue;

            try
            {
                // Generic koleksiyon ekleme
                var sourceList = sourceValue as System.Collections.IEnumerable;
                var targetList = targetValue as System.Collections.IEnumerable;

                if (sourceList != null && targetList != null)
                {
                    var combined = new List<object>();
                    
                    // Target elemanlarını ekle
                    foreach (var item in targetList)
                        combined.Add(item);
                    
                    // Source elemanlarını ekle
                    foreach (var item in sourceList)
                        combined.Add(item);

                    return combined;
                }
            }
            catch
            {
                // Hata durumunda source değerini döndür
                return sourceValue;
            }

            return sourceValue;
        }

        /// <summary>
        /// Derinlemesine merge
        /// </summary>
        private static object DeepMergeValues(object sourceValue, object targetValue, 
            PropertyMergeInfo propInfo, MergeConfiguration config, int depth)
        {
            if (!propInfo.IsComplexType || depth >= config.MaxDepth)
                return sourceValue ?? targetValue;

            if (sourceValue == null) return targetValue;
            if (targetValue == null) return sourceValue;

            try
            {
                // Recursive merge
                var sourceType = sourceValue.GetType();
                var targetType = targetValue.GetType();

                if (sourceType == targetType)
                {
                    var mergedObject = Activator.CreateInstance(sourceType);
                    var nestedConfig = new MergeConfiguration
                    {
                        Strategy = config.Strategy,
                        DeepMerge = true,
                        MaxDepth = config.MaxDepth
                    };

                    var nestedResult = Merge(mergedObject, sourceValue, nestedConfig);
                    return nestedResult.MergedObject;
                }
            }
            catch
            {
                // Hata durumunda source değerini döndür
                return sourceValue;
            }

            return sourceValue;
        }

        /// <summary>
        /// Source değerini al
        /// </summary>
        private static object GetSourceValue(object source, PropertyMergeInfo propInfo)
        {
            try
            {
                var sourceType = source.GetType();
                var sourceProperty = sourceType.GetProperty(propInfo.PropertyName);

                if (sourceProperty?.CanRead == true)
                {
                    return sourceProperty.GetValue(source);
                }
            }
            catch
            {
                // Property bulunamadı veya okunamadı
            }

            return null;
        }

        /// <summary>
        /// Property merge bilgilerini al
        /// </summary>
        private static PropertyMergeInfo[] GetPropertyMergeInfo(Type type)
        {
            if (_propertyMergeCache.TryGetValue(type, out var cached))
                return cached;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var infos = new PropertyMergeInfo[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                infos[i] = new PropertyMergeInfo
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.PropertyType,
                    Getter = prop.CanRead ? CreateGetter(prop) : null,
                    Setter = prop.CanWrite ? CreateSetter(prop) : null,
                    IsCollection = IsCollectionType(prop.PropertyType),
                    IsComplexType = IsComplexType(prop.PropertyType),
                    IsNullable = IsNullableType(prop.PropertyType),
                    ElementType = IsCollectionType(prop.PropertyType) ? GetCollectionElementType(prop.PropertyType) : null
                };
            }

            _propertyMergeCache[type] = infos;
            return infos;
        }

        /// <summary>
        /// Property getter oluştur
        /// </summary>
        private static Func<object, object> CreateGetter(PropertyInfo property)
        {
            return obj => property.GetValue(obj);
        }

        /// <summary>
        /// Property setter oluştur
        /// </summary>
        private static Action<object, object> CreateSetter(PropertyInfo property)
        {
            return (obj, value) => property.SetValue(obj, value);
        }

        /// <summary>
        /// Collection tip kontrolü
        /// </summary>
        private static bool IsCollectionType(Type type)
        {
            return type != typeof(string) && 
                   typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// Karmaşık tip kontrolü
        /// </summary>
        private static bool IsComplexType(Type type)
        {
            return !type.IsPrimitive && type != typeof(string) && type != typeof(DateTime) && 
                   type != typeof(Guid) && !type.IsEnum;
        }

        /// <summary>
        /// Nullable tip kontrolü
        /// </summary>
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Collection element tipini al
        /// </summary>
        private static Type GetCollectionElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            
            var genericArgs = type.GetGenericArguments();
            return genericArgs.Length > 0 ? genericArgs[0] : typeof(object);
        }

        /// <summary>
        /// Boş merge sonucu oluştur
        /// </summary>
        private static MergeResult<T> CreateEmptyMergeResult<T>()
        {
            return new MergeResult<T>
            {
                MergedObject = default(T),
                UpdatedProperties = 0,
                SkippedProperties = 0,
                MergeTime = TimeSpan.Zero
            };
        }

        /// <summary>
        /// Merge cache'ini temizle
        /// </summary>
        public static void ClearMergeCache()
        {
            _propertyMergeCache.Clear();
            _mergeStrategyCache.Clear();
        }

        /// <summary>
        /// Merge istatistiklerini al
        /// </summary>
        public static (int PropertyCache, int StrategyCache) GetMergeStats()
        {
            return (_propertyMergeCache.Count, _mergeStrategyCache.Count);
        }
    }
} 