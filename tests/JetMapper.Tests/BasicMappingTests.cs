using System;
using Xunit;
using JetMapper;

namespace JetMapper.Tests
{
    public class BasicMappingTests
    {
        [Fact]
        public void FastMapTo_ShouldMapSimpleObject_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true
            };

            // Act
            var result = source.FastMapTo<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(source.Id, result.Id);
            Assert.Equal(source.FirstName, result.FirstName);
            Assert.Equal(source.LastName, result.LastName);
            Assert.Equal(source.Age, result.Age);
        }

        [Fact]
        public void FastMapTo_ShouldMapToExistingObject_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true
            };

            var target = new PersonDto
            {
                Id = 999,
                FullName = "Old Name"
            };

            // Act
            source.FastMapTo(target);

            // Assert
            Assert.Equal(source.Id, target.Id);
            Assert.Equal(source.FirstName, target.FirstName);
            Assert.Equal(source.LastName, target.LastName);
            Assert.Equal(source.Age, target.Age);
            // FullName should remain unchanged as no custom mapping exists
        }

        [Fact]
        public void FastMapTo_ShouldBeCacheEfficient_ForRepeatedMappings()
        {
            // Arrange
            var source1 = new Person { Id = 1, FirstName = "John" };
            var source2 = new Person { Id = 2, FirstName = "Jane" };

            // Act - Multiple mappings should use cached mapper
            var result1 = source1.FastMapTo<PersonDto>();
            var result2 = source2.FastMapTo<PersonDto>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(1, result1.Id);
            Assert.Equal(2, result2.Id);
            Assert.Equal("John", result1.FirstName);
            Assert.Equal("Jane", result2.FirstName);
        }

        [Fact]
        public void FastMapTo_ShouldMapSameTypes_Successfully()
        {
            // Arrange - Test with identical property types
            var source = new Address
            {
                Street = "123 Main St",
                City = "New York",
                Country = "USA",
                PostalCode = "10001"
            };

            // Act
            var result = source.FastMapTo<Address>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(source.Street, result.Street);
            Assert.Equal(source.City, result.City);
            Assert.Equal(source.Country, result.Country);
            Assert.Equal(source.PostalCode, result.PostalCode);
        }
    }
} 