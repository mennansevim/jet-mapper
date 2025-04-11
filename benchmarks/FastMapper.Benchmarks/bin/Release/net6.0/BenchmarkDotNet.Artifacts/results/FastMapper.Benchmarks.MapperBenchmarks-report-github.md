``` ini

BenchmarkDotNet=v0.13.5, OS=macOS 14.5 (23F79) [Darwin 23.5.0]
Apple M2, 1 CPU, 8 logical and 8 physical cores
.NET SDK=9.0.200
  [Host]     : .NET 6.0.25 (6.0.2523.51912), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.25 (6.0.2523.51912), Arm64 RyuJIT AdvSIMD


```
|                            Method |             Mean |          Error |        StdDev |      Ratio |  RatioSD | Rank |      Gen0 | Allocated | Alloc Ratio |
|---------------------------------- |-----------------:|---------------:|--------------:|-----------:|---------:|-----:|----------:|----------:|------------:|
|                  ManualMap_Simple |         9.548 ns |      0.1598 ns |     0.1248 ns |       1.00 |     0.00 |    1 |    0.0229 |      48 B |        1.00 |
|                 ManualMap_Complex |       266.935 ns |      0.8787 ns |     0.8219 ns |      27.96 |     0.39 |    2 |    0.4663 |     976 B |       20.33 |
|  FastMapper_Simple_ExistingObject |     1,855.296 ns |      8.3525 ns |     7.4043 ns |     194.19 |     2.85 |    3 |    1.1387 |    2384 B |       49.67 |
|                 FastMapper_Simple |     1,906.526 ns |     37.2487 ns |    44.3419 ns |     200.26 |     5.97 |    4 |    1.1387 |    2384 B |       49.67 |
|             ManualMap_BulkMapping |    15,147.297 ns |    174.3546 ns |   154.5608 ns |   1,589.41 |    26.87 |    5 |   30.8533 |   64600 B |    1,345.83 |
|          FastMapper_TypeConverter |    85,553.668 ns |    448.8526 ns |   397.8961 ns |   8,973.26 |   121.28 |    6 |   12.4512 |   26192 B |      545.67 |
|      FastMapper_WithCustomMapping |    85,943.950 ns |  1,679.7400 ns | 1,402.6590 ns |   9,005.50 |   216.47 |    6 |   12.3291 |   25883 B |      539.23 |
|            FastMapper_WithCombine |    85,965.413 ns |    598.9249 ns |   530.9313 ns |   8,989.83 |   117.59 |    6 |   12.3291 |   25880 B |      539.17 |
|   FastMapper_WithMultipleCombines |    86,721.123 ns |  1,728.9682 ns | 1,349.8647 ns |   9,084.28 |   211.75 |    6 |   12.4512 |   26149 B |      544.77 |
| FastMapper_Complex_ExistingObject |    87,036.911 ns |    331.0894 ns |   276.4746 ns |   9,116.85 |   116.08 |    6 |   12.6953 |   26686 B |      555.96 |
|                FastMapper_Complex |    87,301.917 ns |  1,123.0367 ns |   937.7865 ns |   9,124.39 |    91.72 |    6 |   12.3291 |   25864 B |      538.83 |
|            FastMapper_BulkMapping | 1,938,309.936 ns | 10,357.4620 ns | 9,181.6203 ns | 203,200.58 | 2,730.93 |    7 | 1146.4844 | 2400625 B |   50,013.02 |
