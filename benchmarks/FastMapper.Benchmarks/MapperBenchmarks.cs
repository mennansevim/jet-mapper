using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using AutoMapper;
using FastMapper;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FastMapper.Benchmarks
{
    public class BenchmarkModels
    {
        public class SimpleSource
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Email { get; set; }
        }

        public class SimpleTarget
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Email { get; set; }
        }

        public class ComplexSource
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime BirthDate { get; set; }
            public int Score { get; set; }
            public bool IsActive { get; set; }
            public List<ContactInfo> Contacts { get; set; }
            public Address HomeAddress { get; set; }
        }

        public class ContactInfo
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string ZipCode { get; set; }
            public string Country { get; set; }
        }

        public class CustomerDto
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public DateTime BirthDate { get; set; }
            public int Score { get; set; }
            public bool IsActive { get; set; }
            public List<ContactInfo> Contacts { get; set; }
            public Address HomeAddress { get; set; }
        }
    }

    [MemoryDiagnoser]
    [RankColumn]
    public class MapperBenchmarks
    {
        private BenchmarkModels.SimpleSource _simpleSource;
        private BenchmarkModels.SimpleTarget _simpleExistingTarget;
        private BenchmarkModels.ComplexSource _complexSource;
        private BenchmarkModels.CustomerDto _complexExistingTarget;
        private List<BenchmarkModels.ComplexSource> _bulkSources;
        private AutoMapper.IMapper _autoMapper;

        [GlobalSetup]
        public void Setup()
        {
            // Setup simple source
            _simpleSource = new BenchmarkModels.SimpleSource
            {
                Name = "John Doe",
                Age = 30,
                Email = "john@example.com"
            };

            _simpleExistingTarget = new BenchmarkModels.SimpleTarget();

            // Setup complex source  
            _complexSource = new BenchmarkModels.ComplexSource
            {
                FirstName = "John",
                LastName = "Doe", 
                Email = "john.doe@example.com",
                BirthDate = new DateTime(1990, 5, 15),
                Score = 95,
                IsActive = true,
                Contacts = new List<BenchmarkModels.ContactInfo>
                {
                    new BenchmarkModels.ContactInfo { Type = "Phone", Value = "123-456-7890" },
                    new BenchmarkModels.ContactInfo { Type = "Fax", Value = "123-456-7891" }
                },
                HomeAddress = new BenchmarkModels.Address
                {
                    Street = "123 Main St",
                    City = "New York", 
                    ZipCode = "10001",
                    Country = "USA"
                }
            };

            _complexExistingTarget = new BenchmarkModels.CustomerDto();

            // Setup bulk sources
            _bulkSources = new List<BenchmarkModels.ComplexSource>();
            for (int i = 0; i < 1000; i++)
            {
                _bulkSources.Add(new BenchmarkModels.ComplexSource
                {
                    FirstName = $"FirstName{i}",
                    LastName = $"LastName{i}",
                    Email = $"user{i}@example.com",
                    BirthDate = new DateTime(1990 + (i % 30), (i % 12) + 1, (i % 28) + 1),
                    Score = i % 100,
                    IsActive = i % 2 == 0,
                    Contacts = new List<BenchmarkModels.ContactInfo>
                    {
                        new BenchmarkModels.ContactInfo { Type = "Email", Value = $"contact{i}@example.com" }
                    },
                    HomeAddress = new BenchmarkModels.Address
                    {
                        Street = $"{i} Test St",
                        City = "TestCity",
                        ZipCode = $"{10000 + i}",
                        Country = "TestCountry"
                    }
                });
            }

            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BenchmarkModels.SimpleSource, BenchmarkModels.SimpleTarget>();
                cfg.CreateMap<BenchmarkModels.ComplexSource, BenchmarkModels.CustomerDto>()
                   .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
                cfg.CreateMap<BenchmarkModels.ContactInfo, BenchmarkModels.ContactInfo>();
                cfg.CreateMap<BenchmarkModels.Address, BenchmarkModels.Address>();
            });
            _autoMapper = config.CreateMapper();

            // Setup custom mappings for FastMapper
            MapperExtensions.AddCustomMapping<BenchmarkModels.ComplexSource, BenchmarkModels.CustomerDto>(
                "FirstName", "FullName", source => 
                {
                    var complexSource = (BenchmarkModels.ComplexSource)source;
                    return $"{complexSource.FirstName} {complexSource.LastName}";
                });
        }

        // Manual mapping baseline
        [Benchmark(Baseline = true)]
        public BenchmarkModels.SimpleTarget ManualMap_Simple()
        {
            return new BenchmarkModels.SimpleTarget
            {
                Name = _simpleSource.Name,
                Age = _simpleSource.Age,
                Email = _simpleSource.Email
            };
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto ManualMap_Complex()
        {
            return new BenchmarkModels.CustomerDto
            {
                FirstName = _complexSource.FirstName,
                LastName = _complexSource.LastName,
                FullName = $"{_complexSource.FirstName} {_complexSource.LastName}",
                Email = _complexSource.Email,
                BirthDate = _complexSource.BirthDate,
                Score = _complexSource.Score,
                IsActive = _complexSource.IsActive,
                Contacts = _complexSource.Contacts?.Select(c => new BenchmarkModels.ContactInfo { Type = c.Type, Value = c.Value }).ToList(),
                HomeAddress = _complexSource.HomeAddress == null ? null : new BenchmarkModels.Address
                {
                    Street = _complexSource.HomeAddress.Street,
                    City = _complexSource.HomeAddress.City,
                    ZipCode = _complexSource.HomeAddress.ZipCode,
                    Country = _complexSource.HomeAddress.Country
                }
            };
        }

        [Benchmark]
        public List<BenchmarkModels.CustomerDto> ManualMap_BulkMapping()
        {
            var result = new List<BenchmarkModels.CustomerDto>(_bulkSources.Count);
            foreach (var source in _bulkSources)
            {
                result.Add(new BenchmarkModels.CustomerDto
                {
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    FullName = $"{source.FirstName} {source.LastName}",
                    Email = source.Email,
                    BirthDate = source.BirthDate,
                    Score = source.Score,
                    IsActive = source.IsActive,
                    Contacts = source.Contacts?.Select(c => new BenchmarkModels.ContactInfo { Type = c.Type, Value = c.Value }).ToList(),
                    HomeAddress = source.HomeAddress == null ? null : new BenchmarkModels.Address
                    {
                        Street = source.HomeAddress.Street,
                        City = source.HomeAddress.City,
                        ZipCode = source.HomeAddress.ZipCode,
                        Country = source.HomeAddress.Country
                    }
                });
            }
            return result;
        }

        // AutoMapper benchmarks
        [Benchmark]
        public BenchmarkModels.SimpleTarget AutoMapper_Simple()
        {
            return _autoMapper.Map<BenchmarkModels.SimpleTarget>(_simpleSource);
        }

        [Benchmark]
        public BenchmarkModels.SimpleTarget AutoMapper_Simple_ExistingObject()
        {
            return _autoMapper.Map(_simpleSource, _simpleExistingTarget);
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto AutoMapper_Complex()
        {
            return _autoMapper.Map<BenchmarkModels.CustomerDto>(_complexSource);
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto AutoMapper_Complex_ExistingObject()
        {
            return _autoMapper.Map(_complexSource, _complexExistingTarget);
        }

        [Benchmark]
        public List<BenchmarkModels.CustomerDto> AutoMapper_BulkMapping()
        {
            return _autoMapper.Map<List<BenchmarkModels.CustomerDto>>(_bulkSources);
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto AutoMapper_WithCustomMapping()
        {
            return _autoMapper.Map<BenchmarkModels.CustomerDto>(_complexSource);
        }

        // FastMapper benchmarks
        [Benchmark]
        public BenchmarkModels.SimpleTarget FastMapper_Simple()
        {
            return _simpleSource.FastMapTo<BenchmarkModels.SimpleTarget>();
        }

        [Benchmark]
        public BenchmarkModels.SimpleTarget FastMapper_Simple_ExistingObject()
        {
            _simpleSource.FastMapTo(_simpleExistingTarget);
            return _simpleExistingTarget;
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto FastMapper_Complex()
        {
            return _complexSource.FastMapTo<BenchmarkModels.CustomerDto>();
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto FastMapper_Complex_ExistingObject()
        {
            _complexSource.FastMapTo(_complexExistingTarget);
            return _complexExistingTarget;
        }

        [Benchmark]
        public List<BenchmarkModels.CustomerDto> FastMapper_BulkMapping()
        {
            return _bulkSources.Cast<object>().FastMapToList<BenchmarkModels.CustomerDto>();
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto FastMapper_WithCustomMapping()
        {
            return _complexSource.FastMapTo<BenchmarkModels.CustomerDto>();
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto FastMapper_WithCombine()
        {
            return _complexSource.FastMapTo<BenchmarkModels.CustomerDto>();
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto FastMapper_WithMultipleCombines()
        {
            return _complexSource.FastMapTo<BenchmarkModels.CustomerDto>();
        }

        [Benchmark]
        public BenchmarkModels.CustomerDto FastMapper_TypeConverter()
        {
            return _complexSource.FastMapTo<BenchmarkModels.CustomerDto>();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<MapperBenchmarks>();
        }
    }
} 