# 🚀 FastMapper - Ultra-Performans Object Mapper

FastMapper, .NET için geliştirilmiş ultra-hızlı object mapping kütüphanesidir. AutoMapper ve Mapster'dan daha hızlı performans sunar.

## ⚡ Performans Karşılaştırması

### 🏆 FastMapper vs AutoMapper vs Mapster

| Test Senaryosu | FastMapper | AutoMapper | Mapster | FastMapper vs AutoMapper | FastMapper vs Mapster |
|----------------|------------|------------|---------|------------------------|----------------------|
| **Complex Mapping** | 94.06 ns | 259.17 ns | 250.89 ns | **2.76x daha hızlı** | **2.67x daha hızlı** |
| **Complex Existing Object** | 79.26 ns | 206.50 ns | 256.77 ns | **2.60x daha hızlı** | **3.24x daha hızlı** |
| **Bulk Mapping** | 72.71 µs | 215.71 µs | 256.31 µs | **2.97x daha hızlı** | **3.53x daha hızlı** |
| **Employee Mapping** | 18.50 µs | 83.78 µs | 80.96 µs | **4.53x daha hızlı** | **4.38x daha hızlı** |

### 🧠 Memory Optimizasyonu

| Senaryo | FastMapper | AutoMapper | Mapster | FastMapper vs AutoMapper | FastMapper vs Mapster |
|---------|------------|------------|---------|------------------------|----------------------|
| **Complex Mapping** | 216 B | 576 B | 616 B | **+167% tasarruf** | **+185% tasarruf** |
| **Complex Existing Object** | 96 B | 104 B | 616 B | **+8% tasarruf** | **+542% tasarruf** |
| **Bulk Mapping** | 136,760 B | 592,520 B | 615,976 B | **+333% tasarruf** | **+350% tasarruf** |
| **Employee Mapping** | 48,544 B | 132,304 B | 127,976 B | **+173% tasarruf** | **+164% tasarruf** |

## 🎯 Özellikler

- ⚡ **Ultra-Hızlı**: Expression tree compilation ile maksimum performans
- 🧠 **Memory Optimized**: Karmaşık mapping'lerde %500+ memory tasarrufu
- 🔒 **Type Safe**: Enhanced type compatibility kontrolü
- 🚀 **Zero Allocation**: Mümkün olduğunca az memory allocation
- 📦 **Lightweight**: Minimal dependencies
- 🔧 **Easy to Use**: Basit ve sezgisel API

## 📦 Kurulum

```bash
dotnet add package FastMapper
```

## 🚀 Kullanım

```csharp
using FastMapper;

// Basit mapping
var source = new SimpleSource { Name = "John", Age = 30 };
var target = source.FastMapTo<SimpleTarget>();

// Karmaşık mapping
var complexSource = new ComplexSource { /* ... */ };
var complexTarget = complexSource.FastMapTo<ComplexTarget>();

// Var olan nesneye mapping
var existingTarget = new ComplexTarget();
complexSource.FastMapTo(existingTarget);

// Toplu mapping
var sources = new List<ComplexSource> { /* ... */ };
var targets = sources.FastMapToList<ComplexTarget>();
```

## 🔧 Gelişmiş Özellikler

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

## 📊 Benchmark Sonuçları

Detaylı benchmark sonuçları için [benchmarks/FastMapper.Benchmarks/README.md](benchmarks/FastMapper.Benchmarks/README.md) dosyasını inceleyin.

### 🏆 Önemli Bulgular

- **Complex Mapping**: FastMapper, AutoMapper'dan **2.76x** ve Mapster'dan **2.67x** daha hızlı
- **Complex Existing Object**: FastMapper, AutoMapper'dan **2.60x** ve Mapster'dan **3.24x** daha hızlı
- **Bulk Mapping**: FastMapper, AutoMapper'dan **2.97x** ve Mapster'dan **3.53x** daha hızlı
- **Employee Mapping**: FastMapper, AutoMapper'dan **4.53x** ve Mapster'dan **4.38x** daha hızlı
- **Memory Efficiency**: Karmaşık senaryolarda **%500+** memory tasarrufu
- **Type Safety**: Runtime hataları önlendi

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 🙏 Teşekkürler

- [AutoMapper](https://github.com/AutoMapper/AutoMapper) - Karşılaştırma için
- [Mapster](https://github.com/MapsterMapper/Mapster) - Karşılaştırma için
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) - Benchmark framework

---

**FastMapper - Ultra-Performans Object Mapper** 🚀
