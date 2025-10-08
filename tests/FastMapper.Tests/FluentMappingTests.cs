using Xunit;

namespace FastMapper.Tests
{
    public class FluentMappingTests
    {
        [Fact]
        public void MapTo_ShouldCreateCleanMappingAPI_Successfully()
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

            // Act - Yeni temiz API
            var result = source.Map()
                .MapTo<PersonDto>()
                .Map(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
                .Map(dto => dto.Status, p => p.IsActive ? "Active" : "Inactive")
                .Map(dto => dto.Age, p => DateTime.Now.Year - p.BirthDate.Year)
                .To();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("Active", result.Status);
            Assert.Equal(DateTime.Now.Year - 1990, result.Age);
            Assert.Equal(source.Id, result.Id);
        }

        [Fact]
        public void MapTo_ShouldHandleIgnoreProperties_Successfully()
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
                .MapTo<PersonDto>()
                .Ignore(dto => dto.Status)
                .Ignore(dto => dto.FullName)
                .To();

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Status);
            Assert.Null(result.FullName);
            Assert.Equal(source.Id, result.Id);
            Assert.Equal(source.FirstName, result.FirstName);
        }

        [Fact]
        public void MapTo_ShouldHandleConditionalMapping_Successfully()
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
                .MapTo<PersonDto>()
                .MapIf(dto => dto.Status, 
                    p => p.IsActive, 
                    p => "Active")
                .MapIf(dto => dto.FullName, 
                    p => !string.IsNullOrEmpty(p.FirstName), 
                    p => $"{p.FirstName} {p.LastName}")
                .To();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Active", result.Status);
            Assert.Equal("John Doe", result.FullName);
        }

        [Fact]
        public void MapTo_ShouldHandleComplexMapping_Successfully()
        {
            // Arrange
            var source = new ComplexSource
            {
                Id = 123,
                Name = "Test Entity",
                SubEntity = new SubSource { Value = "Sub Value" },
                CreatedDate = new DateTime(2023, 1, 15, 10, 30, 0)
            };

            // Act - Kullanıcının istediği temiz syntax
            var result = source.Map()
                .MapTo<ComplexTarget>()
                .Map(t => t.Identifier, s => s.Id)
                .Map(t => t.Name, s => s.Name)
                .Map(t => t.TargetSubEntity, s => s.SubEntity)
                .Map(t => t.CreatedDateString, s => s.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"))
                .To();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.Identifier);
            Assert.Equal("Test Entity", result.Name);
            Assert.NotNull(result.TargetSubEntity);
            Assert.Equal("Sub Value", result.TargetSubEntity.Value);
            Assert.Equal("2023-01-15 10:30:00", result.CreatedDateString);
        }

        [Fact]
        public void MapTo_ShouldHandleBeforeAndAfterActions_Successfully()
        {
            // Arrange
            var source = new Person { Id = 1, FirstName = "John" };
            var beforeMapCalled = false;
            var afterMapCalled = false;

            // Act
            var result = source.Map()
                .MapTo<PersonDto>()
                .BeforeMap((s, t) => beforeMapCalled = true)
                .AfterMap((s, t) => afterMapCalled = true)
                .To();

            // Assert
            Assert.True(beforeMapCalled);
            Assert.True(afterMapCalled);
            Assert.NotNull(result);
        }

        [Fact]
        public void MapTo_ShouldHandleExistingTarget_Successfully()
        {
            // Arrange
            var source = new Person { Id = 1, FirstName = "John" };
            var existingTarget = new PersonDto { Status = "Existing" };

            // Act
            var result = source.Map()
                .MapTo<PersonDto>()
                .Map(dto => dto.FullName, p => p.FirstName)
                .To(existingTarget);

            // Assert
            Assert.Same(existingTarget, result);
            Assert.Equal("John", result.FullName);
            Assert.Equal("Existing", result.Status); // Mevcut değer korunmalı
        }
    }

    // Test modelleri
    public class ComplexSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SubSource SubEntity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SubSource
    {
        public string Value { get; set; }
    }

    public class ComplexTarget
    {
        public int Identifier { get; set; }
        public string Name { get; set; }
        public SubTarget TargetSubEntity { get; set; }
        public string CreatedDateString { get; set; }
    }

    public class SubTarget
    {
        public string Value { get; set; }
    }
}


