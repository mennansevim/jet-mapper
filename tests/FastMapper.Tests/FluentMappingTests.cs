using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FastMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FastMapper.Tests
{
    public class FluentMappingTests
    {
        [Fact]
        public void FluentMap_ShouldMapWithCustomMapping_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var result = source.Map()
                .Map<PersonDto>(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
                .Map<PersonDto>(dto => dto.Status, p => p.IsActive ? "Active" : "Inactive")
                .To<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("Active", result.Status);
            Assert.Equal(source.Id, result.Id);
        }

        [Fact]
        public void FluentMap_ShouldIgnoreProperties_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var result = source.Map()
                .Ignore<PersonDto>(dto => dto.Status)
                .Ignore<PersonDto>(dto => dto.FullName)
                .To<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Status);
            Assert.Null(result.FullName);
            Assert.Equal(source.Id, result.Id);
            Assert.Equal(source.FirstName, result.FirstName);
        }

        [Fact]
        public void FluentMap_ShouldMapConditionally_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            // Act
            var result = source.Map()
                .MapIf<PersonDto>(dto => dto.Status, 
                    p => p.IsActive, 
                    p => "Active")
                .MapIf<PersonDto>(dto => dto.FullName, 
                    p => !string.IsNullOrEmpty(p.FirstName), 
                    p => $"{p.FirstName} {p.LastName}")
                .To<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Active", result.Status);
            Assert.Equal("John Doe", result.FullName);
        }

        [Fact]
        public void FluentMap_ShouldExecuteBeforeAndAfterActions_Successfully()
        {
            // Arrange
            var source = new Person { Id = 1, FirstName = "John" };
            var beforeMapCalled = false;
            var afterMapCalled = false;

            // Act
            var result = source.Map()
                .BeforeMap((p, dto) => beforeMapCalled = true)
                .AfterMap((p, dto) => afterMapCalled = true)
                .To<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.True(beforeMapCalled);
            Assert.True(afterMapCalled);
        }

        [Fact]
        public void FluentMap_ShouldMapToExistingObject_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var existingTarget = new PersonDto
            {
                Id = 999,
                Status = "Old Status"
            };

            // Act
            var result = source.Map()
                .Map<PersonDto>(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
                .To(existingTarget);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(source.Id, result.Id);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("Old Status", result.Status); // Değişmemeli
        }

        [Fact]
        public async Task FluentMap_ShouldMapAsync_Successfully()
        {
            // Arrange
            var sources = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe" },
                new Person { Id = 2, FirstName = "Jane", LastName = "Smith" },
                new Person { Id = 3, FirstName = "Bob", LastName = "Johnson" }
            };

            // Act
            var results = await sources.Map()
                .Map<PersonDto>(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
                .ToListAsync<PersonDto>();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
            Assert.Equal("John Doe", results[0].FullName);
            Assert.Equal("Jane Smith", results[1].FullName);
            Assert.Equal("Bob Johnson", results[2].FullName);
        }

        [Fact]
        public void FluentMap_ShouldHandleNullSource_Gracefully()
        {
            // Arrange
            Person source = null;

            // Act & Assert
            var result = source?.Map()?.To<PersonDto>();
            Assert.Null(result);
        }

        [Fact]
        public void FluentMap_ShouldHandleComplexNestedMapping_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                HomeAddress = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    Country = "USA"
                }
            };

            // Act
            var result = source.Map()
                .Map<PersonDto>(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
                .Map<PersonDto>(dto => dto.HomeAddress.FullAddress, 
                    p => $"{p.HomeAddress.Street}, {p.HomeAddress.City}, {p.HomeAddress.Country}")
                .To<PersonDto>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("123 Main St, New York, USA", result.HomeAddress.FullAddress);
        }
    }
} 