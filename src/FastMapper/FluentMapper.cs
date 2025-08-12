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
        private readonly Dictionary<string, Func<TSource, object>> _customMappings = new();
        private readonly HashSet<string> _ignoredProperties = new();
        private readonly Dictionary<string, Func<TSource, bool>> _conditionalMappings = new();
        private readonly List<Action<TSource, object>> _beforeMapActions = new();
        private readonly List<Action<TSource, object>> _afterMapActions = new();

        public FluentMapper(TSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Özel mapping tanımla
        /// </summary>
        public FluentMapper<TSource> Map<TTarget>(Expression<Func<TTarget, object>> targetProperty, 
            Func<TSource, object> mapping)
        {
            var propertyName = GetPropertyName(targetProperty);
            _customMappings[propertyName] = mapping;
            return this;
        }

        /// <summary>
        /// Property'yi ignore et
        /// </summary>
        public FluentMapper<TSource> Ignore<TTarget>(Expression<Func<TTarget, object>> targetProperty)
        {
            var propertyName = GetPropertyName(targetProperty);
            _ignoredProperties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// Koşullu mapping tanımla
        /// </summary>
        public FluentMapper<TSource> MapIf<TTarget>(Expression<Func<TTarget, object>> targetProperty,
            Func<TSource, bool> condition, Func<TSource, object> mapping)
        {
            var propertyName = GetPropertyName(targetProperty);
            _customMappings[propertyName] = mapping;
            _conditionalMappings[propertyName] = condition;
            return this;
        }

        /// <summary>
        /// Hedef property kontrolü ile koşullu mapping tanımla (if-else if-else mantığı)
        /// </summary>
        public FluentMapper<TSource> MapIf<TTarget>(Expression<Func<TTarget, object>> targetProperty,
            Expression<Func<TTarget, object>> targetConditionProperty, Func<TSource, object> mapping)
        {
            var propertyName = GetPropertyName(targetProperty);
            var conditionPropertyName = GetPropertyName(targetConditionProperty);
            
            _customMappings[propertyName] = mapping;
            _conditionalMappings[propertyName] = source => 
            {
                // Hedef nesneyi bul (eğer varsa)
                var target = GetTargetObject();
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
        /// If-else if-else mantığı ile koşullu mapping
        /// </summary>
        public FluentMapper<TSource> MapIfElse<TTarget>(Expression<Func<TTarget, object>> targetProperty,
            params (Expression<Func<TTarget, object>> conditionProperty, Func<TSource, object> mapping)[] conditions)
        {
            var propertyName = GetPropertyName(targetProperty);
            
            // İlk koşul için mapping'i kaydet
            if (conditions.Length > 0)
            {
                var firstCondition = conditions[0];
                var firstConditionPropertyName = GetPropertyName(firstCondition.conditionProperty);
                
                _customMappings[propertyName] = firstCondition.mapping;
                _conditionalMappings[propertyName] = source => 
                {
                    var target = GetTargetObject();
                    if (target == null) return false;
                    
                    var targetType = target.GetType();
                    var conditionProperty = targetType.GetProperty(firstConditionPropertyName);
                    if (conditionProperty == null) return false;
                    
                    var conditionValue = conditionProperty.GetValue(target);
                    return conditionValue != null;
                };
            }
            
            // Diğer koşullar için ayrı mapping'ler oluştur
            for (int i = 1; i < conditions.Length; i++)
            {
                var condition = conditions[i];
                var conditionPropertyName = GetPropertyName(condition.conditionProperty);
                
                // Aynı property için farklı bir mapping key oluştur
                var mappingKey = $"{propertyName}_condition_{i}";
                _customMappings[mappingKey] = condition.mapping;
                _conditionalMappings[mappingKey] = source => 
                {
                    var target = GetTargetObject();
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
        /// Hedef nesneyi al (eğer varsa)
        /// </summary>
        private object GetTargetObject()
        {
            // Bu method, mapping sırasında hedef nesneye erişim sağlar
            // Şimdilik null döndürüyoruz, gerçek implementasyonda hedef nesneye erişim gerekir
            return null;
        }

        /// <summary>
        /// Mapping öncesi aksiyon ekle
        /// </summary>
        public FluentMapper<TSource> BeforeMap(Action<TSource, object> action)
        {
            _beforeMapActions.Add(action);
            return this;
        }

        /// <summary>
        /// Mapping sonrası aksiyon ekle
        /// </summary>
        public FluentMapper<TSource> AfterMap(Action<TSource, object> action)
        {
            _afterMapActions.Add(action);
            return this;
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
                finalProperty.SetValue(current, value);
            }
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
        /// Fluent mapping başlat
        /// </summary>
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