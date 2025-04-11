using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using FastMapper;

namespace FastMapper.Tests
{
    public class PerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MeasurePerformanceForSimpleObjects()
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

            int iterations = 10000;
            
            // Isınma turları
            for (int i = 0; i < 100; i++)
            {
                source.FastMapTo<PersonDto>();
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                source.FastMapTo<PersonDto>();
            }
            
            stopwatch.Stop();
            
            // Assert / Output
            _output.WriteLine($"Basit nesne mapping: {iterations} işlem, toplam süre: {stopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Ortalama işlem süresi: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            
            // Test geçerli mi?
            Assert.True(stopwatch.ElapsedMilliseconds > 0);
        }

        [Fact]
        public void MeasurePerformanceForComplexObjects()
        {
            // Arrange
            var source = CreateComplexPerson();
            int iterations = 1000;
            
            // Isınma turları
            for (int i = 0; i < 10; i++)
            {
                source.FastMapTo<PersonDto>();
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                source.FastMapTo<PersonDto>();
            }
            
            stopwatch.Stop();
            
            // Assert / Output
            _output.WriteLine($"Karmaşık nesne mapping: {iterations} işlem, toplam süre: {stopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Ortalama işlem süresi: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            
            // Test geçerli mi?
            Assert.True(stopwatch.ElapsedMilliseconds > 0);
        }

        [Fact]
        public void MeasurePerformanceForListMapping()
        {
            // Arrange
            List<Person> sourceList = new List<Person>();
            for (int i = 0; i < 100; i++)
            {
                sourceList.Add(new Person
                {
                    Id = i,
                    FirstName = $"FirstName{i}",
                    LastName = $"LastName{i}",
                    BirthDate = DateTime.Now.AddYears(-30).AddDays(i),
                    IsActive = i % 2 == 0
                });
            }
            
            int iterations = 10;
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                List<PersonDto> targetList = new List<PersonDto>();
                foreach (var person in sourceList)
                {
                    targetList.Add(person.FastMapTo<PersonDto>());
                }
            }
            
            stopwatch.Stop();
            
            // Assert / Output
            _output.WriteLine($"Liste mapping (100 öğe): {iterations} işlem, toplam süre: {stopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Ortalama işlem süresi: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            
            // Test geçerli mi?
            Assert.True(stopwatch.ElapsedMilliseconds > 0);
        }

        [Fact]
        public void MeasurePerformanceWithCustomMapping()
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

            int iterations = 10000;
            
            // Isınma turları
            for (int i = 0; i < 100; i++)
            {
                source.FastMapTo<PersonDto>();
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                source.FastMapTo<PersonDto>();
            }
            
            stopwatch.Stop();
            
            // Assert / Output
            _output.WriteLine($"Özel mapping ile: {iterations} işlem, toplam süre: {stopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Ortalama işlem süresi: {(double)stopwatch.ElapsedMilliseconds / iterations} ms");
            
            // Test geçerli mi?
            Assert.True(stopwatch.ElapsedMilliseconds > 0);
            
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
        }

        [Fact]
        public void CompareMappingWithAndWithoutCustomMapping()
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

            int iterations = 10000;
            
            // 1. Standart mapping ölçümü
            var stopwatch1 = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                source.FastMapTo<PersonDto>();
            }
            
            stopwatch1.Stop();
            var standardMappingTime = stopwatch1.ElapsedMilliseconds;

            // 2. Özel eşleştirme kural tanımla
            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "FullName",
                person => $"{person.FirstName} {person.LastName}"
            );

            MapperExtensions.AddCustomMapping<Person, PersonDto, string>(
                "Status",
                person => person.IsActive ? "Aktif" : "Pasif"
            );

            // Özel mapping ölçümü
            var stopwatch2 = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                source.FastMapTo<PersonDto>();
            }
            
            stopwatch2.Stop();
            var customMappingTime = stopwatch2.ElapsedMilliseconds;
            
            // Assert / Output
            _output.WriteLine($"Standart mapping: {iterations} işlem, toplam süre: {standardMappingTime} ms");
            _output.WriteLine($"Özel mapping: {iterations} işlem, toplam süre: {customMappingTime} ms");
            _output.WriteLine($"Fark: {customMappingTime - standardMappingTime} ms");
            _output.WriteLine($"Özel mapping, standart mapping'e göre %{(double)(customMappingTime - standardMappingTime) / standardMappingTime * 100:F2} daha {(customMappingTime > standardMappingTime ? "yavaş" : "hızlı")}");
            
            // Temizlik
            MapperExtensions.ClearAllCustomMappings();
        }

        // Yardımcı metotlar
        private Person CreateComplexPerson()
        {
            return new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1980, 1, 1),
                IsActive = true,
                HomeAddress = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    Country = "USA",
                    PostalCode = "10001"
                },
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
                                ProductName = "Product 1",
                                Quantity = 2,
                                UnitPrice = 25.5m
                            },
                            new OrderItem
                            {
                                ItemId = 1002,
                                ProductName = "Product 2",
                                Quantity = 1,
                                UnitPrice = 99.75m
                            }
                        }
                    },
                    new Order
                    {
                        OrderId = 102,
                        TotalAmount = 245.50m,
                        OrderDate = DateTime.Now.AddDays(-2),
                        Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ItemId = 1003,
                                ProductName = "Product 3",
                                Quantity = 3,
                                UnitPrice = 45.0m
                            },
                            new OrderItem
                            {
                                ItemId = 1004,
                                ProductName = "Product 4",
                                Quantity = 2,
                                UnitPrice = 55.25m
                            }
                        }
                    }
                }
            };
        }
    }
} 