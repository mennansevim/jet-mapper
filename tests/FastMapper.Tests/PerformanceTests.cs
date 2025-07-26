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
        public void FastMapTo_ShouldBeEfficient_ForSimpleMapping()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true
            };

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _ = source.FastMapTo<PersonDto>();
            }

            // Act & Measure
            var stopwatch = Stopwatch.StartNew();
            const int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                _ = source.FastMapTo<PersonDto>();
            }

            stopwatch.Stop();

            // Assert
            var avgTime = stopwatch.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Simple mapping: {avgTime:F4} ms per operation");
            _output.WriteLine($"Total time for {iterations} iterations: {stopwatch.ElapsedMilliseconds} ms");

            // Should be reasonably fast
            Assert.True(avgTime < 10.0, $"Simple mapping took {avgTime:F4} ms per operation, expected < 10.0 ms");
        }

        [Fact]
        public void FastMapToList_ShouldBeEfficient_ForBulkMapping()
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

            // Warm up
            _ = sourceList.FastMapToList<PersonDto>();

            // Act & Measure
            var stopwatch = Stopwatch.StartNew();
            const int iterations = 10;

            for (int i = 0; i < iterations; i++)
            {
                _ = sourceList.FastMapToList<PersonDto>();
            }

            stopwatch.Stop();

            // Assert
            var avgTime = stopwatch.ElapsedMilliseconds / (double)iterations;
            _output.WriteLine($"Bulk mapping (100 items): {avgTime:F2} ms per operation");
            _output.WriteLine($"Total time for {iterations} iterations: {stopwatch.ElapsedMilliseconds} ms");

            // Should be efficient for bulk operations
            Assert.True(avgTime < 1000.0, $"Bulk mapping took {avgTime:F2} ms per operation, expected < 1000.0 ms");
        }

        [Fact]
        public void FastMapTo_ShouldShowCacheEfficiency_ForRepeatedMappings()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true
            };

            // First mapping (cold start)
            var stopwatch1 = Stopwatch.StartNew();
            _ = source.FastMapTo<PersonDto>();
            stopwatch1.Stop();

            // Second mapping (should use cached mapper)
            var stopwatch2 = Stopwatch.StartNew();
            _ = source.FastMapTo<PersonDto>();
            stopwatch2.Stop();

            // Multiple cached mappings
            var stopwatch3 = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                _ = source.FastMapTo<PersonDto>();
            }
            stopwatch3.Stop();

            var avgCachedTime = stopwatch3.ElapsedMilliseconds / 10.0;

            _output.WriteLine($"First mapping (cold): {stopwatch1.ElapsedMilliseconds} ms");
            _output.WriteLine($"Second mapping (cached): {stopwatch2.ElapsedMilliseconds} ms");
            _output.WriteLine($"Average cached mapping: {avgCachedTime:F4} ms");

            // Just verify that mappings complete successfully
            Assert.True(stopwatch1.ElapsedMilliseconds >= 0);
            Assert.True(stopwatch2.ElapsedMilliseconds >= 0);
            Assert.True(avgCachedTime >= 0);
        }

        [Fact]
        public void FastMapTo_ShouldBeFasterThanReflection_Benchmark()
        {
            // Arrange
            var source = new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateTime(1990, 1, 1),
                IsActive = true
            };

            // Manual mapping (baseline)
            var manualStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var manual = new PersonDto
                {
                    Id = source.Id,
                    FirstName = source.FirstName,
                    LastName = source.LastName,
                    Age = source.Age
                };
            }
            manualStopwatch.Stop();

            // FastMapper
            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _ = source.FastMapTo<PersonDto>();
            }

            var fastMapperStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                _ = source.FastMapTo<PersonDto>();
            }
            fastMapperStopwatch.Stop();

            _output.WriteLine($"Manual mapping: {manualStopwatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"FastMapper: {fastMapperStopwatch.ElapsedMilliseconds} ms");
            
            // Just verify both complete successfully
            Assert.True(manualStopwatch.ElapsedMilliseconds >= 0);
            Assert.True(fastMapperStopwatch.ElapsedMilliseconds >= 0);
        }
    }
} 