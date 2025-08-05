# FastMapper Kullanım Örnekleri

Bu doküman, FastMapper'ın tüm özelliklerini pratik örneklerle açıklar.

## 📋 İçindekiler

1. [Temel Mapping](#temel-mapping)
2. [Fluent API](#fluent-api)
3. [Koşullu Mapping](#koşullu-mapping)
4. [Custom Mapping](#custom-mapping)
5. [Type Converter](#type-converter)
6. [Asenkron Mapping](#asenkron-mapping)
7. [Diff Mapping](#diff-mapping)
8. [Snapshot & Restore](#snapshot--restore)
9. [Mapping Validator](#mapping-validator)
10. [Partial Merge](#partial-merge)
11. [Diagnostic & Profiling](#diagnostic--profiling)
12. [Gerçek Dünya Senaryoları](#gerçek-dünya-senaryoları)

---

## 🚀 Temel Mapping

### Basit Nesne Eşleme

```csharp
using FastMapper;

// Kaynak sınıf
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public bool IsActive { get; set; }
}

// Hedef sınıf
public class PersonDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public bool IsActive { get; set; }
}

// Kullanım
var person = new Person 
{ 
    Id = 1, 
    FirstName = "John", 
    LastName = "Doe", 
    BirthDate = new DateTime(1990, 1, 1),
    IsActive = true 
};

var dto = person.FastMapTo<PersonDto>();
Console.WriteLine($"DTO: {dto.FirstName} {dto.LastName}");
```

### Mevcut Nesneye Mapping

```csharp
var existingDto = new PersonDto { Id = 1 };
person.FastMapTo(existingDto); // Mevcut nesneyi günceller
```

### Koleksiyon Mapping

```csharp
var personList = new List<Person>
{
    new Person { Id = 1, FirstName = "John", LastName = "Doe" },
    new Person { Id = 2, FirstName = "Jane", LastName = "Smith" }
};

var dtoList = personList.Cast<object>().FastMapToList<PersonDto>();
```

---

## 🔗 Fluent API

### Zincirlenebilir Mapping

```csharp
var result = person.Map()
    .Map<PersonDto>(dto => dto.FullName, p => $"{p.FirstName} {p.LastName}")
    .Map<PersonDto>(dto => dto.Age, p => DateTime.Now.Year - p.BirthDate.Year)
    .Map<PersonDto>(dto => dto.Status, p => p.IsActive ? "Active" : "Inactive")
    .Ignore<PersonDto>(dto => dto.InternalId)
    .To<PersonDto>();
```

### BeforeMap ve AfterMap Hook'ları

```csharp
var result = person.Map()
    .BeforeMap((source, target) => 
    {
        Console.WriteLine($"Mapping başlıyor: {source.FirstName}");
    })
    .AfterMap((source, target) => 
    {
        Console.WriteLine($"Mapping tamamlandı: {target.FirstName}");
    })
    .To<PersonDto>();
```

---

## 🎯 Koşullu Mapping

### Koşullu Property Mapping

```csharp
var result = person.Map()
    .MapIf<PersonDto>(dto => dto.Status, 
        p => p.IsActive, 
        p => "Active")
    .MapIf<PersonDto>(dto => dto.Age, 
        p => p.BirthDate != default(DateTime), 
        p => DateTime.Now.Year - p.BirthDate.Year)
    .MapIf<PersonDto>(dto => dto.FullName, 
        p => !string.IsNullOrEmpty(p.FirstName), 
        p => $"{p.FirstName} {p.LastName}")
    .To<PersonDto>();
```

### Karmaşık Koşullar

```csharp
var result = person.Map()
    .MapIf<PersonDto>(dto => dto.Category, 
        p => p.Age >= 18, 
        p => "Adult")
    .MapIf<PersonDto>(dto => dto.Discount, 
        p => p.IsActive && p.Age > 65, 
        p => 0.15m)
    .To<PersonDto>();
```

### Hedef Property Kontrolü ile Mapping

Hedef nesnenin belirli property'lerinin null olup olmadığını kontrol ederek mapping yapabilirsiniz:

```csharp
public class Invoice
{
    public decimal Amount { get; set; }
    public decimal? Vat18 { get; set; }
    public decimal? Vat20 { get; set; }
    public decimal? Vat8 { get; set; }
}

public class InvoiceDto
{
    public decimal Amount { get; set; }
    public decimal? Vat18 { get; set; }
    public decimal? Vat20 { get; set; }
    public decimal? Vat8 { get; set; }
    public decimal? VatRate { get; set; }  // Bu alan koşullu olarak doldurulacak
}

// Hedef property kontrolü ile mapping
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

### Çoklu Hedef Property Kontrolü

```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
}

public class ProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? DisplayInfo { get; set; }  // Koşullu olarak doldurulacak
}

// Hedef property'lere göre farklı mapping
var result = product.Map()
    .MapIf<ProductDto>(dto => dto.DisplayInfo, 
        dto => dto.Description,  // Eğer Description null değilse
        p => $"Açıklama: {p.Description}")
    .MapIf<ProductDto>(dto => dto.DisplayInfo, 
        dto => dto.Category,     // Eğer Category null değilse
        p => $"Kategori: {p.Category}")
    .MapIf<ProductDto>(dto => dto.DisplayInfo, 
        dto => dto.Name,         // Eğer Name null değilse (her zaman true olur)
        p => $"Ürün: {p.Name}")
    .To<ProductDto>();
```

### Karmaşık Hedef Property Kontrolü

```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class UserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ContactInfo { get; set; }  // Koşullu olarak doldurulacak
}

// Öncelik sırasına göre contact bilgisi
var result = user.Map()
    .MapIf<UserDto>(dto => dto.ContactInfo, 
        dto => dto.Email,    // Önce email kontrol et
        u => $"Email: {u.Email}")
    .MapIf<UserDto>(dto => dto.ContactInfo, 
        dto => dto.Phone,    // Sonra telefon kontrol et
        u => $"Telefon: {u.Phone}")
    .MapIf<UserDto>(dto => dto.ContactInfo, 
        dto => dto.FirstName, // Son çare olarak isim
        u => $"İletişim: {u.FirstName} {u.LastName}")
    .To<UserDto>();
```

### If-Else If-Else Mantığı ile Mapping

`MapIfElse` methodu ile if-else if-else mantığında koşullu mapping yapabilirsiniz:

```csharp
public class Invoice
{
    public decimal Amount { get; set; }
    public decimal? Vat18 { get; set; }
    public decimal? Vat20 { get; set; }
    public decimal? Vat8 { get; set; }
}

public class InvoiceDto
{
    public decimal Amount { get; set; }
    public decimal? Vat18 { get; set; }
    public decimal? Vat20 { get; set; }
    public decimal? Vat8 { get; set; }
    public decimal? VatRate { get; set; }  // Bu alan koşullu olarak doldurulacak
}

// If-else if-else mantığı ile mapping
var result = invoice.Map()
    .MapIfElse<InvoiceDto>(dto => dto.VatRate,
        (dto => dto.Vat18, i => i.Vat18),    // if Vat18 != null
        (dto => dto.Vat20, i => i.Vat20),    // else if Vat20 != null
        (dto => dto.Vat8, i => i.Vat8))      // else if Vat8 != null
    .To<InvoiceDto>();
```

**MapIfElse Özellikleri:**
- **Öncelik Sırası**: İlk koşul sağlanırsa, diğer koşullar kontrol edilmez
- **If-Else If-Else Mantığı**: Geleneksel if-else if-else yapısı
- **Performans**: Gereksiz kontroller yapılmaz
- **Esneklik**: Birden fazla koşul parametresi alabilir

**Kullanım Senaryoları:**
- **VAT Oranı Önceliği**: Vat18 > Vat20 > Vat8
- **İletişim Bilgisi Önceliği**: Email > Phone > Name
- **Ürün Bilgisi Önceliği**: Description > Category > Brand
- **İndirim Sebebi Önceliği**: DiscountCode > PremiumCustomer > Loyalty

### Çoklu Koşul Önceliği

```csharp
public class Product
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
}

public class ProductDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? DisplayInfo { get; set; }  // Koşullu olarak doldurulacak
}

// Öncelik sırası: Description > Category > Brand > Name
var result = product.Map()
    .MapIfElse<ProductDto>(dto => dto.DisplayInfo,
        (dto => dto.Description, p => $"Açıklama: {p.Description}"),  // if Description != null
        (dto => dto.Category, p => $"Kategori: {p.Category}"),       // else if Category != null
        (dto => dto.Brand, p => $"Marka: {p.Brand}"),                // else if Brand != null
        (dto => dto.Name, p => $"Ürün: {p.Name}"))                   // else if Name != null
    .To<ProductDto>();
```

### Karmaşık İş Mantığı

```csharp
public class Order
{
    public decimal TotalAmount { get; set; }
    public string? DiscountCode { get; set; }
    public bool IsPremiumCustomer { get; set; }
    public int OrderCount { get; set; }
}

public class OrderDto
{
    public decimal TotalAmount { get; set; }
    public string? DiscountCode { get; set; }
    public bool IsPremiumCustomer { get; set; }
    public int OrderCount { get; set; }
    public string? DiscountReason { get; set; }  // Koşullu olarak doldurulacak
}

// İş mantığına göre indirim sebebi
var result = order.Map()
    .MapIfElse<OrderDto>(dto => dto.DiscountReason,
        (dto => dto.DiscountCode, o => $"Kod: {o.DiscountCode}"),           // if DiscountCode != null
        (dto => dto.IsPremiumCustomer, o => "Premium müşteri indirimi"),     // else if IsPremiumCustomer
        (dto => dto.OrderCount, o => $"Sadık müşteri ({o.OrderCount} sipariş)")) // else if OrderCount > 0
    .To<OrderDto>();
```

---

## 🛠️ Custom Mapping

### Özel Property Mapping

```csharp
// Setup'da bir kez tanımla
MapperExtensions.AddCustomMapping<Person, PersonDto>(
    "FullName",
    person => $"{person.FirstName} {person.LastName}"
);

// Kullan
var dto = person.FastMapTo<PersonDto>(); // FullName otomatik doldurulur
```

### Karmaşık Custom Mapping

```csharp
MapperExtensions.AddCustomMapping<Person, PersonDto>(
    "ProfileSummary",
    person => 
    {
        var age = DateTime.Now.Year - person.BirthDate.Year;
        var status = person.IsActive ? "Active" : "Inactive";
        return $"{person.FirstName} ({age} yaşında, {status})";
    }
);
```

---

## 🔄 Type Converter

### Temel Type Converter

```csharp
// String'den int'e dönüşüm
MapperExtensions.AddTypeConverter<string, int>(int.Parse);

// DateTime'dan string'e dönüşüm
MapperExtensions.AddTypeConverter<DateTime, string>(dt => dt.ToString("dd.MM.yyyy"));

// decimal'dan string'e para birimi formatı
MapperExtensions.AddTypeConverter<decimal, string>(price => $"₺{price:F2}");
```

### Karmaşık Type Converter

```csharp
// Enum'dan string'e dönüşüm
MapperExtensions.AddTypeConverter<UserStatus, string>(status => 
    status switch
    {
        UserStatus.Active => "Aktif",
        UserStatus.Inactive => "Pasif",
        UserStatus.Suspended => "Askıya Alınmış",
        _ => "Bilinmiyor"
    });

// String'den enum'a dönüşüm
MapperExtensions.AddTypeConverter<string, UserStatus>(str => 
    Enum.Parse<UserStatus>(str, true));
```

---

## ⚡ Asenkron Mapping

### Basit Async Mapping

```csharp
var personList = GetLargePersonList(); // 1000+ kayıt
var results = await AsyncMapper.MapAsync<Person, PersonDto>(personList);

Console.WriteLine($"Toplam {results.Count} kayıt işlendi");
```

### Progress Reporting ile

```csharp
var progress = new Progress<AsyncMapper.MappingProgress>(p =>
{
    Console.WriteLine($"İlerleme: %{p.Percentage:F1} ({p.ProcessedCount}/{p.TotalCount})");
});

var results = await AsyncMapper.MapAsync<Person, PersonDto>(personList, progress);
```

### Concurrency Control

```csharp
// Maksimum 4 paralel işlem
var results = await AsyncMapper.MapAsync<Person, PersonDto>(personList, maxConcurrency: 4);
```

---

## 🔍 Diff Mapping

### Basit Fark Bulma

```csharp
var original = new Person { Id = 1, FirstName = "John", LastName = "Doe" };
var updated = new Person { Id = 1, FirstName = "Jane", LastName = "Smith" };

var diff = DiffMapper.FindDifferences(original, updated);

if (diff.HasDifferences)
{
    Console.WriteLine($"Fark sayısı: {diff.Differences.Count}");
    Console.WriteLine($"Benzerlik: %{diff.SimilarityPercentage}");
    
    foreach (var difference in diff.Differences)
    {
        Console.WriteLine($"- {difference.PropertyName}: {difference.OldValue} → {difference.NewValue}");
    }
}
```

### Karmaşık Nesne Farkları

```csharp
public class ComplexPerson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public List<string> Hobbies { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

var original = new ComplexPerson 
{ 
    Id = 1, 
    Name = "John",
    Address = new Address { Street = "Main St", City = "NYC" },
    Hobbies = new List<string> { "Reading", "Swimming" }
};

var updated = new ComplexPerson 
{ 
    Id = 1, 
    Name = "John",
    Address = new Address { Street = "Oak St", City = "LA" },
    Hobbies = new List<string> { "Reading", "Gaming" }
};

var diff = DiffMapper.FindDifferences(original, updated);
```

---

## 💾 Snapshot & Restore

### Basit Snapshot

```csharp
// Snapshot oluştur
var snapshot = AsyncMapper.CreateSnapshot(person);

// Snapshot bilgilerini görüntüle
Console.WriteLine($"Snapshot ID: {snapshot.Id}");
Console.WriteLine($"Oluşturulma: {snapshot.CreatedAt}");
Console.WriteLine($"Boyut: {snapshot.SerializedData.Length} bytes");

// Snapshot'tan geri yükle
var restored = AsyncMapper.RestoreFromSnapshot<Person>(snapshot.Id);
```

### Deep Copy Snapshot

```csharp
// Deep copy snapshot oluştur
var deepSnapshot = AsyncMapper.CreateDeepCopySnapshot(person);

// Deep copy'den geri yükle
var deepRestored = AsyncMapper.RestoreFromSnapshot<Person>(deepSnapshot.Id);
```

### Snapshot Yönetimi

```csharp
// Tüm snapshot'ları listele
var snapshots = AsyncMapper.GetAllSnapshots();

// Belirli bir snapshot'ı sil
AsyncMapper.DeleteSnapshot(snapshotId);

// Eski snapshot'ları temizle (7 günden eski)
AsyncMapper.CleanupOldSnapshots(TimeSpan.FromDays(7));
```

---

## ✅ Mapping Validator

### Basit Validation

```csharp
var result = MappingValidator.ValidateMapping<Person, PersonDto>();

if (result.IsValid)
{
    Console.WriteLine("Mapping geçerli!");
}
else
{
    Console.WriteLine($"Mapping hataları: {result.Errors.Count}");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error.Message}");
    }
}
```

### Detaylı Validation Raporu

```csharp
var result = MappingValidator.ValidateMapping<Person, PersonDto>();

Console.WriteLine($"Geçerli mi: {result.IsValid}");
Console.WriteLine($"Hata sayısı: {result.Errors.Count}");
Console.WriteLine($"Uyarı sayısı: {result.Warnings.Count}");
Console.WriteLine($"Property sayısı: {result.PropertyCount}");

foreach (var property in result.Properties)
{
    Console.WriteLine($"- {property.Name}: {property.SourceType} → {property.TargetType}");
}
```

---

## 🔄 Partial Merge

### Basit Merge

```csharp
var target = new Person { Id = 1, FirstName = "John" };
var source = new Person { Id = 1, FirstName = "Jane", LastName = "Smith" };

// Sadece belirli alanları güncelle
var result = MergeMapper.PartialMerge(target, source, "FirstName", "LastName");

Console.WriteLine($"Güncellenen alan sayısı: {result.UpdatedProperties.Count}");
```

### Koşullu Merge

```csharp
var result = MergeMapper.ConditionalMerge(target, source, 
    (sourceValue, targetValue) => sourceValue != null);

// Sadece null olmayan değerlerle güncelle
```

### Deep Merge

```csharp
var result = MergeMapper.DeepMerge(target, source, maxDepth: 3);

// İç içe nesneleri de merge et
```

---

## 📊 Diagnostic & Profiling

### Performance Profili

```csharp
// Profil başlat
var profile = DiagnosticMapper.StartPerformanceProfile("UserMapping");

// Mapping işlemleri...
for (int i = 0; i < 1000; i++)
{
    var dto = person.FastMapTo<PersonDto>();
}

// Profil bitir
var result = DiagnosticMapper.EndPerformanceProfile("UserMapping");

Console.WriteLine($"Toplam mapping: {result.TotalMappings}");
Console.WriteLine($"Ortalama süre: {result.AverageMappingTime}");
Console.WriteLine($"Toplam süre: {result.TotalTime}");
```

### Diagnostic Raporu

```csharp
var report = DiagnosticMapper.GenerateDiagnosticReport();

Console.WriteLine($"Başarı oranı: %{report.Summary.SuccessRate:P1}");
Console.WriteLine($"Toplam çağrı: {report.Summary.TotalCalls}");
Console.WriteLine($"Ortalama süre: {report.Summary.AverageTime}");
Console.WriteLine($"Hata sayısı: {report.Summary.ErrorCount}");

// En çok kullanılan mapping'ler
foreach (var mapping in report.TopMappings)
{
    Console.WriteLine($"- {mapping.SourceType} → {mapping.TargetType}: {mapping.CallCount} çağrı");
}
```

---

## 🌍 Gerçek Dünya Senaryoları

### E-Ticaret Senaryosu

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Price { get; set; }        // decimal → string
    public string Stock { get; set; }        // int → string
    public string Status { get; set; }       // bool → string
    public string CreatedDate { get; set; }  // DateTime → string
}

// Type converter'ları tanımla
MapperExtensions.AddTypeConverter<decimal, string>(price => $"₺{price:F2}");
MapperExtensions.AddTypeConverter<int, string>(stock => $"{stock} adet");
MapperExtensions.AddTypeConverter<bool, string>(active => active ? "Stokta" : "Stokta Yok");
MapperExtensions.AddTypeConverter<DateTime, string>(date => date.ToString("dd.MM.yyyy HH:mm"));

// Kullan
var products = GetProductList();
var productDtos = products.Cast<object>().FastMapToList<ProductListDto>();
```

### Blog/CMS Senaryosu

```csharp
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string AuthorName { get; set; }
    public DateTime PublishDate { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
}

public class BlogPostSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }      // Özel: İçeriğin ilk 100 karakteri
    public string AuthorName { get; set; }
    public string PublishDate { get; set; }
    public string Status { get; set; }       // Özel: Yayın durumu + görüntüleme sayısı
}

// Custom mapping'ler
MapperExtensions.AddCustomMapping<BlogPost, BlogPostSummaryDto>(
    "Content", "Summary",
    post => post.Content.Length > 100 ? post.Content.Substring(0, 100) + "..." : post.Content
);

MapperExtensions.AddCustomMapping<BlogPost, BlogPostSummaryDto>(
    "IsPublished", "Status",
    post => 
    {
        var status = post.IsPublished ? "Yayında" : "Taslak";
        return $"{status} ({post.ViewCount:N0} görüntüleme)";
    }
);

// Kullan
var blogPosts = GetBlogPosts();
var summaries = blogPosts.Cast<object>().FastMapToList<BlogPostSummaryDto>();
```

### API Response Mapping

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public string Timestamp { get; set; }  // DateTime → string
}

// Type converter
MapperExtensions.AddTypeConverter<DateTime, string>(dt => dt.ToString("yyyy-MM-ddTHH:mm:ssZ"));

// Kullan
var response = new ApiResponse<Person> 
{ 
    Success = true, 
    Message = "OK", 
    Data = person,
    Timestamp = DateTime.UtcNow 
};

var responseDto = response.FastMapTo<ApiResponseDto<PersonDto>>();
```

---

## 🎯 En İyi Uygulamalar

### 1. Performance Optimizasyonu

```csharp
// Uygulama başlangıcında cache'leri ısıt
public static class FastMapperInitializer
{
    public static void WarmUpCaches()
    {
        var dummyPerson = new Person();
        var dummyProduct = new Product();
        
        _ = dummyPerson.FastMapTo<PersonDto>();
        _ = dummyProduct.FastMapTo<ProductDto>();
        
        Console.WriteLine("FastMapper cache'leri ısıtıldı!");
    }
}
```

### 2. Error Handling

```csharp
try
{
    var result = person.FastMapTo<PersonDto>();
}
catch (MappingException ex)
{
    Console.WriteLine($"Mapping hatası: {ex.Message}");
    // Fallback mapping veya hata işleme
}
```

### 3. Validation ile Güvenli Mapping

```csharp
// Mapping'i önceden doğrula
var validation = MappingValidator.ValidateMapping<Person, PersonDto>();
if (!validation.IsValid)
{
    throw new InvalidOperationException("Mapping konfigürasyonu geçersiz");
}

// Güvenli mapping yap
var result = person.FastMapTo<PersonDto>();
```

Bu örnekler FastMapper'ın tüm özelliklerini kapsamlı bir şekilde gösterir. İhtiyacınıza göre bu örnekleri uyarlayabilir ve geliştirebilirsiniz. 