using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FastMapper
{
    /// <summary>
    /// Fluent API için mapper sınıfı
    /// </summary>
    public class FluentMapper<TSource>
    {
        private readonly TSource _source;
        internal readonly Dictionary<string, Func<TSource, object>> _customMappings = new();
        internal readonly HashSet<string> _ignoredProperties = new();
        internal readonly Dictionary<string, Func<TSource, bool>> _conditionalMappings = new();
        internal readonly List<Action<TSource, object>> _beforeMapActions = new();
        internal readonly List<Action<TSource, object>> _afterMapActions = new();
        private Type _targetType;

        public FluentMapper(TSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Hedef tipi belirle ve mapping context'i döndür
        /// </summary>
        public MappingContext<TSource, TTarget> MapTo<TTarget>() where TTarget : new()
        {
            _targetType = typeof(TTarget);
            return new MappingContext<TSource, TTarget>(this);
        }

        /// <summary>
        /// Hedef nesneyi al (eğer varsa)
        /// </summary>
        internal object GetTargetObject()
        {
            // Bu method, mapping sırasında hedef nesneye erişim sağlar
            // Şimdilik null döndürüyoruz, gerçek implementasyonda hedef nesneye erişim gerekir
            return null;
        }

        /// <summary>
        /// Merge stratejisi belirle
        /// </summary>
        public FluentMapper<TSource> WithMergeStrategy(MergeStrategy strategy)
        {
            // TODO: Implement merge strategy
            return this;
        }

        /// <summary>
        /// Uyumluluk modu belirle
        /// </summary>
        public FluentMapper<TSource> WithCompatibilityMode(CompatibilityMode mode)
        {
            // TODO: Implement compatibility mode
            return this;
        }

        /// <summary>
        /// Yeni nesne oluşturarak mapping yap
        /// </summary>
        public TTarget To<TTarget>() where TTarget : new()
        {
            var target = new TTarget();
            return To(target);
        }

        /// <summary>
        /// Mevcut nesneye mapping yap
        /// </summary>
        public TTarget To<TTarget>(TTarget target)
        {
            // BeforeMap aksiyonlarını çalıştır
            foreach (var action in _beforeMapActions)
            {
                action(_source, target);
            }

            // Standart mapping
            _source.FastMapTo(target);

            // Özel mapping'leri uygula
            ApplyCustomMappings(target);

            // AfterMap aksiyonlarını çalıştır
            foreach (var action in _afterMapActions)
            {
                action(_source, target);
            }

            return target;
        }

        /// <summary>
        /// Asenkron liste mapping
        /// </summary>
        public async Task<List<TTarget>> ToListAsync<TTarget>(IEnumerable<TSource> sources) 
            where TTarget : new()
        {
            var results = new List<TTarget>();
            var tasks = new List<Task<TTarget>>();

            foreach (var source in sources)
            {
                var fluentMapper = new FluentMapper<TSource>(source);
                // Aynı konfigürasyonu uygula
                ApplyConfiguration(fluentMapper);
                tasks.Add(Task.Run(() => fluentMapper.To<TTarget>()));
            }

            var mappedResults = await Task.WhenAll(tasks);
            results.AddRange(mappedResults);

            return results;
        }

        /// <summary>
        /// Konfigürasyonu başka bir mapper'a uygula
        /// </summary>
        public void ApplyConfiguration(FluentMapper<TSource> other)
        {
            foreach (var mapping in _customMappings)
            {
                other._customMappings[mapping.Key] = mapping.Value;
            }

            foreach (var ignored in _ignoredProperties)
            {
                other._ignoredProperties.Add(ignored);
            }

            foreach (var conditional in _conditionalMappings)
            {
                other._conditionalMappings[conditional.Key] = conditional.Value;
            }

            other._beforeMapActions.AddRange(_beforeMapActions);
            other._afterMapActions.AddRange(_afterMapActions);
        }

        /// <summary>
        /// Özel mapping'leri uygula
        /// </summary>
        private void ApplyCustomMappings(object target)
        {
            foreach (var mapping in _customMappings)
            {
                var propertyPath = mapping.Key;
                var mappingFunc = mapping.Value;

                // Koşullu mapping kontrolü
                if (_conditionalMappings.TryGetValue(propertyPath, out var condition))
                {
                    if (!condition(_source))
                        continue;
                }

                // Nested property path'i işle
                var value = mappingFunc(_source);
                SetNestedPropertyValue(target, propertyPath, value);
            }
        }

        /// <summary>
        /// Nested property değerini ata
        /// </summary>
        private void SetNestedPropertyValue(object target, string propertyPath, object value)
        {
            var parts = propertyPath.Split('.');
            var current = target;

            // Son property'ye kadar ilerle
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var property = current.GetType().GetProperty(parts[i]);
                if (property == null || !property.CanRead) return;

                var propertyValue = property.GetValue(current);
                if (propertyValue == null)
                {
                    // Null ise yeni instance oluştur
                    if (property.CanWrite)
                    {
                        propertyValue = Activator.CreateInstance(property.PropertyType);
                        property.SetValue(current, propertyValue);
                    }
                    else
                    {
                        return;
                    }
                }

                current = propertyValue;
            }

            // Son property'ye değeri ata
            var finalProperty = current.GetType().GetProperty(parts[parts.Length - 1]);
            if (finalProperty != null && finalProperty.CanWrite)
            {
                // Tip dönüşümü yap
                var convertedValue = ConvertValue(value, finalProperty.PropertyType);
                finalProperty.SetValue(current, convertedValue);
            }
        }

        /// <summary>
        /// Değeri hedef tipe dönüştür
        /// </summary>
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
            
            var sourceType = value.GetType();
            
            // Aynı tip ise direkt döndür
            if (sourceType == targetType) return value;
            
            // Nullable tip kontrolü
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null)
                {
                    return ConvertValue(value, underlyingType);
                }
            }
            
            // Temel tip dönüşümleri
            try
            {
                if (targetType == typeof(string))
                {
                    return value.ToString();
                }
                
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, value.ToString());
                }
                
                // FastMapper ile dönüştürmeyi dene
                if (targetType.IsClass && !targetType.IsValueType)
                {
                    try
                    {
                        var convertMethod = typeof(MapperExtensions).GetMethod("FastMapTo", new[] { typeof(object) });
                        if (convertMethod != null)
                        {
                            var genericMethod = convertMethod.MakeGenericMethod(targetType);
                            return genericMethod.Invoke(null, new[] { value });
                        }
                    }
                    catch
                    {
                        // FastMapper ile dönüştürme başarısız olursa devam et
                    }
                }
                
                // Standart Convert.ChangeType kullan
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // Dönüştürme başarısız olursa null döndür
                return null;
            }
        }
    }

    /// <summary>
    /// Mapping context sınıfı - hedef tip belirlendikten sonra property mapping'leri için
    /// </summary>
    public class MappingContext<TSource, TTarget> where TTarget : new()
    {
        private readonly FluentMapper<TSource> _mapper;

        public MappingContext(FluentMapper<TSource> mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Property değerini ayarla
        /// </summary>
        public MappingContext<TSource, TTarget> Set(Expression<Func<TTarget, object>> targetProperty, 
            Func<TSource, object> mapping)
        {
            var propertyName = GetPropertyName(targetProperty);
            _mapper._customMappings[propertyName] = mapping;
            return this;
        }

        /// <summary>
        /// Property mapping tanımla (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("Map() yerine Set() kullanın")]
        public MappingContext<TSource, TTarget> Map(Expression<Func<TTarget, object>> targetProperty, 
            Func<TSource, object> mapping)
        {
            return Set(targetProperty, mapping);
        }

        /// <summary>
        /// Property'yi ignore et
        /// </summary>
        public MappingContext<TSource, TTarget> Ignore(Expression<Func<TTarget, object>> targetProperty)
        {
            var propertyName = GetPropertyName(targetProperty);
            _mapper._ignoredProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// Koşullu olarak property değerini ayarla
        /// </summary>
        public MappingContext<TSource, TTarget> SetIf(Expression<Func<TTarget, object>> targetProperty,
            Func<TSource, bool> condition, Func<TSource, object> mapping)
        {
            var propertyName = GetPropertyName(targetProperty);
            _mapper._customMappings[propertyName] = mapping;
            _mapper._conditionalMappings[propertyName] = condition;
            return this;
        }

        /// <summary>
        /// Koşullu mapping tanımla (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("MapIf() yerine SetIf() kullanın")]
        public MappingContext<TSource, TTarget> MapIf(Expression<Func<TTarget, object>> targetProperty,
            Func<TSource, bool> condition, Func<TSource, object> mapping)
        {
            return SetIf(targetProperty, condition, mapping);
        }

        /// <summary>
        /// Hedef property kontrolü ile koşullu olarak değer ayarla
        /// </summary>
        public MappingContext<TSource, TTarget> SetIf(Expression<Func<TTarget, object>> targetProperty,
            Expression<Func<TTarget, object>> targetConditionProperty, Func<TSource, object> mapping)
        {
            var propertyName = GetPropertyName(targetProperty);
            var conditionPropertyName = GetPropertyName(targetConditionProperty);
            
            _mapper._customMappings[propertyName] = mapping;
            _mapper._conditionalMappings[propertyName] = source => 
            {
                var target = _mapper.GetTargetObject();
                if (target == null) return false;
                
                var targetType = target.GetType();
                var conditionProperty = targetType.GetProperty(conditionPropertyName);
                if (conditionProperty == null) return false;
                
                var conditionValue = conditionProperty.GetValue(target);
                return conditionValue != null;
            };
            return this;
        }

        /// <summary>
        /// Hedef property kontrolü ile koşullu mapping tanımla (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("MapIf() yerine SetIf() kullanın")]
        public MappingContext<TSource, TTarget> MapIf(Expression<Func<TTarget, object>> targetProperty,
            Expression<Func<TTarget, object>> targetConditionProperty, Func<TSource, object> mapping)
        {
            return SetIf(targetProperty, targetConditionProperty, mapping);
        }

        /// <summary>
        /// İlk mevcut property'ye göre değer ayarlama (öncelik sırasıyla)
        /// </summary>
        public MappingContext<TSource, TTarget> SetFirstIfExist(Expression<Func<TTarget, object>> targetProperty,
            params (Expression<Func<TTarget, object>> conditionProperty, Func<TSource, object> mapping)[] conditions)
        {
            var propertyName = GetPropertyName(targetProperty);
            
            if (conditions.Length > 0)
            {
                var firstCondition = conditions[0];
                var firstConditionPropertyName = GetPropertyName(firstCondition.conditionProperty);
                
                _mapper._customMappings[propertyName] = firstCondition.mapping;
                _mapper._conditionalMappings[propertyName] = source => 
                {
                    var target = _mapper.GetTargetObject();
                    if (target == null) return false;
                    
                    var targetType = target.GetType();
                    var conditionProperty = targetType.GetProperty(firstConditionPropertyName);
                    if (conditionProperty == null) return false;
                    
                    var conditionValue = conditionProperty.GetValue(target);
                    return conditionValue != null;
                };
            }
            
            for (int i = 1; i < conditions.Length; i++)
            {
                var condition = conditions[i];
                var conditionPropertyName = GetPropertyName(condition.conditionProperty);
                
                var mappingKey = $"{propertyName}_condition_{i}";
                _mapper._customMappings[mappingKey] = condition.mapping;
                _mapper._conditionalMappings[mappingKey] = source => 
                {
                    var target = _mapper.GetTargetObject();
                    if (target == null) return false;
                    
                    var targetType = target.GetType();
                    var conditionProperty = targetType.GetProperty(conditionPropertyName);
                    if (conditionProperty == null) return false;
                    
                    var conditionValue = conditionProperty.GetValue(target);
                    return conditionValue != null;
                };
            }
            
            return this;
        }

        /// <summary>
        /// İlk mevcut property'ye göre değer ayarlama (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("SetIfElse() yerine SetFirstIfExist() kullanın")]
        public MappingContext<TSource, TTarget> SetIfElse(Expression<Func<TTarget, object>> targetProperty,
            params (Expression<Func<TTarget, object>> conditionProperty, Func<TSource, object> mapping)[] conditions)
        {
            return SetFirstIfExist(targetProperty, conditions);
        }

        /// <summary>
        /// If-else if-else mantığı ile koşullu mapping (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("MapIfElse() yerine SetFirstIfExist() kullanın")]
        public MappingContext<TSource, TTarget> MapIfElse(Expression<Func<TTarget, object>> targetProperty,
            params (Expression<Func<TTarget, object>> conditionProperty, Func<TSource, object> mapping)[] conditions)
        {
            return SetFirstIfExist(targetProperty, conditions);
        }

        /// <summary>
        /// Mapping öncesi aksiyon ekle
        /// </summary>
        public MappingContext<TSource, TTarget> BeforeMap(Action<TSource, object> action)
        {
            _mapper._beforeMapActions.Add(action);
            return this;
        }

        /// <summary>
        /// Mapping sonrası aksiyon ekle
        /// </summary>
        public MappingContext<TSource, TTarget> AfterMap(Action<TSource, object> action)
        {
            _mapper._afterMapActions.Add(action);
            return this;
        }

        /// <summary>
        /// Merge stratejisi belirle
        /// </summary>
        public MappingContext<TSource, TTarget> WithMergeStrategy(MergeStrategy strategy)
        {
            _mapper.WithMergeStrategy(strategy);
            return this;
        }

        /// <summary>
        /// Uyumluluk modu belirle
        /// </summary>
        public MappingContext<TSource, TTarget> WithCompatibilityMode(CompatibilityMode mode)
        {
            _mapper.WithCompatibilityMode(mode);
            return this;
        }

        /// <summary>
        /// Yeni nesne oluşturarak mapping yap
        /// </summary>
        public TTarget Create()
        {
            return _mapper.To<TTarget>();
        }

        /// <summary>
        /// Mevcut nesneye mapping yap
        /// </summary>
        public TTarget Create(TTarget target)
        {
            return _mapper.To(target);
        }

        /// <summary>
        /// Yeni nesne oluşturarak mapping yap (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("To() yerine Create() kullanın")]
        public TTarget To()
        {
            return _mapper.To<TTarget>();
        }

        /// <summary>
        /// Mevcut nesneye mapping yap (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("To(target) yerine Create(target) kullanın")]
        public TTarget To(TTarget target)
        {
            return _mapper.To(target);
        }

        /// <summary>
        /// Expression'dan property path'ini çıkar
        /// </summary>
        private string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            return GetPropertyPath(expression.Body);
        }

        /// <summary>
        /// Expression'dan property path'ini çıkar
        /// </summary>
        private string GetPropertyPath(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                var parentPath = GetPropertyPath(memberExpression.Expression);
                return string.IsNullOrEmpty(parentPath) 
                    ? memberExpression.Member.Name 
                    : $"{parentPath}.{memberExpression.Member.Name}";
            }

            if (expression is UnaryExpression unaryExpression)
            {
                return GetPropertyPath(unaryExpression.Operand);
            }

            if (expression is ParameterExpression)
            {
                return string.Empty;
            }

            if (expression is ConstantExpression)
            {
                return string.Empty;
            }

            throw new ArgumentException($"Geçersiz property expression: {expression.GetType().Name}");
        }
    }

    /// <summary>
    /// Merge stratejileri
    /// </summary>
    public enum MergeStrategy
    {
        Replace,        // Tüm değerleri değiştir
        Merge,          // Null olmayan değerlerle güncelle
        Append,         // Mevcut değerlere ekle
        Conditional     // Koşullu güncelleme
    }

    /// <summary>
    /// Uyumluluk modları
    /// </summary>
    public enum CompatibilityMode
    {
        Latest,         // En son özellikler
        Legacy,         // Geriye dönük uyumluluk
        Strict,         // Katı tip kontrolü
        Loose           // Esnek tip kontrolü
    }

    /// <summary>
    /// Fluent API extension methods
    /// </summary>
    public static class FluentMapperExtensions
    {
        /// <summary>
        /// Fluent mapping builder başlat (hedef tip ile)
        /// </summary>
        public static MappingContext<TSource, TTarget> BuildFor<TSource, TTarget>(this TSource source)
            where TTarget : new()
        {
            var mapper = new FluentMapper<TSource>(source);
            return new MappingContext<TSource, TTarget>(mapper);
        }

        /// <summary>
        /// Fluent mapping builder başlat (hedef tip olmadan)
        /// </summary>
        public static FluentMapper<TSource> Builder<TSource>(this TSource source)
        {
            return new FluentMapper<TSource>(source);
        }

        /// <summary>
        /// Fluent mapping başlat (geriye dönük uyumluluk için)
        /// </summary>
        [Obsolete("Map() yerine Builder() kullanın")]
        public static FluentMapper<TSource> Map<TSource>(this TSource source)
        {
            return new FluentMapper<TSource>(source);
        }

        /// <summary>
        /// Toplu fluent mapping
        /// </summary>
        public static FluentMapper<TSource> Map<TSource>(this IEnumerable<TSource> sources)
        {
            // İlk elemanı al (örnek amaçlı)
            var enumerator = sources.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return new FluentMapper<TSource>(enumerator.Current);
            }
            
            throw new InvalidOperationException("Kaynak koleksiyon boş olamaz");
        }

        /// <summary>
        /// Asenkron liste mapping extension method
        /// </summary>
        public static async Task<List<TTarget>> ToListAsync<TSource, TTarget>(this FluentMapper<TSource> fluentMapper, IEnumerable<TSource> sources) 
            where TTarget : new()
        {
            var results = new List<TTarget>();
            var tasks = new List<Task<TTarget>>();

            foreach (var source in sources)
            {
                var newFluentMapper = new FluentMapper<TSource>(source);
                // Aynı konfigürasyonu uygula
                fluentMapper.ApplyConfiguration(newFluentMapper);
                tasks.Add(Task.Run(() => newFluentMapper.To<TTarget>()));
            }

            var mappedResults = await Task.WhenAll(tasks);
            results.AddRange(mappedResults);

            return results;
        }
    }
} 