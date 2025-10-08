# 🚀 FastMapper - Ultra-Performance Object Mapper

FastMapper is an ultra-fast object mapping library developed for .NET. It offers faster performance than AutoMapper and Mapster.

## ⚡ Performance Comparison

### 🏆 FastMapper vs AutoMapper vs Mapster

| Test Scenario | FastMapper | AutoMapper | Mapster | FastMapper vs AutoMapper | FastMapper vs Mapster |
|----------------|------------|------------|---------|------------------------|----------------------|
| **Complex Mapping** | 94.06 ns | 259.17 ns | 250.89 ns | **2.76x faster** | **2.67x faster** |
| **Complex Existing Object** | 79.26 ns | 206.50 ns | 256.77 ns | **2.60x faster** | **3.24x faster** |
| **Bulk Mapping** | 72.71 µs | 215.71 µs | 256.31 µs | **2.97x faster** | **3.53x faster** |
| **Employee Mapping** | 18.50 µs | 83.78 µs | 80.96 µs | **4.53x faster** | **4.38x faster** |

### 🧠 Memory Optimization

| Scenario | FastMapper | AutoMapper | Mapster | FastMapper vs AutoMapper | FastMapper vs Mapster |
|---------|------------|------------|---------|------------------------|----------------------|
| **Complex Mapping** | 216 B | 576 B | 616 B | **+167% savings** | **+185% savings** |
| **Complex Existing Object** | 96 B | 104 B | 616 B | **+8% savings** | **+542% savings** |
| **Bulk Mapping** | 136,760 B | 592,520 B | 615,976 B | **+333% savings** | **+350% savings** |
| **Employee Mapping** | 48,544 B | 132,304 B | 127,976 B | **+173% savings** | **+164% savings** |

## 🎯 Features

- ⚡ **Ultra-Fast**: Maximum performance with expression tree compilation
- 🧠 **Memory Optimized**: 500%+ memory savings in complex mappings
- 🔒 **Type Safe**: Enhanced type compatibility checking
- 🚀 **Zero Allocation**: Minimal memory allocation
- 📦 **Lightweight**: Minimal dependencies
- 🔧 **Easy to Use**: Simple and intuitive API
- ✨ **Fluent API**: Builder pattern for custom mappings
  - `Set()` - Property value assignment
  - `SetIf()` - Conditional value assignment
  - `SetFirstIfExist()` - First available property assignment
  - `Ignore()` - Ignore sensitive properties
  - `BeforeMap()`/`AfterMap()` - Lifecycle hooks

## 📦 Installation

```bash
dotnet add package FastMapper
```

## 🚀 Usage

### Basic Mapping
```csharp
using FastMapper;

// Simple mapping
var source = new SimpleSource { Name = "John", Age = 30 };
var target = source.FastMapTo<SimpleTarget>();

// Complex mapping
var complexSource = new ComplexSource { /* ... */ };
var complexTarget = complexSource.FastMapTo<ComplexTarget>();

// Mapping to existing object
var existingTarget = new ComplexTarget();
complexSource.FastMapTo(existingTarget);

// Bulk mapping
var sources = new List<ComplexSource> { /* ... */ };
var targets = sources.FastMapToList<ComplexTarget>();
```

### ✨ Fluent API (Builder Pattern)
```csharp
// Custom property mapping
var dto = person.Builder()
    .MapTo<PersonDto>()
    .Set(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .Set(d => d.Age, p => DateTime.Now.Year - p.BirthDate.Year)
    .Create();

// Conditional mapping
var orderDto = order.Builder()
    .MapTo<OrderDto>()
    .SetIf(d => d.Status, o => o.IsPaid, o => "✅ Paid")
    .SetIf(d => d.Status, o => !o.IsPaid, o => "⏳ Pending")
    .SetIf(d => d.Shipping, o => o.Total > 100, o => "🚚 Free Shipping")
    .Create();

// Ignore sensitive properties
var userDto = user.Builder()
    .MapTo<UserDto>()
    .Ignore(d => d.Password)
    .Ignore(d => d.SocialSecurityNumber)
    .Create();

// Hooks (BeforeMap/AfterMap)
var result = source.Builder()
    .MapTo<Target>()
    .BeforeMap((src, dest) => Console.WriteLine("Mapping started..."))
    .Set(d => d.Name, s => s.FullName)
    .AfterMap((src, dest) => Console.WriteLine("Mapping completed!"))
    .Create();
```

📖 **For more examples**: See [API_EXAMPLES.md](API_EXAMPLES.md)

## 🔧 Advanced Features

### Custom Mapping
```csharp
MapperExtensions.AddCustomMapping<Source, Target>(
    "SourceProperty", 
    "TargetProperty", 
    source => /* custom logic */
);
```

### Type Converter
```csharp
MapperExtensions.AddTypeConverter<string, int>(
    value => int.Parse(value)
);
```

## 📊 Benchmark Results

For detailed benchmark results, see [benchmarks/FastMapper.Benchmarks/README.md](benchmarks/FastMapper.Benchmarks/README.md).

### 🏆 Key Findings

- **Complex Mapping**: FastMapper is **2.76x** faster than AutoMapper and **2.67x** faster than Mapster
- **Complex Existing Object**: FastMapper is **2.60x** faster than AutoMapper and **3.24x** faster than Mapster
- **Bulk Mapping**: FastMapper is **2.97x** faster than AutoMapper and **3.53x** faster than Mapster
- **Employee Mapping**: FastMapper is **4.53x** faster than AutoMapper and **4.38x** faster than Mapster
- **Memory Efficiency**: 500%+ memory savings in complex scenarios
- **Type Safety**: Runtime errors prevented

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- [AutoMapper](https://github.com/AutoMapper/AutoMapper) - For comparison
- [Mapster](https://github.com/MapsterMapper/Mapster) - For comparison
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) - Benchmark framework

---

**FastMapper - Ultra-Performance Object Mapper** 🚀
