using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace FastMapper
{
    /// <summary>
    /// FastMapper kütüphanesi için uzantı metotları
    /// </summary>
    public static class MapperExtensions
    {
        // Özel eşleştirme işlevlerini saklamak için statik sözlük
        private static readonly Dictionary<string, Func<object, object>> _customMappings = new Dictionary<string, Func<object, object>>();
        
        // Özel tip dönüştürücüleri için sözlük
        private static readonly Dictionary<string, Func<object, object>> _typeConverters = new Dictionary<string, Func<object, object>>();

        /// <summary>
        /// Kaynak nesneyi hedef tipine eşler
        /// </summary>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <returns>Hedef tipinde yeni bir nesne</returns>
        public static TTarget FastMapTo<TTarget>(this object source) where TTarget : new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var targetType = typeof(TTarget);
            var sourceType = source.GetType();
            
            // Hedef tipinde yeni bir nesne oluştur
            var target = new TTarget();
            
            // Property'leri kopyala
            CopyProperties(source, target, sourceType, targetType);
            
            return target;
        }

        /// <summary>
        /// Kaynak nesnenin özelliklerini mevcut hedef nesnesine kopyalar
        /// </summary>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        /// <param name="source">Kaynak nesne</param>
        /// <param name="target">Hedef nesne</param>
        /// <returns>Güncellenen hedef nesne</returns>
        public static TTarget FastMapTo<TTarget>(this object source, TTarget target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var targetType = typeof(TTarget);
            var sourceType = source.GetType();
            
            // Property'leri kopyala
            CopyProperties(source, target, sourceType, targetType);
            
            return target;
        }

        /// <summary>
        /// Belirli bir özellik için özel bir eşleştirme kuralı tanımlar
        /// </summary>
        /// <typeparam name="TSource">Kaynak tip</typeparam>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        /// <typeparam name="TProperty">Özellik tipi</typeparam>
        /// <param name="targetPropertyName">Hedef özellik adı</param>
        /// <param name="mappingFunc">Eşleştirme fonksiyonu</param>
        public static void AddCustomMapping<TSource, TTarget, TProperty>(
            string targetPropertyName,
            Func<TSource, TProperty> mappingFunc)
        {
            if (string.IsNullOrEmpty(targetPropertyName))
                throw new ArgumentNullException(nameof(targetPropertyName));
            if (mappingFunc == null)
                throw new ArgumentNullException(nameof(mappingFunc));

            var key = GetMappingKey(typeof(TSource), typeof(TTarget), targetPropertyName);
            _customMappings[key] = src => mappingFunc((TSource)src);
        }

        /// <summary>
        /// Belirli bir özellik için özel bir eşleştirme kuralını kaldırır
        /// </summary>
        /// <typeparam name="TSource">Kaynak tip</typeparam>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        /// <param name="targetPropertyName">Hedef özellik adı</param>
        public static void RemoveCustomMapping<TSource, TTarget>(string targetPropertyName)
        {
            var key = GetMappingKey(typeof(TSource), typeof(TTarget), targetPropertyName);
            if (_customMappings.ContainsKey(key))
            {
                _customMappings.Remove(key);
            }
        }

        /// <summary>
        /// Tüm özel eşleştirme kurallarını temizler
        /// </summary>
        public static void ClearAllCustomMappings()
        {
            _customMappings.Clear();
        }
        
        /// <summary>
        /// Özel tip dönüşümü tanımlar
        /// </summary>
        /// <typeparam name="TSource">Kaynak tip</typeparam>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        /// <param name="conversionFunc">Dönüşüm fonksiyonu</param>
        public static void AddTypeConverter<TSource, TTarget>(Func<TSource, TTarget> conversionFunc)
        {
            if (conversionFunc == null)
                throw new ArgumentNullException(nameof(conversionFunc));

            var key = GetTypeConverterKey(typeof(TSource), typeof(TTarget));
            _typeConverters[key] = obj => conversionFunc((TSource)obj);
        }
        
        /// <summary>
        /// Özel tip dönüşümünü kaldırır
        /// </summary>
        /// <typeparam name="TSource">Kaynak tip</typeparam>
        /// <typeparam name="TTarget">Hedef tip</typeparam>
        public static void RemoveTypeConverter<TSource, TTarget>()
        {
            var key = GetTypeConverterKey(typeof(TSource), typeof(TTarget));
            if (_typeConverters.ContainsKey(key))
            {
                _typeConverters.Remove(key);
            }
        }
        
        /// <summary>
        /// Tüm özel tip dönüşümlerini temizler
        /// </summary>
        public static void ClearAllTypeConverters()
        {
            _typeConverters.Clear();
        }

        private static void CopyProperties(object source, object target, Type sourceType, Type targetType)
        {
            // Hedef tipin tüm property'lerini al
            var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            // Kaynak tipin tüm property'lerini al
            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p);

            foreach (var targetProp in targetProperties)
            {
                // Özel eşleştirme var mı kontrol et
                var customMappingKey = GetMappingKey(sourceType, targetType, targetProp.Name);
                if (_customMappings.TryGetValue(customMappingKey, out var customMapping))
                {
                    // Özel eşleştirme kullan
                    var value = customMapping(source);
                    targetProp.SetValue(target, value);
                    continue;
                }

                // Kaynak property'yi bul
                if (sourceProperties.TryGetValue(targetProp.Name, out var sourceProp))
                {
                    try
                    {
                        // Değeri al
                        var value = sourceProp.GetValue(source);

                        // Tipler uyuşuyor mu kontrol et
                        if (value != null)
                        {
                            // Direkt atama yapılabilir mi?
                            if (targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                            {
                                targetProp.SetValue(target, value);
                            }
                            // Özel tip dönüştürücü var mı?
                            else if (TryUseTypeConverter(value, sourceProp.PropertyType, targetProp.PropertyType, out var convertedValue))
                            {
                                targetProp.SetValue(target, convertedValue);
                            }
                            // Dönüşüm gerekiyor mu?
                            else
                            {
                                try
                                {
                                    // TimeSpan için özel destek
                                    if (sourceProp.PropertyType == typeof(TimeSpan) && targetProp.PropertyType == typeof(long))
                                    {
                                        var timeSpan = (TimeSpan)value;
                                        targetProp.SetValue(target, (long)timeSpan.TotalMilliseconds);
                                        continue;
                                    }
                                    
                                    // Guid için özel destek
                                    if (sourceProp.PropertyType == typeof(Guid) && targetProp.PropertyType == typeof(string))
                                    {
                                        var guid = (Guid)value;
                                        targetProp.SetValue(target, guid.ToString());
                                        continue;
                                    }
                                    
                                    // Sayısal tipler arası dönüşüm için özel kontrol
                                    if (IsNumericType(targetProp.PropertyType) && IsNumericType(sourceProp.PropertyType))
                                    {
                                        // Kaynak değer hedef tipe sığıyor mu kontrol et
                                        var targetValue = SafeNumericConvert(value, targetProp.PropertyType);
                                        targetProp.SetValue(target, targetValue);
                                    }
                                    else
                                    {
                                        // Convert.ChangeType kullanarak dönüşüm dene
                                        var changeTypeObj = Convert.ChangeType(value, targetProp.PropertyType);
                                        targetProp.SetValue(target, changeTypeObj);
                                    }
                                }
                                catch
                                {
                                    // Dönüşüm başarısız oldu, eğer nesne tipi uyuşuyorsa özyinelemeli map yapabilir miyiz?
                                    if (!sourceProp.PropertyType.IsPrimitive && !targetProp.PropertyType.IsPrimitive)
                                    {
                                        // Karmaşık tipleri ve koleksiyonları ele almak için gelişmiş işleme
                                        HandleComplexTypeMapping(source, target, sourceProp, targetProp);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Hata durumunda bu property'yi atla
                    }
                }
            }
        }
        
        private static bool TryUseTypeConverter(object value, Type sourceType, Type targetType, out object result)
        {
            var key = GetTypeConverterKey(sourceType, targetType);
            if (_typeConverters.TryGetValue(key, out var converter))
            {
                result = converter(value);
                return true;
            }
            
            result = null;
            return false;
        }
        
        private static bool IsNumericType(Type type)
        {
            if (type == null) return false;
            
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
        
        private static object SafeNumericConvert(object value, Type targetType)
        {
            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Byte:
                    return Convert.ToByte(value);
                case TypeCode.SByte:
                    return Convert.ToSByte(value);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value);
                case TypeCode.Int16:
                    return Convert.ToInt16(value);
                case TypeCode.Int32:
                    // Int32 için taşma durumunu kontrol et
                    if (value is long longValue)
                    {
                        if (longValue > int.MaxValue)
                            return int.MaxValue;
                        if (longValue < int.MinValue)
                            return int.MinValue;
                    }
                    else if (value is double doubleValue)
                    {
                        if (doubleValue > int.MaxValue)
                            return int.MaxValue;
                        if (doubleValue < int.MinValue)
                            return int.MinValue;
                    }
                    else if (value is decimal decimalValue)
                    {
                        if (decimalValue > int.MaxValue)
                            return int.MaxValue;
                        if (decimalValue < int.MinValue)
                            return int.MinValue;
                    }
                    return Convert.ToInt32(value);
                case TypeCode.Int64:
                    return Convert.ToInt64(value);
                case TypeCode.Decimal:
                    return Convert.ToDecimal(value);
                case TypeCode.Double:
                    return Convert.ToDouble(value);
                case TypeCode.Single:
                    return Convert.ToSingle(value);
                default:
                    return Convert.ChangeType(value, targetType);
            }
        }

        private static string GetMappingKey(Type sourceType, Type targetType, string targetPropertyName)
        {
            return $"{sourceType.FullName}_{targetType.FullName}_{targetPropertyName}";
        }
        
        private static string GetTypeConverterKey(Type sourceType, Type targetType)
        {
            return $"{sourceType.FullName}_TO_{targetType.FullName}";
        }

        private static void HandleComplexTypeMapping(object source, object target, PropertyInfo sourceProp, PropertyInfo targetProp)
        {
            var sourceValue = sourceProp.GetValue(source);
            if (sourceValue == null) return;

            // Kompleks nesne türlerini özyinelemeli maplemek için FastMapTo yöntemini kullan
            if (targetProp.PropertyType.IsClass && !targetProp.PropertyType.IsArray && !IsCollectionType(targetProp.PropertyType))
            {
                // Tek bir kompleks nesne
                var mapMethod = typeof(MapperExtensions).GetMethod("FastMapTo", new[] { typeof(object) })?.MakeGenericMethod(targetProp.PropertyType);
                if (mapMethod != null)
                {
                    try
                    {
                        var mappedValue = mapMethod.Invoke(null, new[] { sourceValue });
                        targetProp.SetValue(target, mappedValue);
                    }
                    catch
                    {
                        // Özyinelemeli mapleme başarısız oldu, es geç
                    }
                }
            }
            // Koleksiyonları ele al
            else if (IsCollectionType(sourceValue.GetType()) && IsCollectionType(targetProp.PropertyType))
            {
                // Koleksiyonlar için mapleme
                var sourceCollection = sourceValue as System.Collections.IEnumerable;
                if (sourceCollection != null)
                {
                    // Hedef koleksiyon tipini belirle ve oluştur
                    Type targetItemType = GetCollectionItemType(targetProp.PropertyType);
                    if (targetItemType != null)
                    {
                        // Dinamik olarak hedef koleksiyon yöntemini çağır
                        var mapCollectionMethod = typeof(MapperExtensions).GetMethod("MapCollection", BindingFlags.NonPublic | BindingFlags.Static)
                            ?.MakeGenericMethod(targetItemType);
                        if (mapCollectionMethod != null)
                        {
                            var mappedCollection = mapCollectionMethod.Invoke(null, new[] { sourceCollection });
                            targetProp.SetValue(target, mappedCollection);
                        }
                    }
                }
            }
        }

        private static Type GetCollectionItemType(Type collectionType)
        {
            // Koleksiyon tipinin içindeki öğe tipini bulma
            foreach (Type interfaceType in collectionType.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }
            
            // IEnumerable<T> bulunamadı ama yine de bu bir tür koleksiyon olabilir
            if (collectionType.IsGenericType &&
                collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return collectionType.GetGenericArguments()[0];
            }
            
            // List<T> veya benzeri olabilir
            if (collectionType.IsGenericType &&
                (collectionType.GetGenericTypeDefinition() == typeof(List<>) ||
                 collectionType.GetGenericTypeDefinition() == typeof(IList<>) ||
                 collectionType.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                return collectionType.GetGenericArguments()[0];
            }

            return null;
        }

        private static bool IsCollectionType(Type type)
        {
            return 
                type.IsArray || 
                (type.IsGenericType && (
                    type.GetGenericTypeDefinition() == typeof(List<>) ||
                    type.GetGenericTypeDefinition() == typeof(IList<>) ||
                    type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                    type.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        private static IList<T> MapCollection<T>(System.Collections.IEnumerable source) where T : new()
        {
            var result = new List<T>();
            foreach (var sourceItem in source)
            {
                if (sourceItem != null)
                {
                    var targetItem = sourceItem.FastMapTo<T>();
                    result.Add(targetItem);
                }
            }
            return result;
        }
    }
} 