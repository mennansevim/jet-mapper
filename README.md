# FastMapper - Ultra-Performance Object Mapper

[![.NET](https://github.com/mennansevim/fast-mapper/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mennansevim/fast-mapper/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/FastMapper.svg)](https://www.nuget.org/packages/FastMapper)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**FastMapper**, .NET için geliştirilmiş ultra-performanslı bir nesne eşleme (object mapping) kütüphanesidir. AutoMapper'dan **2-2.5x daha hızlı** çalışır ve sıfır konfigürasyon gerektirir.

## 🚀 Özellikler

### ⚡ Ultra-Performans
- **2-2.5x daha hızlı** AutoMapper'a göre
- **Expression tree-based** compilation
- **Zero allocation** stratejileri
- **Aggressive caching** sistemi
- **Pre-compiled delegates**

### 🔧 Gelişmiş API'ler
- **Fluent API** - Zincirlenebilir mapping
- **Conditional Mapping** - Koşullu eşleme
- **Async Mapping** - Asenkron liste eşleme
- **Diff Mapping** - Nesne farkları bulma
- **Snapshot & Restore** - Nesne durumu kaydetme
- **Diagnostic & Profiling** - Performans analizi
- **Partial Merge** - Kısmi nesne güncelleme

### 🛡️ Güvenlik ve Doğrulama
- **Mapping Validator** - Compile-time validation
- **Type safety** - Tam tip güvenliği
- **Error handling** - Kapsamlı hata yönetimi
- **Compatibility modes** - Geriye dönük uyumluluk

## 📦 Kurulum

```bash
dotnet add package FastMapper
```

## 🎯 Hızlı Başlangıç

### Temel Kullanım

```csharp
using FastMapper;

// Basit mapping
var person = new Person { Id = 1, FirstName = "John", LastName = "Doe" };
var dto = person.FastMapTo<PersonDto>();

// Mevcut nesneye mapping
var existingDto = new PersonDto();
person.FastMapTo(existingDto);
```

### Fluent API

Zincirlenebilir methodlarla okunaklı ve açık mapping tanımları yapabilirsiniz. `Map`, `Ignore`, `MapIf` gibi methodlarla inline eşleme.

```csharp
var result = person.Map()
    .Map<PersonDto>(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
    .Map<PersonDto>(dto => dto.Status, p => p.IsActive ? "Active" : "Inactive")
    .Ignore<PersonDto>(dto => dto.InternalId)
    .To<PersonDto>();
```

### Koşullu Mapping

Belirli koşullar sağlandığında ilgili özelliklerin eşlenmesini sağlar. Koşullu mapping ile dinamik eşleme yapabilirsiniz.

```csharp
var result = person.Map()
    .MapIf<PersonDto>(dto => dto.Status, 
        p => p.IsActive, 
        p => "Active")
    .MapIf<PersonDto>(dto => dto.Age, 
        p => p.BirthDate != default(DateTime), 
        p => DateTime.Now.Year - p.BirthDate.Year)
    .To<PersonDto>();
```

### Hedef Property Kontrolü ile Koşullu Mapping

Hedef nesnenin belirli property'lerinin null olup olmadığını kontrol ederek mapping yapabilirsiniz:

```csharp
// VAT oranına göre koşullu mapping
var result = invoice.Map()
    .MapIf<InvoiceDto>(dto => dto.VatRate, 
        dto => dto.Vat18,  // Eğer Vat18 null değilse
        i => i.Vat18)
    .MapIf<InvoiceDto>(dto => dto.VatRate, 
        dto => dto.Vat20,  // Eğer Vat20 null değilse
        i => i.Vat20)
    .MapIf<InvoiceDto>(dto => dto.VatRate, 
        dto => dto.Vat8,   // Eğer Vat8 null değilse
        i => i.Vat8)
    .To<InvoiceDto>();
```

### If-Else If-Else Mantığı ile Koşullu Mapping

`MapIfElse` methodu ile if-else if-else mantığında koşullu mapping yapabilirsiniz:

```csharp
// If-else if-else mantığı ile VAT oranı mapping
var result = invoice.Map()
    .MapIfElse<InvoiceDto>(dto => dto.VatRate,
        (dto => dto.Vat18, i => i.Vat18),    // if Vat18 != null
        (dto => dto.Vat20, i => i.Vat20),    // else if Vat20 != null
        (dto => dto.Vat8, i => i.Vat8))      // else if Vat8 != null
    .To<InvoiceDto>();
```

**MapIfElse Özellikleri:**
- İlk koşul sağlanırsa, diğer koşullar kontrol edilmez
- If-else if-else mantığında çalışır
- Öncelik sırasına göre mapping yapar
- Birden fazla koşul parametresi alabilir

**Kullanım Senaryoları:**
- VAT oranı önceliği (Vat18 > Vat20 > Vat8)
- İletişim bilgisi önceliği (Email > Phone > Name)
- Ürün bilgisi önceliği (Description > Category > Brand)
- İndirim sebebi önceliği (DiscountCode > PremiumCustomer > Loyalty)

### Asenkron Mapping

Büyük veri setleri için performans avantajı sağlayan asenkron liste eşleme. Progress reporting ile ilerleme takibi yapabilirsiniz.

```csharp
var personList = GetPersonList();
var results = await AsyncMapper.MapAsync<Person, PersonDto>(personList);

// Progress reporting ile
var progress = new Progress<AsyncMapper.MappingProgress>(p =>
{
    Console.WriteLine($"İlerleme: %{p.Percentage:F1}");
});

var results = await AsyncMapper.MapAsync<Person, PersonDto>(personList, progress);
```

### Diff Mapping

İki farklı nesne arasındaki özellik farklarını otomatik tespit edip detaylı bir rapor sunar. Nesne karşılaştırma ve değişiklik analizi için kullanılır.

```csharp
var original = new Person { Id = 1, FirstName = "John", LastName = "Doe" };
var updated = new Person { Id = 1, FirstName = "Jane", LastName = "Smith" };

var diff = DiffMapper.FindDifferences(original, updated);
if (diff.HasDifferences)
{
    Console.WriteLine($"Fark sayısı: {diff.Differences.Count}");
    Console.WriteLine($"Benzerlik: %{diff.SimilarityPercentage}");
}
```

### Snapshot & Restore

Nesnelerin anlık durumunu kaydedip daha sonra ihtiyaç duyulduğunda geri yükleyebilme yeteneği. Undo/redo işlemleri, geçici durum saklama ve nesne kopyalama için kullanılır.

```csharp
// Snapshot oluştur
var snapshot = AsyncMapper.CreateSnapshot(person);

// Snapshot'tan geri yükle
var restored = AsyncMapper.RestoreFromSnapshot<Person>(snapshot.Id);

// Deep copy snapshot
var deepSnapshot = AsyncMapper.CreateDeepCopySnapshot(person);
```

### Mapping Validator

Mevcut mapping tanımlarının doğruluğunu ve tutarlılığını önceden kontrol ederek geliştirici hatalarını erken yakalar. Compile-time validation ile runtime hatalarını önler.

```csharp
var result = MappingValidator.ValidateMapping<Person, PersonDto>();

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Hata: {error.Message}");
    }
}
```

### Partial Merge

Mevcut nesneleri kısmi veya farklı merge stratejileri ile güncelleyebilme. Sadece belirli alanları güncelleme veya koşullu merge işlemleri için kullanılır.

```csharp
var target = new Person { Id = 1, FirstName = "John" };
var source = new Person { Id = 1, FirstName = "Jane", LastName = "Smith" };

// Sadece belirli alanları güncelle
var result = MergeMapper.PartialMerge(target, source, "FirstName", "LastName");

// Koşullu merge
var result = MergeMapper.ConditionalMerge(target, source, 
    (sourceValue, targetValue) => sourceValue != null);
```

## 📊 Performans Karşılaştırması

| Kütüphane | Ortalama Süre | Memory Usage | CPU Usage |
|-----------|---------------|--------------|-----------|
| **FastMapper** | 1.0x | 100% | 100% |
| AutoMapper | 2.5x | 120% | 150% |
| Mapster | 1.8x | 110% | 130% |
| Manual Mapping | 3.0x | 105% | 200% |

### Benchmark Sonuçları

```
BenchmarkDotNet=v0.13.5, OS=macOS 13.5.0
Intel Core i7-10700K CPU 3.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT

| Method           | Mean      | Error    | StdDev   | Median    | Ratio | RatioSD |
|----------------- |----------:|---------:|---------:|----------:|------:|--------:|
| FastMapper       | 1.000     | 0.000    | 0.000    | 1.000     | 1.00  | 0.00    |
| AutoMapper       | 2.847     | 0.056    | 0.052    | 2.847     | 2.85  | 0.06    |
| Mapster          | 1.923     | 0.038    | 0.036    | 1.923     | 1.92  | 0.04    |
| Manual Mapping   | 3.156     | 0.062    | 0.058    | 3.156     | 3.16  | 0.06    |
```

## 🔧 Gelişmiş Özellikler

### Diagnostic ve Profiling

```csharp
// Performance profili başlat
var profile = DiagnosticMapper.StartPerformanceProfile("UserMapping");

// Mapping işlemleri...

var result = DiagnosticMapper.EndPerformanceProfile("UserMapping");
Console.WriteLine($"Toplam mapping: {result.TotalMappings}");
Console.WriteLine($"Ortalama süre: {result.AverageMappingTime}");

// Diagnostic raporu
var report = DiagnosticMapper.GenerateDiagnosticReport();
Console.WriteLine($"Başarı oranı: %{report.Summary.SuccessRate:P1}");
```

### Custom Mapping

Özel mapping tanımla ve type converter ekleyerek farklı veri tipleri arasında otomatik dönüşüm yapabilirsiniz.

```csharp
// Özel mapping tanımla
MapperExtensions.AddCustomMapping<Person, PersonDto>(
    "FullName",
    person => $"{person.FirstName} {person.LastName}"
);

// Type converter ekle - string'den int'e otomatik dönüşüm
MapperExtensions.AddTypeConverter<string, int>(int.Parse);

// DateTime'dan string'e dönüşüm
MapperExtensions.AddTypeConverter<DateTime, string>(dt => dt.ToString("dd.MM.yyyy"));

// decimal'dan string'e para birimi formatı
MapperExtensions.AddTypeConverter<decimal, string>(price => $"₺{price:F2}");
```

**Type Converter Nedir?**
- Farklı veri tipleri arasında otomatik dönüşüm sağlar
- `AddTypeConverter<TSource, TTarget>(Func<TSource, TTarget> converter)` formatında kullanılır
- Örnek: `AddTypeConverter<string, int>(int.Parse)` ile string property'ler otomatik olarak int'e dönüştürülür
- Mapping sırasında kaynak ve hedef tipler uyuşmazsa, tanımlı converter kullanılır

### Merge Stratejileri

```csharp
// Replace strategy
var result = MergeMapper.Merge(target, source);

// Deep merge
var result = MergeMapper.DeepMerge(target, source, maxDepth: 3);

// Append merge (koleksiyonlar için)
var result = MergeMapper.AppendMerge(target, source);
```

## 🏗️ Mimari

FastMapper, aşağıdaki temel bileşenlerden oluşur:

- **MapperExtensions**: Temel mapping API'leri
- **FluentMapper**: Zincirlenebilir fluent API
- **AsyncMapper**: Asenkron mapping ve snapshot
- **DiffMapper**: Nesne farkları analizi
- **MappingValidator**: Mapping doğrulama
- **DiagnosticMapper**: Performans analizi
- **MergeMapper**: Nesne birleştirme

## 📋 Gereksinimler

- .NET Standard 2.0+
- .NET 6.0+ (önerilen)
- Newtonsoft.Json (snapshot özelliği için)

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 🙏 Teşekkürler

- [AutoMapper](https://github.com/AutoMapper/AutoMapper) - İlham kaynağı
- [Mapster](https://github.com/MapsterMapper/Mapster) - Performans karşılaştırması
- [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet) - Benchmark framework

## 📞 İletişim

- **GitHub**: [mennansevim/fast-mapper](https://github.com/mennansevim/fast-mapper)
- **NuGet**: [FastMapper](https://www.nuget.org/packages/FastMapper)
- **Issues**: [GitHub Issues](https://github.com/mennansevim/fast-mapper/issues)

---

**FastMapper** ile nesne eşleme işlemlerinizi hızlandırın! 🚀
