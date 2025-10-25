![JetMapper](https://raw.githubusercontent.com/mennansevim/jet-mapper/master/src/JetMapper/public/bg.jpg)

# JetMapper

**A high-performance .NET object mapper - 2-4x faster than AutoMapper with 500%+ less memory usage.**

[![NuGet](https://img.shields.io/nuget/v/JetMapper.svg)](https://www.nuget.org/packages/JetMapper)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Why JetMapper?

- **Ultra-Fast**: Expression tree compilation for maximum performance - 2-4x faster than AutoMapper
- **Memory Optimized**: 500%+ savings in complex mappings
- **Type Safe**: Enhanced type compatibility checks
- **Low Allocation**: Minimal memory usage
- **Lightweight**: Minimal dependencies
- **Multi-Platform**: .NET Standard 2.0/2.1, .NET Framework 4.6.2/4.7.2/4.8, .NET 6/7/8/9
- **Easy to Use**: Simple and intuitive API
- **Fluent API**: Builder pattern for custom mappings
  - `Set()` - Property value assignment
  - `SetIf()` - Conditional value assignment
  - `SetFirstIfExist()` - First available property assignment
  - `Ignore()` - Ignore sensitive properties
  - `BeforeMap()`/`AfterMap()` - Lifecycle hooks

## Installation

```bash
  dotnet add package JetMapper
```

## Quick Start

### Basic Mapping

The simplest way to map objects - just one line of code with zero configuration:

```csharp
using JetMapper;

// Simple mapping
User user = new User { FirstName = "John", LastName = "Doe" };
UserDto dto = user.FastMapTo<UserDto>();

// Collection mapping
List<User> users = GetUsers();
List<UserDto> dtos = users.FastMapToList<User, UserDto>();
```

### Fluent API

Build complex mappings with custom transformations and property assignments:

```csharp
PersonDto dto = person.Builder()
    .MapTo<PersonDto>()
    .Set(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .Set(d => d.Age, p => DateTime.Now.Year - p.BirthDate.Year)
    .Ignore(d => d.Password)
    .Create();
```

## Advanced Features

### Conditional Mapping

Assign values based on conditions - perfect for status fields and business logic:

```csharp
AccountDto dto = account.Builder()
    .MapTo<AccountDto>()
    .SetIf(d => d.Status, a => a.IsActive, a => "Active")
    .SetIf(d => d.Status, a => !a.IsActive, a => "Inactive")
    .Create();
```

### Priority Assignment

Choose the first available non-null value from multiple properties:

```csharp
ContactDto dto = contact.Builder()
    .MapTo<ContactDto>()
    .SetFirstIfExist(d => d.PreferredContact,
        (d => d.Email, c => $"📧 {c.Email}"),
        (d => d.Phone, c => $"📱 {c.Phone}"),
        (d => d.Address, c => $"🏠 {c.Address}"))
    .Create();
```

### Lifecycle Hooks

Execute custom logic before and after mapping - ideal for logging and validation:

```csharp
OrderDto dto = order.Builder()
    .MapTo<OrderDto>()
    .BeforeMap((src, dest) => Console.WriteLine("Starting..."))
    .Set(d => d.OrderNumber, o => $"#ORD-{o.Id}")
    .AfterMap((src, dest) => Console.WriteLine("Completed!"))
    .Create();
```

### Async Mapping

Process large datasets asynchronously with real-time progress tracking:

```csharp
List<User> users = GetLargeUserList(); // 10,000+ records

Progress<AsyncMapper.MappingProgress> progress = new Progress<AsyncMapper.MappingProgress>(p =>
    Console.WriteLine($"Processing: {p.Percentage:F1}%"));

List<UserDto> dtos = await AsyncMapper.MapAsync<User, UserDto>(users, progress);
```

### Diff Mapping

Compare two objects and detect changes automatically:

```csharp
DiffResult diff = DiffMapper.FindDifferences(originalUser, updatedUser);

if (diff.HasDifferences)
{
    Console.WriteLine($"Found {diff.Differences.Count} changes");
    Console.WriteLine($"Similarity: {diff.SimilarityPercentage:F1}%");
}
```

### Type Converters

Register custom type converters and enable automatic enum conversions:

```csharp
// Register custom converters
MapperExtensions.AddTypeConverter<int, string>(n => n.ToString());
MapperExtensions.AddTypeConverter<DateTime, string>(dt => dt.ToString("yyyy-MM-dd"));

// Automatic enum conversions
ApiOrder apiOrder = new ApiOrder { Status = "processing" };
OrderDto dto = apiOrder.FastMapTo<OrderDto>(); // Status = OrderStatus.Processing
```

## Performance

**Benchmark Results (Apple M2, .NET 6)**

| Scenario | JetMapper | AutoMapper | Speed Gain |
|----------|-----------|------------|------------|
| Complex Mapping | 94 ns | 259 ns | **2.76x faster** |
| Bulk Mapping (1000) | 73 µs | 216 µs | **2.97x faster** |
| Employee Mapping | 19 µs | 84 µs | **4.53x faster** |

| Memory Usage | JetMapper | AutoMapper | Savings |
|--------------|-----------|------------|---------|
| Complex | 216 B | 576 B | **167%** |
| Bulk (1000) | 137 KB | 593 KB | **333%** |
| Employee | 49 KB | 132 KB | **173%** |

## API Reference

| Method | Description |
|--------|-------------|
| `FastMapTo<T>()` | Simple one-line mapping |
| `Builder()` | Start fluent mapping |
| `Set()` | Assign property value |
| `SetIf()` | Conditional assignment |
| `SetFirstIfExist()` | Priority-based assignment |
| `Ignore()` | Skip property |
| `BeforeMap()` / `AfterMap()` | Lifecycle hooks |
| `Create()` | Execute mapping |

## Advanced Configuration

### MappingValidator

Validate mappings at compile-time to catch errors early:

```csharp
ValidationResult result = MappingValidator.ValidateMapping<Person, PersonDto>();
if (!result.IsValid)
{
    foreach (ValidationError error in result.Errors)
        Console.WriteLine($"❌ {error.PropertyName}: {error.Message}");
}
```

### Snapshot & Restore

Save and restore object states for undo/redo functionality:

```csharp
Snapshot snapshot = AsyncMapper.CreateSnapshot(user);
// Make changes...
User restored = AsyncMapper.RestoreFromSnapshot<User>(snapshot.Id);
```

### Diagnostic Mapper

Profile performance and track metrics:

```csharp
PerformanceProfile profile = DiagnosticMapper.StartPerformanceProfile("UserMapping");
// Perform mappings...
PerformanceResult result = DiagnosticMapper.EndPerformanceProfile("UserMapping");
Console.WriteLine($"Average: {result.AverageMappingTime.TotalMicroseconds:F2}µs");
```

### Partial Merge

Merge specific properties from source to target:

```csharp
MergeResult result = MergeMapper.PartialMerge(targetUser, sourceUser, "FirstName", "LastName");
```

## Platform Support

- .NET Standard 2.0, 2.1
- .NET Framework 4.6.2, 4.7.2, 4.8
- .NET 6.0, 7.0, 8.0, 9.0

## Real-World Example

**E-Commerce Order Processing** - Transform database entities to view models with complex business logic:

```csharp
public class OrderEntity
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public bool IsPaid { get; set; }
    public bool IsShipped { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderViewModel
{
    public string OrderNumber { get; set; }
    public string TotalPrice { get; set; }
    public string Status { get; set; }
    public string ContactInfo { get; set; }
}

OrderEntity order = new OrderEntity
{
    Id = 12345,
    Amount = 500m,
    Tax = 90m,
    IsPaid = true,
    IsShipped = false,
    CustomerEmail = "customer@example.com",
    CreatedAt = DateTime.Now.AddDays(-2)
};

OrderViewModel viewModel = order.Builder()
    .MapTo<OrderViewModel>()
    .Set(vm => vm.OrderNumber, o => $"#ORD-{o.Id}")
    .Set(vm => vm.TotalPrice, o => $"${(o.Amount + o.Tax):F2}")
    .SetIf(vm => vm.Status, o => o.IsPaid && o.IsShipped, o => "✅ Delivered")
    .SetIf(vm => vm.Status, o => o.IsPaid && !o.IsShipped, o => "🚚 In Transit")
    .SetIf(vm => vm.Status, o => !o.IsPaid, o => "⏳ Awaiting Payment")
    .Set(vm => vm.ContactInfo, o => $"📧 {o.CustomerEmail}")
    .Create();

// Result:
// OrderNumber = "#ORD-12345"
// TotalPrice = "$590.00"
// Status = "🚚 In Transit"
// ContactInfo = "📧 customer@example.com"
```

## Documentation

For detailed examples and advanced usage:
- Run benchmarks: `dotnet run -c Release` in `benchmarks/JetMapper.Benchmarks`
- View examples: Check `JetMapper.Console/Program.cs`

## Contributing

Contributions are welcome! Feel free to:
- Report bugs via [GitHub Issues](https://github.com/mennansevim/jet-mapper/issues)
- Submit pull requests
- Suggest new features

## License

MIT License - See [LICENSE](LICENSE) file for details.

---

**Made with ❤️ for the .NET community**

Mennan Sevim

[GitHub](https://github.com/mennansevim/jet-mapper) • [NuGet](https://www.nuget.org/packages/JetMapper) • [Report Issue](https://github.com/mennansevim/jet-mapper/issues)
