using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastMapper
{
    /// <summary>
    /// Diff Mapping - İki nesne arasındaki farkları tespit eder ve detaylı rapor sunar
    /// </summary>
    public static class DiffMapper
    {
        private static readonly ConcurrentDictionary<Type, PropertyDiffInfo[]> _propertyDiffCache = new();
        private static readonly ConcurrentDictionary<long, DiffResult> _diffCache = new();

        /// <summary>
        /// Diff sonucu
        /// </summary>
        public class DiffResult
        {
            public bool HasDifferences { get; set; }
            public List<PropertyDiff> Differences { get; set; } = new();
            public List<PropertyDiff> Similarities { get; set; } = new();
            public Dictionary<string, object> SourceValues { get; set; } = new();
            public Dictionary<string, object> TargetValues { get; set; } = new();
            public TimeSpan DiffTime { get; set; }
            public int TotalProperties { get; set; }
            public int DifferentProperties { get; set; }
            public int SimilarProperties { get; set; }
            public double SimilarityPercentage { get; set; }
            public DiffSummary Summary { get; set; }
        }

        /// <summary>
        /// Property diff bilgisi
        /// </summary>
        public class PropertyDiff
        {
            public string PropertyName { get; set; }
            public object SourceValue { get; set; }
            public object TargetValue { get; set; }
            public DiffType DiffType { get; set; }
            public string DiffReason { get; set; }
            public double SimilarityScore { get; set; }
            public DiffSeverity Severity { get; set; }
            public List<string> Suggestions { get; set; } = new();
        }

        /// <summary>
        /// Diff özeti
        /// </summary>
        public class DiffSummary
        {
            public int CriticalDifferences { get; set; }
            public int MajorDifferences { get; set; }
            public int MinorDifferences { get; set; }
            public int InfoDifferences { get; set; }
            public List<string> CriticalProperties { get; set; } = new();
            public List<string> MajorProperties { get; set; } = new();
            public string OverallAssessment { get; set; }
        }

        /// <summary>
        /// Diff tipi
        /// </summary>
        public enum DiffType
        {
            NoDifference,        // Fark yok
            ValueChanged,        // Değer değişti
            TypeMismatch,        // Tip uyumsuzluğu
            NullMismatch,        // Null/NotNull uyumsuzluğu
            CollectionChanged,   // Koleksiyon değişti
            PropertyMissing,     // Property eksik
            PropertyAdded,       // Property eklendi
            StructureChanged     // Yapı değişti
        }

        /// <summary>
        /// Diff şiddeti
        /// </summary>
        public enum DiffSeverity
        {
            Info,       // Bilgi
            Minor,      // Küçük
            Major,      // Büyük
            Critical    // Kritik
        }

        /// <summary>
        /// İki nesne arasındaki farkları bul
        /// </summary>
        public static DiffResult FindDifferences<T>(T source, T target)
        {
            if (source == null && target == null)
                return CreateEmptyResult();

            if (source == null || target == null)
                return CreateNullComparisonResult(source, target);

            var startTime = DateTime.UtcNow;
            var result = new DiffResult();

            try
            {
                var type = typeof(T);
                var propertyInfos = GetPropertyDiffInfo(type);
                
                result.TotalProperties = propertyInfos.Length;
                result.SourceValues = new Dictionary<string, object>();
                result.TargetValues = new Dictionary<string, object>();

                foreach (var propInfo in propertyInfos)
                {
                    var sourceValue = propInfo.Getter(source);
                    var targetValue = propInfo.Getter(target);

                    result.SourceValues[propInfo.PropertyName] = sourceValue;
                    result.TargetValues[propInfo.PropertyName] = targetValue;

                    var diff = AnalyzePropertyDifference(propInfo, sourceValue, targetValue);
                    
                    if (diff.DiffType != DiffType.NoDifference)
                    {
                        result.Differences.Add(diff);
                        result.DifferentProperties++;
                    }
                    else
                    {
                        result.Similarities.Add(diff);
                        result.SimilarProperties++;
                    }
                }

                // Özet bilgileri hesapla
                result.HasDifferences = result.Differences.Count > 0;
                result.SimilarityPercentage = result.TotalProperties > 0 ? (double)result.SimilarProperties / result.TotalProperties * 100 : 0;
                result.DiffTime = DateTime.UtcNow - startTime;
                
                // Summary hesapla
                CalculateSummary(result);
            }
            catch (Exception ex)
            {
                result.Differences.Add(new PropertyDiff
                {
                    PropertyName = "General",
                    DiffType = DiffType.StructureChanged,
                    DiffReason = $"Diff analizi sırasında hata: {ex.Message}",
                    Severity = DiffSeverity.Critical
                });
                result.HasDifferences = true;
                
                // Exception durumunda da Summary hesapla
                CalculateSummary(result);
            }

            return result;
        }

        /// <summary>
        /// Generic diff analizi
        /// </summary>
        public static DiffResult FindDifferences(object source, object target)
        {
            if (source?.GetType() != target?.GetType())
            {
                return CreateTypeMismatchResult(source, target);
            }

            // Generic method'u çağır
            var methods = typeof(DiffMapper).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var genericMethod = methods.FirstOrDefault(m => m.IsGenericMethodDefinition && m.Name == nameof(FindDifferences));
            if (genericMethod != null)
            {
                var constructedMethod = genericMethod.MakeGenericMethod(source.GetType());
                return (DiffResult)constructedMethod.Invoke(null, new[] { source, target });
            }
            
            // Fallback: Doğrudan generic method'u çağır
            var fallbackMethod = typeof(DiffMapper).GetMethod(nameof(FindDifferences), BindingFlags.Public | BindingFlags.Static);
            var constructedFallbackMethod = fallbackMethod.MakeGenericMethod(source.GetType());
            return (DiffResult)constructedFallbackMethod.Invoke(null, new[] { source, target });
        }

        /// <summary>
        /// Property farkını analiz et
        /// </summary>
        private static PropertyDiff AnalyzePropertyDifference(PropertyDiffInfo propInfo, object sourceValue, object targetValue)
        {
            var diff = new PropertyDiff
            {
                PropertyName = propInfo.PropertyName,
                SourceValue = sourceValue,
                TargetValue = targetValue,
                SimilarityScore = 1.0
            };

            // Null kontrolü
            if (sourceValue == null && targetValue == null)
            {
                diff.DiffType = DiffType.NoDifference;
                return diff;
            }

            if (sourceValue == null || targetValue == null)
            {
                diff.DiffType = DiffType.NullMismatch;
                diff.DiffReason = "Null/NotNull uyumsuzluğu";
                diff.Severity = DiffSeverity.Major;
                diff.SimilarityScore = 0.0;
                return diff;
            }

            // Tip kontrolü
            if (sourceValue.GetType() != targetValue.GetType())
            {
                diff.DiffType = DiffType.TypeMismatch;
                diff.DiffReason = $"Tip uyumsuzluğu: {sourceValue.GetType().Name} vs {targetValue.GetType().Name}";
                diff.Severity = DiffSeverity.Critical;
                diff.SimilarityScore = 0.0;
                diff.Suggestions.Add("Tip dönüşümü gerekli");
                return diff;
            }

            // Değer karşılaştırması
            if (propInfo.IsCollection)
            {
                return AnalyzeCollectionDifference(propInfo, sourceValue, targetValue);
            }
            else if (propInfo.IsComplexType)
            {
                return AnalyzeComplexTypeDifference(propInfo, sourceValue, targetValue);
            }
            else
            {
                return AnalyzeSimpleTypeDifference(propInfo, sourceValue, targetValue);
            }
        }

        /// <summary>
        /// Basit tip farkını analiz et
        /// </summary>
        private static PropertyDiff AnalyzeSimpleTypeDifference(PropertyDiffInfo propInfo, object sourceValue, object targetValue)
        {
            var diff = new PropertyDiff
            {
                PropertyName = propInfo.PropertyName,
                SourceValue = sourceValue,
                TargetValue = targetValue
            };

            // Decimal değerler için özel karşılaştırma
            bool areEqual = false;
            if (sourceValue is decimal sourceDecimal && targetValue is decimal targetDecimal)
            {
                areEqual = sourceDecimal == targetDecimal;
            }
            else if (sourceValue is IComparable comparable && targetValue != null)
            {
                try
                {
                    areEqual = comparable.CompareTo(targetValue) == 0;
                }
                catch
                {
                    areEqual = Equals(sourceValue, targetValue);
                }
            }
            else
            {
                areEqual = Equals(sourceValue, targetValue);
            }



            if (areEqual)
            {
                diff.DiffType = DiffType.NoDifference;
                diff.SimilarityScore = 1.0;
            }
            else
            {
                diff.DiffType = DiffType.ValueChanged;
                diff.DiffReason = "Değer değişti";
                diff.SimilarityScore = CalculateSimilarityScore(sourceValue, targetValue);
                diff.Severity = DetermineSeverity(sourceValue, targetValue, propInfo);
            }

            return diff;
        }

        /// <summary>
        /// Koleksiyon farkını analiz et
        /// </summary>
        private static PropertyDiff AnalyzeCollectionDifference(PropertyDiffInfo propInfo, object sourceValue, object targetValue)
        {
            var diff = new PropertyDiff
            {
                PropertyName = propInfo.PropertyName,
                SourceValue = sourceValue,
                TargetValue = targetValue,
                DiffType = DiffType.CollectionChanged
            };

            if (sourceValue is System.Collections.IEnumerable sourceEnum && 
                targetValue is System.Collections.IEnumerable targetEnum)
            {
                var sourceList = sourceEnum.Cast<object>().ToList();
                var targetList = targetEnum.Cast<object>().ToList();

                if (sourceList.Count != targetList.Count)
                {
                    diff.DiffReason = $"Koleksiyon boyutu değişti: {sourceList.Count} -> {targetList.Count}";
                    diff.Severity = DiffSeverity.Major;
                    diff.SimilarityScore = Math.Min((double)sourceList.Count, targetList.Count) / Math.Max(sourceList.Count, targetList.Count);
                }
                else
                {
                    var differences = 0;
                    for (int i = 0; i < sourceList.Count; i++)
                    {
                        if (!Equals(sourceList[i], targetList[i]))
                            differences++;
                    }

                    if (differences == 0)
                    {
                        diff.DiffType = DiffType.NoDifference;
                        diff.SimilarityScore = 1.0;
                    }
                    else
                    {
                        diff.DiffReason = $"Koleksiyon içeriği değişti: {differences}/{sourceList.Count} eleman farklı";
                        diff.Severity = DiffSeverity.Minor;
                        diff.SimilarityScore = (double)(sourceList.Count - differences) / sourceList.Count;
                    }
                }
            }

            return diff;
        }

        /// <summary>
        /// Karmaşık tip farkını analiz et
        /// </summary>
        private static PropertyDiff AnalyzeComplexTypeDifference(PropertyDiffInfo propInfo, object sourceValue, object targetValue)
        {
            var diff = new PropertyDiff
            {
                PropertyName = propInfo.PropertyName,
                SourceValue = sourceValue,
                TargetValue = targetValue,
                DiffType = DiffType.StructureChanged
            };

            // Recursive diff analizi
            var nestedDiff = FindDifferences(sourceValue, targetValue);
            
            if (nestedDiff.HasDifferences)
            {
                diff.DiffReason = $"Nested object değişti: {nestedDiff.DifferentProperties} property farklı";
                diff.SimilarityScore = nestedDiff.SimilarityPercentage / 100.0;
                diff.Severity = nestedDiff.Summary.CriticalDifferences > 0 ? DiffSeverity.Critical : 
                               nestedDiff.Summary.MajorDifferences > 0 ? DiffSeverity.Major : DiffSeverity.Minor;
            }
            else
            {
                diff.DiffType = DiffType.NoDifference;
                diff.SimilarityScore = 1.0;
            }

            return diff;
        }

        /// <summary>
        /// Benzerlik skoru hesapla
        /// </summary>
        private static double CalculateSimilarityScore(object sourceValue, object targetValue)
        {
            if (sourceValue is string sourceStr && targetValue is string targetStr)
            {
                return CalculateStringSimilarity(sourceStr, targetStr);
            }

            if (sourceValue is IComparable comparable)
            {
                try
                {
                    var comparison = comparable.CompareTo(targetValue);
                    return comparison == 0 ? 1.0 : 0.0;
                }
                catch
                {
                    return 0.0;
                }
            }

            return Equals(sourceValue, targetValue) ? 1.0 : 0.0;
        }

        /// <summary>
        /// String benzerlik skoru hesapla
        /// </summary>
        private static double CalculateStringSimilarity(string source, string target)
        {
            if (source == target) return 1.0;
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;

            var maxLength = Math.Max(source.Length, target.Length);
            var minLength = Math.Min(source.Length, target.Length);
            var commonLength = 0;

            for (int i = 0; i < minLength; i++)
            {
                if (source[i] == target[i])
                    commonLength++;
            }

            return (double)commonLength / maxLength;
        }

        /// <summary>
        /// Şiddet belirle
        /// </summary>
        private static DiffSeverity DetermineSeverity(object sourceValue, object targetValue, PropertyDiffInfo propInfo)
        {
            // Kritik property'ler için
            if (IsCriticalProperty(propInfo.PropertyName))
                return DiffSeverity.Critical;

            // Numeric değerler için
            if (IsNumericType(sourceValue.GetType()))
            {
                var sourceNum = Convert.ToDouble(sourceValue);
                var targetNum = Convert.ToDouble(targetValue);
                var difference = Math.Abs(sourceNum - targetNum);
                var percentage = difference / Math.Max(Math.Abs(sourceNum), Math.Abs(targetNum));

                if (percentage > 0.5) return DiffSeverity.Critical;
                if (percentage > 0.2) return DiffSeverity.Major;
                if (percentage > 0.1) return DiffSeverity.Minor;
                return DiffSeverity.Info;
            }

            // String değerler için
            if (sourceValue is string sourceStr && targetValue is string targetStr)
            {
                var similarity = CalculateStringSimilarity(sourceStr, targetStr);
                if (similarity < 0.3) return DiffSeverity.Critical;
                if (similarity < 0.6) return DiffSeverity.Major;
                if (similarity < 0.8) return DiffSeverity.Minor;
                return DiffSeverity.Info;
            }

            return DiffSeverity.Minor;
        }

        /// <summary>
        /// Kritik property kontrolü
        /// </summary>
        private static bool IsCriticalProperty(string propertyName)
        {
            var criticalProps = new[] { "Id", "Key", "PrimaryKey", "Identifier", "GUID", "Hash" };
            return criticalProps.Any(p => propertyName.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Numeric tip kontrolü
        /// </summary>
        private static bool IsNumericType(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(double) || 
                   type == typeof(decimal) || type == typeof(float) || type == typeof(short);
        }

        /// <summary>
        /// Özet hesapla
        /// </summary>
        private static void CalculateSummary(DiffResult result)
        {
            var summary = new DiffSummary();

            foreach (var diff in result.Differences)
            {
                switch (diff.Severity)
                {
                    case DiffSeverity.Critical:
                        summary.CriticalDifferences++;
                        summary.CriticalProperties.Add(diff.PropertyName);
                        break;
                    case DiffSeverity.Major:
                        summary.MajorDifferences++;
                        summary.MajorProperties.Add(diff.PropertyName);
                        break;
                    case DiffSeverity.Minor:
                        summary.MinorDifferences++;
                        break;
                    case DiffSeverity.Info:
                        summary.InfoDifferences++;
                        break;
                }
            }

            // Genel değerlendirme
            if (summary.CriticalDifferences > 0)
                summary.OverallAssessment = "Kritik farklar tespit edildi";
            else if (summary.MajorDifferences > 0)
                summary.OverallAssessment = "Önemli farklar tespit edildi";
            else if (summary.MinorDifferences > 0)
                summary.OverallAssessment = "Küçük farklar tespit edildi";
            else
                summary.OverallAssessment = "Fark tespit edilmedi";

            result.Summary = summary;
        }

        /// <summary>
        /// Boş sonuç oluştur
        /// </summary>
        private static DiffResult CreateEmptyResult()
        {
            return new DiffResult
            {
                HasDifferences = false,
                SimilarityPercentage = 100.0,
                Summary = new DiffSummary { OverallAssessment = "Her iki nesne de null" }
            };
        }

        /// <summary>
        /// Null karşılaştırma sonucu oluştur
        /// </summary>
        private static DiffResult CreateNullComparisonResult(object source, object target)
        {
            return new DiffResult
            {
                HasDifferences = true,
                Differences = { new PropertyDiff
                {
                    PropertyName = "Root",
                    DiffType = DiffType.NullMismatch,
                    DiffReason = "Null/NotNull uyumsuzluğu",
                    Severity = DiffSeverity.Critical
                }},
                Summary = new DiffSummary { OverallAssessment = "Null uyumsuzluğu" }
            };
        }

        /// <summary>
        /// Tip uyumsuzluğu sonucu oluştur
        /// </summary>
        private static DiffResult CreateTypeMismatchResult(object source, object target)
        {
            return new DiffResult
            {
                HasDifferences = true,
                Differences = { new PropertyDiff
                {
                    PropertyName = "Type",
                    DiffType = DiffType.TypeMismatch,
                    DiffReason = $"Tip uyumsuzluğu: {source?.GetType().Name} vs {target?.GetType().Name}",
                    Severity = DiffSeverity.Critical
                }},
                Summary = new DiffSummary { OverallAssessment = "Tip uyumsuzluğu" }
            };
        }

        /// <summary>
        /// Property diff bilgilerini al
        /// </summary>
        private static PropertyDiffInfo[] GetPropertyDiffInfo(Type type)
        {
            if (_propertyDiffCache.TryGetValue(type, out var cached))
                return cached;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var infos = new PropertyDiffInfo[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                infos[i] = new PropertyDiffInfo
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.PropertyType,
                    Getter = prop.CanRead ? CreateGetter(prop) : null,
                    IsCollection = IsCollectionType(prop.PropertyType),
                    IsComplexType = IsComplexType(prop.PropertyType),
                    IsNullable = IsNullableType(prop.PropertyType)
                };
            }

            _propertyDiffCache[type] = infos;
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
                   type != typeof(Guid) && type != typeof(decimal) && !type.IsEnum;
        }

        /// <summary>
        /// Nullable tip kontrolü
        /// </summary>
        private static bool IsNullableType(Type type)
        {
            // Nullable<> tipleri
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return true;
            
            // Reference tipleri (string hariç)
            if (!type.IsValueType && type != typeof(string))
                return true;
            
            return false;
        }

        /// <summary>
        /// Diff cache'ini temizle
        /// </summary>
        public static void ClearDiffCache()
        {
            _diffCache.Clear();
            _propertyDiffCache.Clear();
        }

        /// <summary>
        /// Diff istatistiklerini al
        /// </summary>
        public static (int DiffCache, int PropertyCache) GetDiffStats()
        {
            return (_diffCache.Count, _propertyDiffCache.Count);
        }

        /// <summary>
        /// Property diff bilgisi
        /// </summary>
        private class PropertyDiffInfo
        {
            public string PropertyName { get; set; }
            public Type PropertyType { get; set; }
            public Func<object, object> Getter { get; set; }
            public bool IsCollection { get; set; }
            public bool IsComplexType { get; set; }
            public bool IsNullable { get; set; }
        }
    }
} 