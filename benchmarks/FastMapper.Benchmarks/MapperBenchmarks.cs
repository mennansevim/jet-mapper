using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FastMapper.Benchmarks.Models;
using System;
using System.Collections.Generic;

namespace FastMapper.Benchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class MapperBenchmarks
    {
        private SimpleSource _simpleSource = default!;
        private Customer _complexSource = default!;
        private List<SimpleSource> _simpleList = default!;

        [GlobalSetup]
        public void Setup()
        {
            // Setup test data
            _simpleSource = new SimpleSource
            {
                Id = 1,
                Name = "Test Name",
                Description = "Test Description",
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _complexSource = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                BirthDate = new DateTime(1980, 1, 1),
                HomeAddress = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    State = "NY",
                    ZipCode = "10001",
                    Country = "USA"
                },
                Orders = new List<Order>
                {
                    new Order
                    {
                        OrderId = 1001,
                        Total = 99.95m,
                        OrderDate = DateTime.Now.AddDays(-5),
                        Status = "Completed",
                        Items = new List<OrderItem>
                        {
                            new OrderItem { ItemId = 1, ProductName = "Product 1", Quantity = 2, UnitPrice = 24.99m },
                            new OrderItem { ItemId = 2, ProductName = "Product 2", Quantity = 1, UnitPrice = 49.97m }
                        }
                    },
                    new Order
                    {
                        OrderId = 1002,
                        Total = 149.90m,
                        OrderDate = DateTime.Now.AddDays(-2),
                        Status = "Shipped",
                        Items = new List<OrderItem>
                        {
                            new OrderItem { ItemId = 3, ProductName = "Product 3", Quantity = 3, UnitPrice = 29.99m },
                            new OrderItem { ItemId = 4, ProductName = "Product 4", Quantity = 2, UnitPrice = 29.97m }
                        }
                    }
                }
            };

            // Create a list of simple objects for bulk mapping test
            _simpleList = new List<SimpleSource>();
            for (int i = 0; i < 1000; i++)
            {
                _simpleList.Add(new SimpleSource
                {
                    Id = i,
                    Name = $"Name {i}",
                    Description = $"Description {i}",
                    CreatedAt = DateTime.Now.AddDays(-i),
                    IsActive = i % 2 == 0
                });
            }

            // Configure FastMapper for custom mappings
            MapperExtensions.ClearAllCustomMappings();
            
            MapperExtensions.AddCustomMapping<Customer, CustomerDto, string>(
                "FullName",
                c => $"{c.FirstName} {c.LastName}"
            );

            MapperExtensions.AddCustomMapping<OrderItem, OrderItemDto, decimal>(
                "LineTotal",
                item => item.Quantity * item.UnitPrice
            );
        }

        [Benchmark(Baseline = true)]
        public SimpleDestination ManualMap_Simple()
        {
            return new SimpleDestination
            {
                Id = _simpleSource.Id,
                Name = _simpleSource.Name,
                Description = _simpleSource.Description,
                CreatedAt = _simpleSource.CreatedAt,
                IsActive = _simpleSource.IsActive
            };
        }

        [Benchmark]
        public SimpleDestination FastMapper_Simple()
        {
            return _simpleSource.FastMapTo<SimpleDestination>();
        }

        [Benchmark]
        public SimpleDestination FastMapper_Simple_ExistingObject()
        {
            var dest = new SimpleDestination();
            _simpleSource.FastMapTo(dest);
            return dest;
        }

        [Benchmark]
        public CustomerDto ManualMap_Complex()
        {
            var customer = _complexSource;
            var dto = new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                FullName = $"{customer.FirstName} {customer.LastName}",
                Email = customer.Email,
                BirthDate = customer.BirthDate,
                HomeAddress = new AddressDto
                {
                    Street = customer.HomeAddress.Street,
                    City = customer.HomeAddress.City,
                    State = customer.HomeAddress.State,
                    ZipCode = customer.HomeAddress.ZipCode,
                    Country = customer.HomeAddress.Country
                },
                Orders = new List<OrderDto>()
            };

            foreach (var order in customer.Orders)
            {
                var orderDto = new OrderDto
                {
                    OrderId = order.OrderId,
                    Total = order.Total,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    Items = new List<OrderItemDto>()
                };

                foreach (var item in order.Items)
                {
                    orderDto.Items.Add(new OrderItemDto
                    {
                        ItemId = item.ItemId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.Quantity * item.UnitPrice
                    });
                }

                dto.Orders.Add(orderDto);
            }

            return dto;
        }

        [Benchmark]
        public CustomerDto FastMapper_Complex()
        {
            return _complexSource.FastMapTo<CustomerDto>();
        }

        [Benchmark]
        public CustomerDto FastMapper_Complex_ExistingObject()
        {
            var dto = new CustomerDto();
            _complexSource.FastMapTo(dto);
            return dto;
        }

        [Benchmark]
        public List<SimpleDestination> ManualMap_BulkMapping()
        {
            var result = new List<SimpleDestination>();
            foreach (var item in _simpleList)
            {
                result.Add(new SimpleDestination
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    CreatedAt = item.CreatedAt,
                    IsActive = item.IsActive
                });
            }
            return result;
        }

        [Benchmark]
        public List<SimpleDestination> FastMapper_BulkMapping()
        {
            var result = new List<SimpleDestination>();
            foreach (var item in _simpleList)
            {
                result.Add(item.FastMapTo<SimpleDestination>());
            }
            return result;
        }

        [Benchmark]
        public CustomerDto FastMapper_WithCustomMapping()
        {
            return _complexSource.FastMapTo<CustomerDto>();
        }

        [Benchmark]
        public CustomerDto FastMapper_WithCombine()
        {
            return _complexSource.FastMapTo<CustomerDto>()
                .WithCombine("FullName", _complexSource, c => $"{c.FirstName} {c.LastName}");
        }

        [Benchmark]
        public CustomerDto FastMapper_WithMultipleCombines()
        {
            return _complexSource.FastMapTo<CustomerDto>()
                .WithMultipleCombines(_complexSource,
                    ("FullName", c => $"{c.FirstName} {c.LastName}"),
                    ("Email", c => $"{c.Email} (Verified)")
                );
        }

        [Benchmark]
        public CustomerDto FastMapper_TypeConverter()
        {
            MapperExtensions.ClearAllTypeConverters();
            MapperExtensions.AddTypeConverter<DateTime, string>(dt => dt.ToString("yyyy-MM-dd"));
            
            var dto = _complexSource.FastMapTo<CustomerDto>();
            
            MapperExtensions.ClearAllTypeConverters();
            return dto;
        }
    }
} 