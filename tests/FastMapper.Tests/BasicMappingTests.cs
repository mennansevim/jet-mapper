using System;
using Xunit;
using FastMapper;

namespace FastMapper.Tests
{
    public class BasicMappingTests
    {
        [Fact]
        public void CanMapSimpleProperties()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                IsActive = true
            };

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.Equal(source.Id, target.Id);
            Assert.Equal(source.FirstName, target.FirstName);
            Assert.Equal(source.LastName, target.LastName);
            Assert.Equal(source.Age, target.Age);
            Assert.Null(target.FullName); // Henüz özel eşleme yapmadık
        }

        [Fact]
        public void CanMapToExistingObject()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1)
            };

            var existingTarget = new PersonDto
            {
                Id = 999, // Bu değer değişecek
                FirstName = "Existing", // Bu değer değişecek
                FullName = "Should Remain" // Bu değer kaynak nesnede yok, değişmemeli
            };

            // Act
            source.FastMapTo(existingTarget);

            // Assert
            Assert.Equal(source.Id, existingTarget.Id); // Kaynak değer aktarılmış olmalı
            Assert.Equal(source.FirstName, existingTarget.FirstName); // Kaynak değer aktarılmış olmalı
            Assert.Equal(source.LastName, existingTarget.LastName); // Kaynak değer aktarılmış olmalı
            Assert.Equal("Should Remain", existingTarget.FullName); // Değişmemiş olmalı
        }

        [Fact]
        public void CanMapDifferentPropertyTypes()
        {
            // Arrange
            var source = new VariousTypesSource
            {
                IntValue = 42,
                LongValue = 2000000000, // Int'e sığabilecek bir değer kullanıyoruz
                DoubleValue = 3.14159,
                DecimalValue = 123.456m,
                StringValue = "42",
                DateValue = new DateTime(2023, 5, 15),
                TimeValue = TimeSpan.FromHours(2.5), // 9000000 milisaniye (2.5 saat)
                BoolValue = true,
                GuidValue = Guid.NewGuid(),
                EnumValue = DayOfWeek.Monday
            };

            // Act
            var target = source.FastMapTo<VariousTypesTarget>();

            // Assert
            Assert.Equal(source.IntValue.ToString(), target.IntValue);
            Assert.Equal((int)source.LongValue, target.LongValue);
            Assert.Equal((int)source.DoubleValue, target.DoubleValue);
            Assert.Equal((double)source.DecimalValue, target.DecimalValue);
            Assert.Equal(int.Parse(source.StringValue), target.StringValue);
            Assert.Equal(source.DateValue.ToString(), target.DateValue);
            
            // TimeSpan için doğru beklentiyi ayarlayalım: toplam milisaniye 9000000 (2.5 saat)
            long expectedMilliseconds = 9000000;
            Assert.Equal(expectedMilliseconds, target.TimeValue);
            
            Assert.Equal(source.BoolValue.ToString(), target.BoolValue);
            Assert.Equal(source.GuidValue.ToString(), target.GuidValue);
            Assert.Equal(source.EnumValue.ToString(), target.EnumValue);
        }

        [Fact]
        public void ShouldSkipReadOnlyProperties()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1)
            };

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            // Age özelliği Person sınıfında hesaplanmış bir özellik
            // FastMapper bu değeri hedef nesneye kopyalamalı
            Assert.Equal(source.Age, target.Age);
        }

        [Fact]
        public void CanHandleOverflowInNumericConversions()
        {
            // Arrange
            var source = new VariousTypesSource
            {
                LongValue = 9999999999 // Int'e sığmayacak bir değer
            };

            // Act
            var target = source.FastMapTo<VariousTypesTarget>();

            // Assert
            // Yeni geliştirdiğimiz güvenli dönüşüm özelliği sayesinde, 
            // long'dan int'e dönüşümde taşma olduğunda int.MaxValue değeri kullanılmalı
            Assert.Equal(int.MaxValue, target.LongValue);
        }

        [Fact]
        public void CanUseCustomTypeConverters()
        {
            // Temizlik
            MapperExtensions.ClearAllTypeConverters();
            
            // Arrange
            var source = new VariousTypesSource
            {
                LongValue = 9999999999, // Int'e sığmayacak bir değer
                TimeValue = TimeSpan.FromHours(2.5) // 9000000 milisaniye
            };

            // Özel tip dönüştürücü tanımla
            MapperExtensions.AddTypeConverter<long, int>(longValue => 
                longValue > int.MaxValue ? int.MaxValue / 2 : (int)longValue);
            
            // TimeSpan için özel dönüştürücü
            MapperExtensions.AddTypeConverter<TimeSpan, long>(timeSpan => 
                (long)timeSpan.TotalMilliseconds);

            // Act
            var target = source.FastMapTo<VariousTypesTarget>();

            // Assert
            // Özel dönüştürücü kullanıldığı için int.MaxValue / 2 değeri bekliyoruz
            Assert.Equal(int.MaxValue / 2, target.LongValue);
            // TimeSpan için 9000000 milisaniye bekliyoruz (2.5 saat)
            Assert.Equal(9000000, target.TimeValue);
            
            // Temizlik
            MapperExtensions.ClearAllTypeConverters();
        }
    }
} 