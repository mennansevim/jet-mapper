using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using FastMapper;

namespace FastMapper.Tests
{
    public class CollectionMappingTests
    {
        [Fact]
        public void CanMapListToList()
        {
            // Arrange
            var sourceList = new List<Person>
            {
                new Person { Id = 1, FirstName = "John", LastName = "Doe", BirthDate = new DateTime(1980, 1, 1) },
                new Person { Id = 2, FirstName = "Jane", LastName = "Smith", BirthDate = new DateTime(1985, 5, 10) },
                new Person { Id = 3, FirstName = "Bob", LastName = "Johnson", BirthDate = new DateTime(1975, 3, 15) }
            };

            // Act
            var targetList = new List<PersonDto>();
            foreach (var person in sourceList)
            {
                targetList.Add(person.FastMapTo<PersonDto>());
            }

            // Assert
            Assert.Equal(sourceList.Count, targetList.Count);
            for (int i = 0; i < sourceList.Count; i++)
            {
                Assert.Equal(sourceList[i].Id, targetList[i].Id);
                Assert.Equal(sourceList[i].FirstName, targetList[i].FirstName);
                Assert.Equal(sourceList[i].LastName, targetList[i].LastName);
                Assert.Equal(sourceList[i].Age, targetList[i].Age);
            }
        }

        [Fact]
        public void CanMapNestedCollections()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Orders = new List<Order>
                {
                    new Order { OrderId = 101, TotalAmount = 150.75m, OrderDate = DateTime.Now.AddDays(-5) },
                    new Order { OrderId = 102, TotalAmount = 245.50m, OrderDate = DateTime.Now.AddDays(-2) }
                }
            };

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.NotNull(target.Orders);
            Assert.Equal(source.Orders.Count, target.Orders.Count);
            for (int i = 0; i < source.Orders.Count; i++)
            {
                Assert.Equal(source.Orders[i].OrderId, target.Orders[i].OrderId);
                Assert.Equal(source.Orders[i].TotalAmount.ToString(), target.Orders[i].TotalAmount);
                Assert.Equal(source.Orders[i].OrderDate.ToString(), target.Orders[i].OrderDate);
            }
        }

        [Fact]
        public void CanMapArrayToList()
        {
            // Arrange
            var sourceArray = new Address[]
            {
                new Address { Street = "123 Main St", City = "New York", Country = "USA", PostalCode = "10001" },
                new Address { Street = "456 Elm St", City = "Chicago", Country = "USA", PostalCode = "60007" }
            };

            // Act
            var targetList = new List<AddressDto>();
            foreach (var address in sourceArray)
            {
                targetList.Add(address.FastMapTo<AddressDto>());
            }

            // Assert
            Assert.Equal(sourceArray.Length, targetList.Count);
            for (int i = 0; i < sourceArray.Length; i++)
            {
                Assert.Equal(sourceArray[i].Street, targetList[i].Street);
                Assert.Equal(sourceArray[i].City, targetList[i].City);
                Assert.Equal(sourceArray[i].Country, targetList[i].Country);
                Assert.Equal(sourceArray[i].PostalCode, targetList[i].PostalCode);
            }
        }

        [Fact]
        public void CanHandleEmptyCollections()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Orders = new List<Order>() // Boş liste
            };

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.NotNull(target.Orders);
            Assert.Empty(target.Orders);
        }

        [Fact]
        public void CanHandleNullCollections()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Orders = null // Bilinçli olarak null
            };

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.Null(target.Orders);
        }
    }
} 