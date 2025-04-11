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
   - FastMapper with WithCombine feature
   - FastMapper with multiple WithCombine operations
   - FastMapper with type converters

## Running the Benchmarks

To run the benchmarks:

```bash
cd benchmarks/FastMapper.Benchmarks
dotnet run -c Release
```

## Sample Results

Below are sample benchmark results (will vary based on hardware): 