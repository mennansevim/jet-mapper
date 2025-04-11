using System;
using Xunit;
using System.Collections.Generic;
using FastMapper;

namespace FastMapper.Tests
{
    public class CustomMappingTests
    {
        [Fact]
        public void CanUseCustomPropertyMapping()
        {
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
            
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                IsActive = true
            };

            // Özel eşleştirme kural tanımla
            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "FullName",
                person => $"{person.FirstName} {person.LastName}"
            );

            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "Status",
                person => person.IsActive ? "Aktif" : "Pasif"
            );

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.Equal("John Doe", target.FullName);
            Assert.Equal("Aktif", target.Status);
            
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
        }

        [Fact]
        public void CanMapNestedObjectsWithCustomMappings()
        {
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
            
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
                    Country = "USA",
                    PostalCode = "10001"
                }
            };

            // Özel eşleştirme kural tanımla
            MapperExtensions.AddCustomMapping<Address, AddressDto, string>(
                "FullAddress",
                address => $"{address.Street}, {address.City}, {address.Country} {address.PostalCode}"
            );

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.NotNull(target.HomeAddress);
            Assert.Equal("123 Main St, New York, USA 10001", target.HomeAddress.FullAddress);
            
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
        }

        [Fact]
        public void CanRemoveSpecificCustomMapping()
        {
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
            
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe"
            };

            // Özel eşleştirme kural tanımla
            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "FullName",
                person => $"{person.FirstName} {person.LastName}"
            );

            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "Status",
                person => person.IsActive ? "Aktif" : "Pasif"
            );

            // Bir kuralı kaldır
            MapperExtensions.RemoveCustomMapping<Person, PersonDto>("Status");

            // Act
            var target = source.FastMapTo<PersonDto>();

            // Assert
            Assert.Equal("John Doe", target.FullName); // Bu kural hala var
            Assert.Null(target.Status); // Bu kural kaldırıldı
            
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
        }

        [Fact]
        public void CanUseCustomMappingWithComplexLogic()
        {
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
            
            // Arrange
            var source = new Order
            {
                OrderId = 101,
                TotalAmount = 150.75m,
                OrderDate = new DateTime(2023, 5, 15, 14, 30, 0),
                Items = new List<OrderItem>
                {
                    new OrderItem { ItemId = 1, ProductName = "Ürün 1", Quantity = 2, UnitPrice = 25.50m },
                    new OrderItem { ItemId = 2, ProductName = "Ürün 2", Quantity = 1, UnitPrice = 99.75m }
                }
            };

            // Özel eşleştirme kuralları tanımla, CultureInfo kullanarak sabit format
            MapperExtensions.AddCustomMapping<Order, OrderDto, string>(
                "OrderDate",
                order => order.OrderDate.ToString("dd.MM.yyyy HH:mm")
            );

            MapperExtensions.AddCustomMapping<Order, OrderDto, string>(
                "TotalAmount",
                order => "$" + order.TotalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            );

            MapperExtensions.AddCustomMapping<OrderItem, OrderItemDto, string>(
                "UnitPrice",
                item => "$" + item.UnitPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            );

            MapperExtensions.AddCustomMapping<OrderItem, OrderItemDto, string>(
                "TotalPrice",
                item => "$" + (item.Quantity * item.UnitPrice).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            );

            // Act
            var target = source.FastMapTo<OrderDto>();

            // Assert
            Assert.Equal("15.05.2023 14:30", target.OrderDate);
            Assert.Equal("$150.75", target.TotalAmount);
            Assert.NotNull(target.Items);
            Assert.Equal(2, target.Items.Count);
            Assert.Equal("$25.50", target.Items[0].UnitPrice);
            Assert.Equal("$51.00", target.Items[0].TotalPrice);
            Assert.Equal("$99.75", target.Items[1].UnitPrice);
            Assert.Equal("$99.75", target.Items[1].TotalPrice);
            
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
        }
    }
} 