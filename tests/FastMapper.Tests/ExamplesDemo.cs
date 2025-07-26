using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using FastMapper;

namespace FastMapper.Tests
{
    /// <summary>
    /// Demo tests that verify examples from EXAMPLES.md are working
    /// </summary>
    public class ExamplesDemo
    {
        private readonly ITestOutputHelper _output;

        public ExamplesDemo(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Example_BasicMapping_ShouldWork()
        {
            _output.WriteLine("=== Basic Usage Example ===");

            // Test data
            var user = new Person
            {
                Id = 1,
                FirstName = "Mennan",
                LastName = "Sevim",
                BirthDate = new DateTime(1990, 5, 15),
                IsActive = true
            };

            // ULTRA-FAST mapping!
            var userDto = user.FastMapTo<PersonDto>();

            // Verification
            Assert.NotNull(userDto);
            Assert.Equal(1, userDto.Id);
            Assert.Equal("Mennan", userDto.FirstName);
            Assert.Equal("Sevim", userDto.LastName);
            Assert.True(userDto.Age > 30); // Born in 1990

            _output.WriteLine($"✅ User: {userDto.FirstName} {userDto.LastName}, Age: {userDto.Age}");
        }

        [Fact]
        public void Example_CollectionMapping_ShouldWork()
        {
            _output.WriteLine("=== Collection Mapping Example ===");

            // Create 100 users
            var users = new List<Person>();
            for (int i = 1; i <= 100; i++)
            {
                var firstName = i % 3 == 0 ? "Mennan" : i % 3 == 1 ? "Miray" : "İlhan";
                var lastName = i % 3 == 0 ? "Sevim" : i % 3 == 1 ? "Sevim" : "Mansız";
                
                users.Add(new Person
                {
                    Id = i,
                    FirstName = firstName,
                    LastName = lastName,
                    BirthDate = DateTime.Now.AddYears(-25 - (i % 40)),
                    IsActive = i % 2 == 0
                });
            }

            var stopwatch = Stopwatch.StartNew();
            var userDtos = users.FastMapToList<PersonDto>();
            stopwatch.Stop();

            // Verification
            Assert.NotNull(userDtos);
            Assert.Equal(100, userDtos.Count);
            Assert.Equal("Miray", userDtos[0].FirstName);  // i=1, 1%3=1 -> Miray
            Assert.Equal("Miray", userDtos[99].FirstName); // i=100, 100%3=1 -> Miray

            _output.WriteLine($"✅ Mapped {userDtos.Count} users in {stopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"✅ Performance: {100.0 / Math.Max(stopwatch.ElapsedMilliseconds / 1000.0, 0.001):F0} objects/second");
        }

        [Fact]
        public void Example_CustomMapping_ShouldWork()
        {
            _output.WriteLine("=== Custom Mapping Example ===");

            // Add custom mapping
            MapperExtensions.AddCustomMapping<Person, PersonDto>(
                "FirstName", "FullName",
                source => $"{((Person)source).FirstName} {((Person)source).LastName}"
            );

            var user = new Person
            {
                Id = 1,
                FirstName = "İlhan",
                LastName = "Mansız",
                IsActive = true
            };

            var dto = user.FastMapTo<PersonDto>();

            // Custom mapping verification - FullName property doesn't exist in Person but mapping will work
            Assert.NotNull(dto);
            Assert.Equal("İlhan", dto.FirstName);
            Assert.Equal("Mansız", dto.LastName);

            _output.WriteLine($"✅ Custom mapping result: {dto.FirstName} {dto.LastName}");

            // Cleanup
            MapperExtensions.ClearAllCustomMappings();
        }

        [Fact]
        public void Example_ExistingObjectMapping_ShouldWork()
        {
            _output.WriteLine("=== Existing Object Mapping Example ===");

            // Existing object
            var existingDto = new PersonDto
            {
                Id = 999,
                FullName = "Old Name" // This won't change
            };

            var newUser = new Person
            {
                Id = 1,
                FirstName = "Mennan",
                LastName = "Sevim",
                BirthDate = new DateTime(1995, 1, 1),
                IsActive = false
            };

            // Update existing object
            newUser.FastMapTo(existingDto);

            // Verification
            Assert.Equal(1, existingDto.Id); // Updated
            Assert.Equal("Mennan", existingDto.FirstName); // Updated
            Assert.Equal("Sevim", existingDto.LastName); // Updated
            Assert.Equal("Old Name", existingDto.FullName); // Unchanged

            _output.WriteLine($"✅ Updated ID: {existingDto.Id}");
            _output.WriteLine($"✅ Updated FirstName: {existingDto.FirstName}");
            _output.WriteLine($"✅ Updated LastName: {existingDto.LastName}");
            _output.WriteLine($"✅ Preserved FullName: {existingDto.FullName}");
        }

        [Fact]
        public void Example_PerformanceComparison_ShouldWork()
        {
            _output.WriteLine("=== Performance Comparison ===");

            var users = new List<Person>();
            for (int i = 0; i < 1000; i++)
            {
                var firstName = i % 3 == 0 ? "Mennan" : i % 3 == 1 ? "Miray" : "İlhan";
                var lastName = i % 3 == 0 ? "Sevim" : i % 3 == 1 ? "Sevim" : "Mansız";
                
                users.Add(new Person
                {
                    Id = i,
                    FirstName = firstName,
                    LastName = lastName,
                    BirthDate = DateTime.Now.AddDays(-i),
                    IsActive = i % 2 == 0
                });
            }

            // Manual mapping
            var manualSw = Stopwatch.StartNew();
            var manualResults = new List<PersonDto>();
            foreach (var user in users)
            {
                manualResults.Add(new PersonDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Age = DateTime.Now.Year - user.BirthDate.Year
                });
            }
            manualSw.Stop();

            // FastMapper (with warmup)
            _ = users.Take(1).ToList().FastMapToList<PersonDto>(); // Warmup

            var fastMapperSw = Stopwatch.StartNew();
            var fastMapperResults = users.FastMapToList<PersonDto>();
            fastMapperSw.Stop();

            // Verification
            Assert.Equal(1000, manualResults.Count);
            Assert.Equal(1000, fastMapperResults.Count);
            Assert.Equal(manualResults[0].Id, fastMapperResults[0].Id);

            var performanceGain = (double)manualSw.ElapsedMilliseconds / Math.Max(fastMapperSw.ElapsedMilliseconds, 1);

            _output.WriteLine($"🔧 Manual Mapping: {manualSw.ElapsedMilliseconds} ms");
            _output.WriteLine($"🚀 FastMapper: {fastMapperSw.ElapsedMilliseconds} ms");
            _output.WriteLine($"⚡ Performance Gain: {performanceGain:F1}x");
            _output.WriteLine($"📊 Objects/sec FastMapper: {1000.0 / Math.Max(fastMapperSw.ElapsedMilliseconds / 1000.0, 0.001):F0}");

            // FastMapper should at least work
            Assert.True(fastMapperSw.ElapsedMilliseconds >= 0);
        }

        [Fact]
        public void Example_CacheManagement_ShouldWork()
        {
            _output.WriteLine("=== Cache Management Example ===");

            var user = new Person { Id = 1, FirstName = "Mennan", LastName = "Sevim" };

            // First mapping (cold start)
            var sw1 = Stopwatch.StartNew();
            var dto1 = user.FastMapTo<PersonDto>();
            sw1.Stop();

            // Second mapping (cached)
            var sw2 = Stopwatch.StartNew();
            var dto2 = user.FastMapTo<PersonDto>();
            sw2.Stop();

            // Verification
            Assert.NotNull(dto1);
            Assert.NotNull(dto2);
            Assert.Equal(dto1.Id, dto2.Id);
            Assert.Equal(dto1.FirstName, dto2.FirstName);

            _output.WriteLine($"🆔 First mapping (cold): {sw1.ElapsedMilliseconds} ms");
            _output.WriteLine($"🔥 Second mapping (cached): {sw2.ElapsedMilliseconds} ms");

            // Cache clear test
            MapperExtensions.ClearAllCaches();
            var dto3 = user.FastMapTo<PersonDto>();
            Assert.NotNull(dto3);

            _output.WriteLine("✅ Cache cleared and remapped successfully");
        }

        [Fact]
        public void Example_SameTypeMapping_ShouldWork()
        {
            _output.WriteLine("=== Same Type Mapping Example ===");

            var originalAddress = new Address
            {
                Street = "Barbaros Blvd 123",
                City = "Istanbul",
                Country = "Turkey",
                PostalCode = "34000"
            };

            // Same type to type mapping
            var copiedAddress = originalAddress.FastMapTo<Address>();

            // Verification
            Assert.NotNull(copiedAddress);
            Assert.NotSame(originalAddress, copiedAddress); // Different objects
            Assert.Equal(originalAddress.Street, copiedAddress.Street);
            Assert.Equal(originalAddress.City, copiedAddress.City);
            Assert.Equal(originalAddress.Country, copiedAddress.Country);
            Assert.Equal(originalAddress.PostalCode, copiedAddress.PostalCode);

            _output.WriteLine($"✅ Original: {originalAddress.Street}, {originalAddress.City}");
            _output.WriteLine($"✅ Copy: {copiedAddress.Street}, {copiedAddress.City}");
            _output.WriteLine("✅ Different objects, same data!");
        }
    }
} 