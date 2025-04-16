using System;
using System.Collections.Generic;

namespace FastMapper
{
    /// <summary>
    /// Özel eşleştirme kuralları tanımlamak için kullanılan profil sınıfı
    /// </summary>
    public class MapperProfile
    {
        private readonly string _name;
        private readonly Dictionary<string, Func<object, object>> _customMappings = new Dictionary<string, Func<object, object>>();

        internal MapperProfile(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Özel bir eşleştirme kuralı tanımlar
        /// </summary>
        /// <typeparam name="TSource">Kaynak nesne tipi</typeparam>
        /// <typeparam name="TTarget">Hedef nesne tipi</typeparam>
        /// <typeparam name="TProperty">Hedef property tipi</typeparam>
        /// <param name="targetPropertyName">Hedef property adı</param>
        /// <param name="mappingFunc">Eşleştirme için kullanılacak fonksiyon</param>
        /// <returns>Aynı profil için metot zinciri oluşturmak üzere profil nesnesi</returns>
        public MapperProfile MapProperty<TSource, TTarget, TProperty>(
            string targetPropertyName,
            Func<TSource, TProperty> mappingFunc)
        {
            if (targetPropertyName == null)
                throw new ArgumentNullException(nameof(targetPropertyName));
            if (mappingFunc == null)
                throw new ArgumentNullException(nameof(mappingFunc));

            var key = GetMappingKey(typeof(TSource), typeof(TTarget), targetPropertyName);
            _customMappings[key] = src => mappingFunc((TSource)src);

            return this;
        }

        internal bool HasCustomMapping(Type sourceType, Type targetType, string targetPropertyName)
        {
            var key = GetMappingKey(sourceType, targetType, targetPropertyName);
            return _customMappings.ContainsKey(key);
        }

        internal Func<object, object> GetCustomMapping(Type sourceType, Type targetType, string targetPropertyName)
        {
            var key = GetMappingKey(sourceType, targetType, targetPropertyName);
            return _customMappings.TryGetValue(key, out var func) ? func : null;
        }

        private string GetMappingKey(Type sourceType, Type targetType, string targetPropertyName)
        {
            return $"{sourceType.FullName}_{targetType.FullName}_{targetPropertyName}";
        }
    }
} 