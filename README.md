# FastMapper

FastMapper is a lightweight C# library providing easy and fast data transfer between classes. Developed as an alternative to AutoMapper, it offers a simple and direct approach without requiring complex profile structures.

## Overview

FastMapper provides a simple, fast, and flexible solution for object mapping between C# classes. It can be easily used with a fluent API without requiring complex profile structures.

## Key Features

- 🚀 Simple and direct mapping without profile structures
- ⚡ High performance and low memory usage
- 🧩 Smart conversions between different types
- 🔄 Automatic mapping of complex objects and collections
- 🛠️ Custom mapping definitions and type converters
- 🛡️ Protection against overflow and conversion errors
- 🔒 Proper handling of existing target object properties
- 📊 Optimized performance with property and enum caching

## Installation

You can add the package to your project via NuGet:

```
Install-Package FastMapper
```

or

```
dotnet add package FastMapper
```

## Quick Start

### Basic Mapping

```csharp
using FastMapper;

// Source class
var customer = new Customer 
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    BirthDate = new DateTime(1980, 1, 1)
};

// Convert to destination class
var customerDto = customer.FastMapTo<CustomerDto>();

// Map to an existing object
var existingDto = new CustomerDto();
customer.FastMapTo(existingDto);
```

### Custom Mapping Definitions

```csharp
// Define custom mappings
MapperExtensions.AddCustomMapping<Customer, CustomerDto, string>(
    "FullName", 
    customer => $"{customer.FirstName} {customer.LastName}"
);

MapperExtensions.AddCustomMapping<Customer, CustomerDto, string>(
    "FormattedBirthDate", 
    customer => customer.BirthDate.ToString("dd.MM.yyyy")
);

// Use the defined custom mappings
var customerDto = customer.FastMapTo<CustomerDto>();
// customerDto.FullName = "John Doe"
// customerDto.FormattedBirthDate = "01.01.1980"

// Remove a specific custom mapping
MapperExtensions.RemoveCustomMapping<Customer, CustomerDto>("FullName");

// Clear all custom mappings
MapperExtensions.ClearAllCustomMappings();
```

### Custom Type Converters

```csharp
using System.Globalization;

// Define custom type converters
MapperExtensions.AddTypeConverter<decimal, string>(
    amount => $"{amount:C}"
);

MapperExtensions.AddTypeConverter<DateTime, string>(
    date => date.ToString("yyyy-MM-dd")
);

// Perform mapping with type converters
var customerDto = customer.FastMapTo<CustomerDto>();

// Remove a specific type converter
MapperExtensions.RemoveTypeConverter<decimal, string>();

// Clear all type converters
MapperExtensions.ClearAllTypeConverters();
```

### Complex Object and Collection Mapping

```csharp
// Complex model
var customer = new Customer
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Address = new Address
    {
        Street = "123 Main St",
        City = "New York",
        ZipCode = "10001"
    },
    Orders = new List<Order>
    {
        new Order
        {
            OrderId = 1001,
            Total = 99.95m,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 101, Quantity = 2, Price = 49.95m }
            }
        }
    }
};

// Map all nested objects and collections in a single step
var customerDto = customer.FastMapTo<CustomerDto>();

// customerDto.Address.Street = "123 Main St"
// customerDto.Orders[0].Items[0].ProductId = 101
```

### Existing Object Mapping

When mapping to an existing object, FastMapper preserves properties of the target object that don't have corresponding source properties:

```csharp
// Target with existing values
var existingPerson = new PersonDto
{
    Id = 0,
    FirstName = "",
    LastName = "", 
    FullName = "Should Remain"
};

// Source object
var person = new Person
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe"
};

// Map to the existing object
person.FastMapTo(existingPerson);

// Result:
// existingPerson.Id = 1
// existingPerson.FirstName = "John"
// existingPerson.LastName = "Doe"
// existingPerson.FullName = "Should Remain" (preserved because source doesn't have this property)
```

## Performance Optimizations

FastMapper includes several performance optimizations:

- **Property Caching**: Uses `ConcurrentDictionary` to cache property information, reducing the cost of reflection operations
- **Enum Caching**: Caches enum conversion results to improve performance when converting strings to enums repeatedly
- **Efficient Type Conversions**: Optimized code paths for common conversion scenarios
- **Smart Deep Copy**: Avoids unnecessary operations during complex object mapping

## Supported Type Conversions

FastMapper automatically supports the following type conversions:

- Conversions between numeric types (int→double, decimal→int, etc.)
- String↔numeric type conversions
- String↔enum conversions (with caching for better performance)
- DateTime↔string conversions
- TimeSpan→long conversion (milliseconds)
- Guid→string conversion
- Boolean↔string conversions

## Benchmark Results

Performance comparison between manual mapping and FastMapper:

| Method                          | Mean      | Allocated |
|---------------------------------|----------:|----------:|
| ManualMap_Simple                | 10.21 ns  | 48 B      |
| ManualMap_Complex               | 261.15 ns | 976 B     |
| FastMapper_Simple               | 1.44 μs   | 1520 B    |
| FastMapper_Complex              | 81.85 μs  | 18871 B   |
| FastMapper_WithCustomMapping    | 83.86 μs  | 18871 B   |
| FastMapper_TypeConverter        | 83.30 μs  | 19051 B   |
| ManualMap_BulkMapping           | 15.23 μs  | 64600 B   |
| FastMapper_BulkMapping          | 1.51 ms   | 1536602 B |

## Why FastMapper?

- **Simpler than AutoMapper**: Offers direct and understandable usage without complex profile structures
- **Safe Conversions**: Provides protection against overflow and type conversion errors
- **Collection Support**: Automatically maps lists and other collections
- **Flexible Customization**: Offers custom mapping definitions and type converters
- **Lower Memory Usage**: A lightweight structure that only consumes the resources it needs
- **Deep Object Support**: Automatically maps nested objects
- **Easy Learning Curve**: Can be quickly integrated with minimal API
- **Performant**: Optimized with caching mechanisms and smart property handling

## License

MIT