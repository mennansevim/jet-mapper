using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FastMapper;

namespace FastMapper.Tests
{
    public class DiffMappingTests
    {
        [Fact]
        public void FindDifferences_ShouldDetectValueChanges_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = false
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Differences.Count > 0);
            Assert.True(result.SimilarityPercentage < 100);
        }

        [Fact]
        public void FindDifferences_ShouldDetectNoDifferences_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasDifferences);
            Assert.Equal(100, result.SimilarityPercentage);
        }

        [Fact]
        public void FindDifferences_ShouldHandleNullValues_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = null,
                IsActive = true
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Differences.Any(d => d.DiffType == DiffMapper.DiffType.NullMismatch));
        }

        [Fact]
        public void FindDifferences_ShouldDetectCollectionChanges_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                Orders = new List<Order>
                {
                    new Order { OrderId = 1, TotalAmount = 100 }
                }
            };

            var target = new Person
            {
                Id = 1,
                Orders = new List<Order>
                {
                    new Order { OrderId = 1, TotalAmount = 100 },
                    new Order { OrderId = 2, TotalAmount = 200 }
                }
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Differences.Any(d => d.DiffType == DiffMapper.DiffType.CollectionChanged));
        }

        [Fact]
        public void FindDifferences_ShouldDetectTypeMismatches_Successfully()
        {
            // Arrange
            var source = new VariousTypesSource
            {
                IntValue = 42,
                StringValue = "42"
            };

            var target = new VariousTypesTarget
            {
                IntValue = "42",
                StringValue = 42
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Differences.Any(d => d.DiffType == DiffMapper.DiffType.TypeMismatch));
        }

        [Fact]
        public void FindDifferences_ShouldCalculateSimilarityScore_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith"
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.SimilarityPercentage > 0);
            Assert.True(result.SimilarityPercentage < 100);
        }

        [Fact]
        public void FindDifferences_ShouldHandleComplexNestedObjects_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                HomeAddress = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    Country = "USA"
                }
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "John",
                HomeAddress = new Address
                {
                    Street = "456 Oak Ave",
                    City = "Los Angeles",
                    Country = "USA"
                }
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Differences.Any(d => d.DiffType == DiffMapper.DiffType.StructureChanged));
        }

        [Fact]
        public void FindDifferences_ShouldHandleNullObjects_Successfully()
        {
            // Arrange
            Person source = null;
            Person target = new Person { Id = 1 };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Differences.Any(d => d.DiffType == DiffMapper.DiffType.NullMismatch));
        }

        [Fact]
        public void FindDifferences_ShouldHandleBothNullObjects_Successfully()
        {
            // Arrange
            Person source = null;
            Person target = null;

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasDifferences);
            Assert.Equal(100, result.SimilarityPercentage);
        }

        [Fact]
        public void FindDifferences_ShouldDetectCriticalDifferences_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John"
            };

            var target = new Person
            {
                Id = 2, // Critical difference
                FirstName = "John"
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            Assert.True(result.Summary.CriticalDifferences > 0);
        }

        [Fact]
        public void FindDifferences_ShouldProvideDetailedSummary_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = false
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Summary);
            Assert.True(result.Summary.MajorDifferences > 0);
            Assert.NotNull(result.Summary.OverallAssessment);
        }

        [Fact]
        public void FindDifferences_ShouldHandleStringSimilarity_Successfully()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            var target = new Person
            {
                Id = 1,
                FirstName = "Jon",
                LastName = "Doe"
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            
            var firstNameDiff = result.Differences.FirstOrDefault(d => d.PropertyName == "FirstName");
            Assert.NotNull(firstNameDiff);
            Assert.True(firstNameDiff.SimilarityScore > 0);
            Assert.True(firstNameDiff.SimilarityScore < 1.0);
        }

        [Fact]
        public void FindDifferences_ShouldHandleNumericDifferences_Successfully()
        {
            // Arrange
            var source = new Order
            {
                OrderId = 1,
                TotalAmount = 100.50m
            };

            var target = new Order
            {
                OrderId = 1,
                TotalAmount = 150.75m
            };

            // Act
            var result = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.HasDifferences);
            
            var amountDiff = result.Differences.FirstOrDefault(d => d.PropertyName == "TotalAmount");
            Assert.NotNull(amountDiff);
            Assert.Equal(DiffMapper.DiffType.ValueChanged, amountDiff.DiffType);
        }

        [Fact]
        public void FindDifferences_ShouldCacheResults_Successfully()
        {
            // Arrange
            var source = new Person { Id = 1, FirstName = "John" };
            var target = new Person { Id = 1, FirstName = "Jane" };

            // Act
            var result1 = DiffMapper.FindDifferences(source, target);
            var result2 = DiffMapper.FindDifferences(source, target);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(result1.HasDifferences, result2.HasDifferences);
            Assert.Equal(result1.SimilarityPercentage, result2.SimilarityPercentage);
        }
    }
} 