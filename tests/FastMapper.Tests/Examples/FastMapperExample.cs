using System;
using System.Collections.Generic;
using FastMapper;

namespace FastMapper.Tests.Examples
{
    public class FastMapperExample
    {
        // Bu sınıf örnek kullanım senaryolarını gösterir
        public static void RunAllExamples()
        {
            Console.WriteLine("=== FastMapper Örnek Uygulaması ===\n");
            
            BasicExample();
            CustomMappingExample();
            NestedObjectsExample();
            CollectionsExample();
        }

        public static void BasicExample()
        {
            Console.WriteLine("--- Temel Mapping Örneği ---");
            
            // Kaynak nesne oluştur
            var person = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                IsActive = true
            };
            
            // Hedef nesne oluştur (FastMapTo kullanarak)
            var personDto = person.FastMapTo<PersonDto>();
            
            // Sonuçları yazdır
            Console.WriteLine($"ID: {personDto.Id}");
            Console.WriteLine($"Ad: {personDto.FirstName}");
            Console.WriteLine($"Soyad: {personDto.LastName}");
            Console.WriteLine($"Yaş: {personDto.Age}");
            Console.WriteLine();
        }

        public static void CustomMappingExample()
        {
            Console.WriteLine("--- Özel Mapping Örneği ---");
            
            // Özel eşleştirme kurallarını temizle
            MapperExtensions.ClearAllCustomMappings();
            
            // Kaynak nesne oluştur
            var person = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                IsActive = true
            };
            
            // Özel eşleştirme kuralları tanımla
            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "FullName",
                p => $"{p.FirstName} {p.LastName}"
            );
            
            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "Status",
                p => p.IsActive ? "Aktif" : "Pasif"
            );
            
            // Hedef nesne oluştur (FastMapTo kullanarak)
            var personDto = person.FastMapTo<PersonDto>();
            
            // Sonuçları yazdır
            Console.WriteLine($"ID: {personDto.Id}");
            Console.WriteLine($"Tam Ad: {personDto.FullName}");
            Console.WriteLine($"Durum: {personDto.Status}");
            Console.WriteLine();
            
            // Özel eşleştirme kurallarını temizle
            MapperExtensions.ClearAllCustomMappings();
        }

        public static void NestedObjectsExample()
        {
            Console.WriteLine("--- İç İçe Nesneler Örneği ---");
            
            // Özel eşleştirme kurallarını temizle
            MapperExtensions.ClearAllCustomMappings();
            
            // Kaynak nesne oluştur
            var person = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                HomeAddress = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    Country = "USA",
                    PostalCode = "10001"
                }
            };
            
            // Özel eşleştirme kuralları tanımla
            MapperExtensions.AddCustomMapping<Address, AddressDto, string>(
                "FullAddress",
                a => $"{a.Street}, {a.City}, {a.Country} {a.PostalCode}"
            );
            
            // Hedef nesne oluştur (FastMapTo kullanarak)
            var personDto = person.FastMapTo<PersonDto>();
            
            // Sonuçları yazdır
            Console.WriteLine($"ID: {personDto.Id}");
            Console.WriteLine($"Ad: {personDto.FirstName}");
            Console.WriteLine($"Soyad: {personDto.LastName}");
            Console.WriteLine("Adres:");
            Console.WriteLine($"  Sokak: {personDto.HomeAddress.Street}");
            Console.WriteLine($"  Şehir: {personDto.HomeAddress.City}");
            Console.WriteLine($"  Ülke: {personDto.HomeAddress.Country}");
            Console.WriteLine($"  Tam Adres: {personDto.HomeAddress.FullAddress}");
            Console.WriteLine();
            
            // Özel eşleştirme kurallarını temizle
            MapperExtensions.ClearAllCustomMappings();
        }

        public static void CollectionsExample()
        {
            Console.WriteLine("--- Koleksiyonlar Örneği ---");
            
            // Kaynak nesne oluştur
            var person = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                Orders = new List<Order>
                {
                    new Order
                    {
                        OrderId = 101,
                        TotalAmount = 150.75m,
                        OrderDate = DateTime.Now.AddDays(-5),
                        Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ItemId = 1001,
                                ProductName = "Ürün 1",
                                Quantity = 2,
                                UnitPrice = 25.5m
                            },
                            new OrderItem
                            {
                                ItemId = 1002,
                                ProductName = "Ürün 2",
                                Quantity = 1,
                                UnitPrice = 99.75m
                            }
                        }
                    }
                }
            };
            
            // Özel eşleştirme kuralları
            MapperExtensions.AddCustomMapping<Order, OrderDto, string>(
                "TotalAmount",
                o => $"{o.TotalAmount:C}"
            );
            
            MapperExtensions.AddCustomMapping<Order, OrderDto, string>(
                "OrderDate",
                o => o.OrderDate.ToString("dd.MM.yyyy")
            );
            
            MapperExtensions.AddCustomMapping<OrderItem, OrderItemDto, string>(
                "UnitPrice",
                item => $"{item.UnitPrice:C}"
            );
            
            MapperExtensions.AddCustomMapping<OrderItem, OrderItemDto, string>(
                "TotalPrice",
                item => $"{item.Quantity * item.UnitPrice:C}"
            );
            
            // Hedef nesne oluştur (FastMapTo kullanarak)
            var personDto = person.FastMapTo<PersonDto>();
            
            // Sonuçları yazdır
            Console.WriteLine($"ID: {personDto.Id}");
            Console.WriteLine($"Ad: {personDto.FirstName}");
            Console.WriteLine($"Soyad: {personDto.LastName}");
            Console.WriteLine("Siparişler:");
            
            foreach (var order in personDto.Orders)
            {
                Console.WriteLine($"  Sipariş ID: {order.OrderId}");
                Console.WriteLine($"  Toplam Tutar: {order.TotalAmount}");
                Console.WriteLine($"  Sipariş Tarihi: {order.OrderDate}");
                Console.WriteLine("  Ürünler:");
                
                foreach (var item in order.Items)
                {
                    Console.WriteLine($"    - {item.ProductName} x {item.Quantity} = {item.TotalPrice}");
                }
            }
            
            Console.WriteLine();
            
            // Özel eşleştirme kurallarını temizle
            MapperExtensions.ClearAllCustomMappings();
        }
    }
} 