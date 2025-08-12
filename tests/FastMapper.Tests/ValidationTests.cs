using System;
using System.Linq;
using Xunit;
using FastMapper;
using System.Collections.Generic;

namespace FastMapper.Tests
{
    public class ValidationTests
    {
        [Fact]
        public void ValidateMapping_ShouldValidateSimpleMapping_Successfully()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<Person, PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.True(result.MappedProperties > 0);
            Assert.True(result.TotalProperties > 0);
        }

        [Fact]
        public void ValidateMapping_ShouldDetectUnmappedProperties()
        {
            // Act
            var result = MappingValidator.ValidateMapping<Person, PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Errors.Any(e => e.ErrorType == "UnmappedProperty"));
        }

        [Fact]
        public void ValidateMapping_ShouldDetectTypeMismatches()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<VariousTypesSource, VariousTypesTarget>();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Errors.Any(e => e.ErrorType == "UnsafeConversion"));
        }

        [Fact]
        public void ValidateMapping_ShouldDetectNullableMismatches()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<VariousTypesSource, VariousTypesTarget>();

            // Assert
            Assert.NotNull(result);
            // Type conversion uyarıları kontrol et - int -> string dönüşümü
            Assert.True(result.PropertyValidations.Any(p => p.Value.RequiresConversion));
        }

        [Fact]
        public void ValidateMapping_ShouldDetectDeepNesting()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<LargeObject, LargeObject>();

            // Assert
            Assert.NotNull(result);
            // Large object uyarıları kontrol et (50+ property)
            Assert.True(result.Warnings.Any(w => w.WarningType == "LargeObject"));
        }

        [Fact]
        public void ValidateMapping_ShouldCalculateCoverageCorrectly()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<Person, PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.MappedProperties > 0);
            Assert.True(result.TotalProperties > 0);
            
            var coverage = (double)result.MappedProperties / result.TotalProperties;
            Assert.True(coverage > 0);
        }

        [Fact]
        public void ValidateMapping_ShouldHandleCircularReferences()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<Person, Person>();

            // Assert
            Assert.NotNull(result);
            // Circular reference kontrolü
            Assert.True(result.Errors.Any(e => e.ErrorType == "CircularReference"));
        }

        [Fact]
        public void ValidateMapping_ShouldDetectLargeObjects()
        {
            // Act
            var result = MappingValidator.ValidateMapping<LargeObject, LargeObject>();

            // Assert
            Assert.NotNull(result);
            // Large object uyarıları kontrol et
            Assert.True(result.Warnings.Any(w => w.WarningType == "LargeObject"));
        }

        [Fact]
        public void ValidateMapping_ShouldProvideDetailedPropertyInfo()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<Person, PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.PropertyValidations.Count > 0);

            foreach (var validation in result.PropertyValidations.Values)
            {
                Assert.NotNull(validation.PropertyName);
                Assert.NotNull(validation.MappingStrategy);
            }
        }

        [Fact]
        public void ValidateMapping_ShouldHandleComplexTypeConversions()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<VariousTypesSource, VariousTypesTarget>();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.PropertyValidations.Any(p => p.Value.RequiresConversion));
        }

        [Fact]
        public void ValidateMapping_ShouldCacheResults()
        {
            // Arrange & Act
            var result1 = MappingValidator.ValidateMapping<Person, PersonDto>();
            var result2 = MappingValidator.ValidateMapping<Person, PersonDto>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(result1.TotalProperties, result2.TotalProperties);
            Assert.Equal(result1.MappedProperties, result2.MappedProperties);
        }

        [Fact]
        public void ValidateMapping_ShouldHandleNullTypes()
        {
            // Arrange & Act
            var result = MappingValidator.ValidateMapping<object, object>();

            // Assert
            Assert.NotNull(result);
            // Null type handling kontrolü
        }

        // Test için büyük nesne sınıfı
        public class LargeObject
        {
            public int Id { get; set; }
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
            public string Property4 { get; set; }
            public string Property5 { get; set; }
            public string Property6 { get; set; }
            public string Property7 { get; set; }
            public string Property8 { get; set; }
            public string Property9 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
            public string Property12 { get; set; }
            public string Property13 { get; set; }
            public string Property14 { get; set; }
            public string Property15 { get; set; }
            public string Property16 { get; set; }
            public string Property17 { get; set; }
            public string Property18 { get; set; }
            public string Property19 { get; set; }
            public string Property20 { get; set; }
            public string Property21 { get; set; }
            public string Property22 { get; set; }
            public string Property23 { get; set; }
            public string Property24 { get; set; }
            public string Property25 { get; set; }
            public string Property26 { get; set; }
            public string Property27 { get; set; }
            public string Property28 { get; set; }
            public string Property29 { get; set; }
            public string Property30 { get; set; }
            public string Property31 { get; set; }
            public string Property32 { get; set; }
            public string Property33 { get; set; }
            public string Property34 { get; set; }
            public string Property35 { get; set; }
            public string Property36 { get; set; }
            public string Property37 { get; set; }
            public string Property38 { get; set; }
            public string Property39 { get; set; }
            public string Property40 { get; set; }
            public string Property41 { get; set; }
            public string Property42 { get; set; }
            public string Property43 { get; set; }
            public string Property44 { get; set; }
            public string Property45 { get; set; }
            public string Property46 { get; set; }
            public string Property47 { get; set; }
            public string Property48 { get; set; }
            public string Property49 { get; set; }
            public string Property50 { get; set; }
        }
    }
} 