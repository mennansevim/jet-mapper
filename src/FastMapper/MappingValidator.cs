using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastMapper
{
    /// <summary>
    /// Mapping Validator - mapping tanımlarının doğruluğunu ve tutarlılığını kontrol eder
    /// </summary>
    public static class MappingValidator
    {
        private static readonly ConcurrentDictionary<long, ValidationResult> _validationCache = new();
        private static readonly ConcurrentDictionary<Type, PropertyValidationInfo[]> _propertyValidationCache = new();

        /// <summary>
        /// Mapping validation sonucu
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationError> Errors { get; set; } = new();
            public List<ValidationWarning> Warnings { get; set; } = new();
            public Dictionary<string, PropertyValidationInfo> PropertyValidations { get; set; } = new();
            public TimeSpan ValidationTime { get; set; }
            public int TotalProperties { get; set; }
            public int MappedProperties { get; set; }
            public int IgnoredProperties { get; set; }
        }

        /// <summary>
        /// Validation hatası
        /// </summary>
        public class ValidationError
        {
            public string PropertyName { get; set; }
            public string ErrorType { get; set; }
            public string Message { get; set; }
            public ValidationSeverity Severity { get; set; }
            public string SuggestedFix { get; set; }
        }

        /// <summary>
        /// Validation uyarısı
        /// </summary>
        public class ValidationWarning
        {
            public string PropertyName { get; set; }
            public string WarningType { get; set; }
            public string Message { get; set; }
            public ValidationSeverity Severity { get; set; }
        }

        /// <summary>
        /// Property validation bilgisi
        /// </summary>
        public class PropertyValidationInfo
        {
            public string PropertyName { get; set; }
            public Type SourceType { get; set; }
            public Type TargetType { get; set; }
            public bool CanMap { get; set; }
            public string MappingStrategy { get; set; }
            public bool RequiresConversion { get; set; }
            public string ConversionType { get; set; }
            public bool IsNullable { get; set; }
            public bool IsCollection { get; set; }
            public Type ElementType { get; set; }
            public int Depth { get; set; }
        }

        /// <summary>
        /// Validation şiddeti
        /// </summary>
        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// Mapping validation'ı gerçekleştir
        /// </summary>
        public static ValidationResult ValidateMapping<TSource, TTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            var key = GetTypeKey(sourceType, targetType);

            if (_validationCache.TryGetValue(key, out var cached))
                return cached;

            var startTime = DateTime.UtcNow;
            var result = new ValidationResult();

            try
            {
                // Property validation'larını gerçekleştir
                var sourceProps = GetPropertyValidationInfo(sourceType);
                var targetProps = GetPropertyValidationInfo(targetType);

                result.TotalProperties = targetProps.Length;
                result.PropertyValidations = new Dictionary<string, PropertyValidationInfo>();

                // Her target property için validation
                foreach (var targetProp in targetProps)
                {
                    var validation = ValidatePropertyMapping(sourceProps, targetProp);
                    result.PropertyValidations[targetProp.PropertyName] = validation;

                    if (validation.CanMap)
                    {
                        result.MappedProperties++;
                    }
                    else
                    {
                        result.IgnoredProperties++;
                        result.Errors.Add(new ValidationError
                        {
                            PropertyName = targetProp.PropertyName,
                            ErrorType = "UnmappedProperty",
                            Message = $"Property '{targetProp.PropertyName}' için uygun source property bulunamadı",
                            Severity = ValidationSeverity.Warning,
                            SuggestedFix = $"Source type'da '{targetProp.PropertyName}' property'si ekleyin veya custom mapping tanımlayın"
                        });
                    }

                    // Tip uyumsuzluğu kontrolü
                    if (validation.RequiresConversion)
                    {
                        var conversionValidation = ValidateTypeConversion(validation);
                        if (!conversionValidation.IsValid)
                        {
                            result.Errors.AddRange(conversionValidation.Errors);
                        }
                    }

                    // Nullable kontrolü
                    if (!validation.IsNullable && targetProp.IsNullable)
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            PropertyName = targetProp.PropertyName,
                            WarningType = "NullableMismatch",
                            Message = $"Target property nullable ama source property nullable değil",
                            Severity = ValidationSeverity.Warning
                        });
                    }

                    // Collection depth kontrolü
                    if (validation.Depth > 3)
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            PropertyName = targetProp.PropertyName,
                            WarningType = "DeepNesting",
                            Message = $"Çok derin nested mapping ({validation.Depth} seviye) performans sorunlarına neden olabilir",
                            Severity = ValidationSeverity.Warning
                        });
                    }
                }

                // Genel validation kuralları
                ValidateGeneralRules(result, sourceType, targetType);

                result.IsValid = result.Errors.Count == 0;
                result.ValidationTime = DateTime.UtcNow - startTime;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "General",
                    ErrorType = "ValidationException",
                    Message = $"Validation sırasında hata oluştu: {ex.Message}",
                    Severity = ValidationSeverity.Critical
                });
                result.IsValid = false;
            }

            _validationCache[key] = result;
            return result;
        }

        /// <summary>
        /// Property mapping validation'ı
        /// </summary>
        private static PropertyValidationInfo ValidatePropertyMapping(
            PropertyValidationInfo[] sourceProps, 
            PropertyValidationInfo targetProp)
        {
            var validation = new PropertyValidationInfo
            {
                PropertyName = targetProp.PropertyName,
                TargetType = targetProp.TargetType,
                CanMap = false,
                Depth = 0
            };

            // Source property'yi bul
            var sourceProp = sourceProps.FirstOrDefault(p => 
                string.Equals(p.PropertyName, targetProp.PropertyName, StringComparison.OrdinalIgnoreCase));

            if (sourceProp != null)
            {
                validation.SourceType = sourceProp.SourceType;
                validation.CanMap = true;
                validation.MappingStrategy = "DirectMapping";

                // Tip uyumluluğu kontrolü
                if (sourceProp.SourceType == targetProp.TargetType)
                {
                    validation.RequiresConversion = false;
                }
                else if (targetProp.TargetType.IsAssignableFrom(sourceProp.SourceType))
                {
                    validation.RequiresConversion = false;
                    validation.MappingStrategy = "InheritanceMapping";
                }
                else
                {
                    validation.RequiresConversion = true;
                    validation.ConversionType = DetermineConversionType(sourceProp.SourceType, targetProp.TargetType);
                    validation.MappingStrategy = "ConversionMapping";
                }

                // Nullable kontrolü
                validation.IsNullable = IsNullableType(sourceProp.SourceType);
                validation.IsCollection = IsCollectionType(sourceProp.SourceType);
                
                if (validation.IsCollection)
                {
                    validation.ElementType = GetCollectionElementType(sourceProp.SourceType);
                    validation.Depth = CalculateNestingDepth(sourceProp.SourceType);
                }
            }

            return validation;
        }

        /// <summary>
        /// Tip dönüşümü validation'ı
        /// </summary>
        private static ValidationResult ValidateTypeConversion(PropertyValidationInfo validation)
        {
            var result = new ValidationResult();

            var sourceType = validation.SourceType;
            var targetType = validation.TargetType;

            // Güvenli dönüşüm kontrolü
            if (!IsSafeConversion(sourceType, targetType))
            {
                result.Errors.Add(new ValidationError
                {
                    PropertyName = validation.PropertyName,
                    ErrorType = "UnsafeConversion",
                    Message = $"'{sourceType.Name}' tipinden '{targetType.Name}' tipine güvenli dönüşüm mümkün değil",
                    Severity = ValidationSeverity.Error,
                    SuggestedFix = "Custom type converter tanımlayın"
                });
            }

            // Veri kaybı kontrolü
            if (WillLoseData(sourceType, targetType))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    PropertyName = validation.PropertyName,
                    WarningType = "DataLoss",
                    Message = $"Dönüşüm sırasında veri kaybı olabilir",
                    Severity = ValidationSeverity.Warning
                });
            }

            return result;
        }

        /// <summary>
        /// Genel validation kuralları
        /// </summary>
        private static void ValidateGeneralRules(ValidationResult result, Type sourceType, Type targetType)
        {
            // Circular reference kontrolü
            if (HasCircularReference(sourceType, targetType))
            {
                result.Errors.Add(new ValidationError
                {
                    PropertyName = "General",
                    ErrorType = "CircularReference",
                    Message = "Circular reference tespit edildi",
                    Severity = ValidationSeverity.Critical,
                    SuggestedFix = "Mapping yapısını yeniden tasarlayın"
                });
            }

            // Performance kontrolü
            var sourceProps = sourceType.GetProperties().Length;
            var targetProps = targetType.GetProperties().Length;
            
            if (sourceProps > 50 || targetProps > 50)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    PropertyName = "General",
                    WarningType = "LargeObject",
                    Message = $"Büyük nesne ({sourceProps} source, {targetProps} target property) performans sorunlarına neden olabilir",
                    Severity = ValidationSeverity.Warning
                });
            }

            // Mapping coverage kontrolü
            var coverage = (double)result.MappedProperties / result.TotalProperties;
            if (coverage < 0.5)
            {
                result.Warnings.Add(new ValidationWarning
                {
                    PropertyName = "General",
                    WarningType = "LowCoverage",
                    Message = $"Düşük mapping coverage (%{coverage:P0})",
                    Severity = ValidationSeverity.Warning
                });
            }
        }

        /// <summary>
        /// Property validation bilgilerini al
        /// </summary>
        private static PropertyValidationInfo[] GetPropertyValidationInfo(Type type)
        {
            if (_propertyValidationCache.TryGetValue(type, out var cached))
                return cached;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var infos = new PropertyValidationInfo[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                var prop = properties[i];
                infos[i] = new PropertyValidationInfo
                {
                    PropertyName = prop.Name,
                    SourceType = prop.PropertyType,
                    TargetType = prop.PropertyType,
                    CanMap = prop.CanRead && prop.CanWrite,
                    IsNullable = IsNullableType(prop.PropertyType),
                    IsCollection = IsCollectionType(prop.PropertyType),
                    ElementType = IsCollectionType(prop.PropertyType) ? GetCollectionElementType(prop.PropertyType) : null,
                    Depth = CalculateNestingDepth(prop.PropertyType)
                };
            }

            _propertyValidationCache[type] = infos;
            return infos;
        }

        /// <summary>
        /// Nullable tip kontrolü
        /// </summary>
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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
        /// Nested depth hesapla
        /// </summary>
        private static int CalculateNestingDepth(Type type, int currentDepth = 0)
        {
            if (currentDepth > 10) return currentDepth; // Sonsuz döngü koruması

            if (IsCollectionType(type))
            {
                var elementType = GetCollectionElementType(type);
                return CalculateNestingDepth(elementType, currentDepth + 1);
            }

            return currentDepth;
        }

        /// <summary>
        /// Güvenli dönüşüm kontrolü
        /// </summary>
        private static bool IsSafeConversion(Type sourceType, Type targetType)
        {
            if (sourceType == targetType) return true;
            if (targetType.IsAssignableFrom(sourceType)) return true;

            // Numeric conversions
            if (IsNumericType(sourceType) && IsNumericType(targetType)) return true;

            // String conversions
            if (sourceType == typeof(string) || targetType == typeof(string)) return true;

            // Enum conversions
            if (sourceType.IsEnum || targetType.IsEnum) return true;

            return false;
        }

        /// <summary>
        /// Veri kaybı kontrolü
        /// </summary>
        private static bool WillLoseData(Type sourceType, Type targetType)
        {
            // Numeric precision loss
            if (sourceType == typeof(decimal) && targetType == typeof(double)) return true;
            if (sourceType == typeof(double) && targetType == typeof(float)) return true;
            if (sourceType == typeof(long) && targetType == typeof(int)) return true;

            return false;
        }

        /// <summary>
        /// Circular reference kontrolü
        /// </summary>
        private static bool HasCircularReference(Type sourceType, Type targetType)
        {
            // Basit circular reference kontrolü
            return sourceType == targetType;
        }

        /// <summary>
        /// Numeric tip kontrolü
        /// </summary>
        private static bool IsNumericType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return typeCode >= TypeCode.SByte && typeCode <= TypeCode.Decimal;
        }

        /// <summary>
        /// Dönüşüm tipini belirle
        /// </summary>
        private static string DetermineConversionType(Type sourceType, Type targetType)
        {
            if (IsNumericType(sourceType) && IsNumericType(targetType))
                return "NumericConversion";
            
            if (sourceType == typeof(string) || targetType == typeof(string))
                return "StringConversion";
            
            if (sourceType.IsEnum || targetType.IsEnum)
                return "EnumConversion";
            
            return "CustomConversion";
        }

        /// <summary>
        /// Type key oluştur
        /// </summary>
        private static long GetTypeKey(Type sourceType, Type targetType)
        {
            return ((long)sourceType.GetHashCode() << 32) | (uint)targetType.GetHashCode();
        }

        /// <summary>
        /// Validation cache'ini temizle
        /// </summary>
        public static void ClearValidationCache()
        {
            _validationCache.Clear();
            _propertyValidationCache.Clear();
        }

        /// <summary>
        /// Validation istatistiklerini al
        /// </summary>
        public static (int ValidationCache, int PropertyCache) GetValidationStats()
        {
            return (_validationCache.Count, _propertyValidationCache.Count);
        }
    }
} 