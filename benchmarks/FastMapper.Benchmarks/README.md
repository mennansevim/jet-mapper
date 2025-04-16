# FastMapper Benchmarks

This project benchmarks FastMapper against manual mapping and measures performance of different FastMapper features.

## Benchmark Scenarios

1. **Simple Mapping**: Basic mapping between two simple classes with primitive properties
   - Manual mapping vs FastMapper
   - Creating new objects vs mapping to existing objects

2. **Complex Mapping**: Mapping of complex object graphs with nested objects and collections
   - Manual mapping vs FastMapper
   - Creating new objects vs mapping to existing objects

3. **Bulk Mapping**: Performance when mapping a large collection of objects (1000 items)
   - Manual mapping vs FastMapper

4. **Feature Performance**:
   - FastMapper with custom mappings
   - FastMapper with type converters
   - FastMapper with property and enum caching
   - FastMapper with preserved target properties

## Optimization Features

Recent performance optimizations in FastMapper include:

- **Property Caching**: Caching property information to reduce reflection overhead
- **String-to-Enum Caching**: Caching enum conversions to avoid expensive parsing
- **Improved Property Copy Logic**: Better handling of existing target object properties
- **Optimized Deep Copy**: More efficient deep object mapping

## Running the Benchmarks

To run the benchmarks:

```bash
cd benchmarks/FastMapper.Benchmarks
dotnet run -c Release
```

## Latest Results

Below are the latest benchmark results (Apple M2, macOS 14.5):

| Method                          | Mean      | Error    | StdDev   | Median    | Allocated |
|---------------------------------|----------:|---------:|---------:|----------:|----------:|
| ManualMap_Simple                | 10.21 ns  | 0.30 ns  | 0.85 ns  | 9.94 ns   | 48 B      |
| ManualMap_Complex               | 261.15 ns | 5.19 ns  | 5.09 ns  | 258.70 ns | 976 B     |
| FastMapper_Simple               | 1.44 μs   | 0.01 μs  | 0.01 μs  | 1.44 μs   | 1520 B    |
| FastMapper_Simple_ExistingObject| 1.53 μs   | 0.04 μs  | 0.12 μs  | 1.46 μs   | 1520 B    |
| ManualMap_BulkMapping           | 15.23 μs  | 0.24 μs  | 0.28 μs  | 15.13 μs  | 64600 B   |
| FastMapper_WithMultipleCombines | 81.53 μs  | 0.23 μs  | 0.20 μs  | 81.48 μs  | 19055 B   |
| FastMapper_Complex              | 81.85 μs  | 0.44 μs  | 0.37 μs  | 81.77 μs  | 18871 B   |
| FastMapper_WithCombine          | 83.12 μs  | 1.58 μs  | 2.50 μs  | 81.93 μs  | 18911 B   |
| FastMapper_TypeConverter        | 83.30 μs  | 1.46 μs  | 3.05 μs  | 81.89 μs  | 19051 B   |
| FastMapper_WithCustomMapping    | 83.86 μs  | 1.66 μs  | 3.88 μs  | 81.95 μs  | 18871 B   |
| FastMapper_Complex_ExistingObject | 84.76 μs | 1.67 μs | 4.19 μs  | 82.89 μs  | 19704 B   |
| FastMapper_BulkMapping          | 1.51 ms   | 0.03 ms  | 0.04 ms  | 1.49 ms   | 1536602 B |

## Performance Analysis

The benchmarks demonstrate:

1. **Manual vs FastMapper**: While direct manual mapping is faster for simple cases (as expected), FastMapper provides:
   - Automation and consistency across mapping operations
   - Type safety and overflow protection
   - Flexible custom mappings

2. **Caching Benefits**: Property and enum caching deliver consistent performance across complex scenarios

3. **Existing Object Handling**: Mapping to existing objects has minimal performance overhead while preserving non-mapped properties

4. **Memory Usage**: FastMapper uses more memory than manual mapping, which is the trade-off for its flexibility and features 