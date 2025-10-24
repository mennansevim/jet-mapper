using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using JetMapper;

namespace JetMapper.Tests
{
    public class CollectionMappingTests
    {
        [Fact]
        public void FastMapToList_ShouldMapSimpleCollection_Successfully()
        {
            // Arrange
            var sourceList = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990, 1, 1) },
                new Person { Id = 2, FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1985, 5, 15) },
                new Person { Id = 3, FirstName = "Bob", LastName = "Johnson", BirthDate = new DateTime(1992, 10, 30) }
            };

            // Act
            var result = sourceList.FastMapToList<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            
            Assert.Equal(sourceList[0].Id, result[0].Id);
            Assert.Equal(sourceList[0].FirstName, result[0].FirstName);
            Assert.Equal(sourceList[0].LastName, result[0].LastName);
            
            Assert.Equal(sourceList[1].Id, result[1].Id);
            Assert.Equal(sourceList[1].FirstName, result[1].FirstName);
            Assert.Equal(sourceList[1].LastName, result[1].LastName);
            
            Assert.Equal(sourceList[2].Id, result[2].Id);
            Assert.Equal(sourceList[2].FirstName, result[2].FirstName);
            Assert.Equal(sourceList[2].LastName, result[2].LastName);
        }

        [Fact]
        public void FastMapToList_ShouldMapEmptyCollection_Successfully()
        {
            // Arrange
            var sourceList = new List<Person>();

            // Act
            var result = sourceList.FastMapToList<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void FastMapToList_ShouldMapSameTypes_Successfully()
        {
            // Arrange - Test with identical types
            var sourceList = new List<Address>
            {
                new Address { Street = "123 Main St", City = "New York", Country = "USA", PostalCode = "10001" },
                new Address { Street = "456 Oak Ave", City = "Los Angeles", Country = "USA", PostalCode = "90210" }
            };

            // Act
            var result = sourceList.FastMapToList<Address>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("123 Main St", result[0].Street);
            Assert.Equal("New York", result[0].City);
            Assert.Equal("456 Oak Ave", result[1].Street);
            Assert.Equal("Los Angeles", result[1].City);
        }

        [Fact]
        public void FastMapToList_ShouldMapLargeCollection_Efficiently()
        {
            // Arrange
            var sourceList = new List<Person>();
            for (int i = 1; i <= 100; i++)
            {
                sourceList.Add(new Person
                {
                    Id = i,
                    FirstName = $"Person{i}",
                    LastName = $"Lastname{i}",
                    BirthDate = new DateTime(1990, 1, 1).AddDays(i),
                    IsActive = i % 2 == 0
                });
            }

            // Act
            var result = sourceList.FastMapToList<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Count);
            Assert.Equal("Person1", result[0].FirstName);
            Assert.Equal("Person50", result[49].FirstName);
            Assert.Equal("Person100", result[99].FirstName);
        }

        [Fact]
        public void FastMapToList_ShouldReturnDifferentInstancesForEachCall_Successfully()
        {
            // Arrange
            var sourceList = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1990, 1, 1) }
            };

            // Act
            var result1 = sourceList.FastMapToList<PersonDto>();
            var result2 = sourceList.FastMapToList<PersonDto>();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2); // Different list instances
            Assert.NotSame(result1[0], result2[0]); // Different object instances
            Assert.Equal(result1[0].Id, result2[0].Id); // Same data
        }
    }
} 