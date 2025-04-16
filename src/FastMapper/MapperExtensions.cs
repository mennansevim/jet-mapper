using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Concurrent;

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
        
        // Property önbellekleri - tekrarlanan yansıma operasyonlarını azaltmak için
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> _targetPropertyCache = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _sourcePropertyCache = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();
        
        // Enum dönüşüm önbelleği
        private static readonly ConcurrentDictionary<string, object> _enumCache = new ConcurrentDictionary<string, object>();

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
            // Hedef tipin tüm property'lerini önbellekten al veya ekle
            var targetProperties = _targetPropertyCache.GetOrAdd(targetType, type => 
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToList());

            // Kaynak tipin tüm property'lerini önbellekten al veya ekle
            var sourceProperties = _sourcePropertyCache.GetOrAdd(sourceType, type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead)
                    .ToDictionary(p => p.Name, p => p));

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

                // Kaynak property'yi bul - aynı isimde bir özellik yoksa mevcut değeri koru
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
                // Eğer kaynak özelliği yoksa, hedef özelliğin değerini koruyalım - KODA EKLENDİ
                // (Eğer özel bir davranış istenmiyorsa bu şekilde kalabilir)
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
            
            // String-to-Enum dönüşümü için optimize edilmiş yol
            if (sourceType == typeof(string) && targetType.IsEnum)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    var cacheKey = $"{targetType.FullName}_{stringValue}";
                    
                    // Önbellekte varsa kullan
                    if (_enumCache.TryGetValue(cacheKey, out var cachedValue))
                    {
                        result = cachedValue;
                        return true;
                    }
                    
                    try
                    {
                        // Enum.Parse yöntemiyle çöz ve önbelleğe al
                        var parsedEnum = Enum.Parse(targetType, stringValue, true);
                        _enumCache.TryAdd(cacheKey, parsedEnum);
                        result = parsedEnum;
                        return true;
                    }
                    catch
                    {
                        // Çözümleme başarısız olduysa, sonucu null olarak belirle
                    }
                }
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
            if (value == null)
                return null;
                
            Type valueType = value.GetType();
            TypeCode sourceTypeCode = Type.GetTypeCode(valueType);
            TypeCode targetTypeCode = Type.GetTypeCode(targetType);
            
            // Aynı tip ise doğrudan dön
            if (valueType == targetType)
                return value;
                
            try
            {
                // En yaygın dönüşüm yolları için optimize edilmiş yöntemler
                switch (targetTypeCode)
                {
                    case TypeCode.Byte:
                        if (sourceTypeCode == TypeCode.Int32)
                        {
                            int intVal = (int)value;
                            if (intVal < byte.MinValue || intVal > byte.MaxValue)
                                return intVal < byte.MinValue ? byte.MinValue : byte.MaxValue;
                            return (byte)intVal;
                        }
                        return Convert.ToByte(value);
                        
                    case TypeCode.SByte:
                        if (sourceTypeCode == TypeCode.Int32)
                        {
                            int intVal = (int)value;
                            if (intVal < sbyte.MinValue || intVal > sbyte.MaxValue)
                                return intVal < sbyte.MinValue ? sbyte.MinValue : sbyte.MaxValue;
                            return (sbyte)intVal;
                        }
                        return Convert.ToSByte(value);
                        
                    case TypeCode.Int16:
                        if (sourceTypeCode == TypeCode.Int32)
                        {
                            int intVal = (int)value;
                            if (intVal < short.MinValue || intVal > short.MaxValue)
                                return intVal < short.MinValue ? short.MinValue : short.MaxValue;
                            return (short)intVal;
                        }
                        return Convert.ToInt16(value);
                        
                    case TypeCode.UInt16:
                        if (sourceTypeCode == TypeCode.Int32)
                        {
                            int intVal = (int)value;
                            if (intVal < ushort.MinValue || intVal > ushort.MaxValue)
                                return intVal < ushort.MinValue ? ushort.MinValue : ushort.MaxValue;
                            return (ushort)intVal;
                        }
                        return Convert.ToUInt16(value);
                        
                    case TypeCode.Int32:
                        if (sourceTypeCode == TypeCode.Int64)
                        {
                            long longVal = (long)value;
                            if (longVal < int.MinValue || longVal > int.MaxValue)
                                return longVal < int.MinValue ? int.MinValue : int.MaxValue;
                            return (int)longVal;
                        }
                        else if (sourceTypeCode == TypeCode.Double)
                        {
                            double doubleVal = (double)value;
                            if (doubleVal < int.MinValue || doubleVal > int.MaxValue)
                                return doubleVal < int.MinValue ? int.MinValue : int.MaxValue;
                            return (int)doubleVal;
                        }
                        else if (sourceTypeCode == TypeCode.Decimal)
                        {
                            decimal decimalVal = (decimal)value;
                            if (decimalVal < int.MinValue || decimalVal > int.MaxValue)
                                return decimalVal < int.MinValue ? int.MinValue : int.MaxValue;
                            return (int)decimalVal;
                        }
                        return Convert.ToInt32(value);
                        
                    case TypeCode.UInt32:
                        if (sourceTypeCode == TypeCode.Int32)
                        {
                            int intVal = (int)value;
                            if (intVal < 0)
                                return (uint)0;
                            return (uint)intVal;
                        }
                        return Convert.ToUInt32(value);
                        
                    case TypeCode.Int64:
                        return Convert.ToInt64(value);
                        
                    case TypeCode.UInt64:
                        return Convert.ToUInt64(value);
                        
                    case TypeCode.Single:
                        return Convert.ToSingle(value);
                        
                    case TypeCode.Double:
                        return Convert.ToDouble(value);
                        
                    case TypeCode.Decimal:
                        return Convert.ToDecimal(value);
                        
                    default:
                        return Convert.ChangeType(value, targetType);
                }
            }
            catch
            {
                // Taşma veya dönüşüm hataları durumunda en iyi yaklaşımı uygula
                try
                {
                    switch (targetTypeCode)
                    {
                        case TypeCode.Byte:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), byte.MinValue, byte.MaxValue, Convert.ToByte) : default(byte);
                        case TypeCode.SByte:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), sbyte.MinValue, sbyte.MaxValue, Convert.ToSByte) : default(sbyte);
                        case TypeCode.Int16:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), short.MinValue, short.MaxValue, Convert.ToInt16) : default(short);
                        case TypeCode.UInt16:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), ushort.MinValue, ushort.MaxValue, Convert.ToUInt16) : default(ushort);
                        case TypeCode.Int32:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), int.MinValue, int.MaxValue, Convert.ToInt32) : default(int);
                        case TypeCode.UInt32:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), uint.MinValue, uint.MaxValue, Convert.ToUInt32) : default(uint);
                        case TypeCode.Int64:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), long.MinValue, long.MaxValue, Convert.ToInt64) : default(long);
                        case TypeCode.UInt64:
                            return value is IConvertible ? ClampValue(Convert.ToDouble(value), ulong.MinValue, ulong.MaxValue, Convert.ToUInt64) : default(ulong);
                        default:
                            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                    }
                }
                catch
                {
                    // Son çare
                    return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                }
            }
        }
        
        private static T ClampValue<T>(double value, T min, T max, Func<double, T> converter)
        {
            if (Convert.ToDouble(min) > value)
                return min;
            if (Convert.ToDouble(max) < value)
                return max;
            return converter(value);
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

        /// <summary>
        /// Bir koleksiyondaki tüm öğeleri eşler
        /// </summary>
        /// <typeparam name="TSource">Kaynak öğe tipi</typeparam>
        /// <typeparam name="TTarget">Hedef öğe tipi</typeparam>
        /// <param name="source">Kaynak koleksiyon</param>
        /// <returns>Eşlenmiş hedef koleksiyonu</returns>
        public static List<TTarget> FastMapToList<TSource, TTarget>(this IEnumerable<TSource> source)
            where TTarget : new()
            where TSource : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<TTarget>();
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            
            // Tüm koleksiyonu tek geçişte işleyelim
            foreach (var sourceItem in source)
            {
                if (sourceItem == null)
                    continue;
                    
                var targetItem = new TTarget();
                CopyProperties(sourceItem, targetItem, sourceType, targetType);
                result.Add(targetItem);
            }
            
            return result;
        }
        
        /// <summary>
        /// Bir koleksiyondaki tüm öğeleri eldeki koleksiyona eşler
        /// </summary>
        /// <typeparam name="TSource">Kaynak öğe tipi</typeparam>
        /// <typeparam name="TTarget">Hedef öğe tipi</typeparam>
        /// <param name="source">Kaynak koleksiyon</param>
        /// <param name="destination">Hedef koleksiyon</param>
        /// <returns>Eşlenmiş hedef koleksiyonu</returns>
        public static List<TTarget> FastMapToList<TSource, TTarget>(this IEnumerable<TSource> source, List<TTarget> destination)
            where TTarget : new()
            where TSource : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
                
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            
            // Her bir öğeyi eşleyelim
            foreach (var sourceItem in source)
            {
                if (sourceItem == null)
                    continue;
                    
                var targetItem = new TTarget();
                CopyProperties(sourceItem, targetItem, sourceType, targetType);
                destination.Add(targetItem);
            }
            
            return destination;
        }
    }
} 