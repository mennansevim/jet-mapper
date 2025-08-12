# FastMapper Release DLL Export

Bu klasör, FastMapper projesinin Release modunda derlenmiş DLL'lerini ve gerekli dosyalarını içerir.

## 📁 Dosyalar

- **FastMapper.dll** (70KB) - Ana FastMapper kütüphanesi
- **FastMapper.pdb** (29KB) - Debug sembolleri (production'da gerekli değil)
- **FastMapper.deps.json** (1.9KB) - Dependency bilgileri

## 🚀 Kullanım

### .NET Standard 2.0 Projeleri
Bu DLL'ler .NET Standard 2.0 uyumlu projelerde kullanılabilir:
- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5+
- .NET 6+
- .NET 7+
- .NET 8+

### Kurulum
1. DLL'yi projenizin lib klasörüne kopyalayın
2. Proje referansı olarak ekleyin
3. `using FastMapper;` ile kullanmaya başlayın

## 🔧 FastMapper Kullanım Kılavuzu

### 1️⃣ DLL Referansı Ekleme
```csharp
// Proje referanslarına FastMapper.dll'i ekleyin
// Visual Studio: Add Reference > Browse > FastMapper.dll
// .NET CLI: Proje dosyasına manuel olarak ekleyin
```

### 2️⃣ Using Statement Ekleme
```csharp
using FastMapper;
```

### 3️⃣ Basit Kullanım (Configuration Gerektirmez)
```csharp
// Basit mapping - hiçbir configuration gerekmez
var source = new Person { Name = "John", Age = 30 };
var target = source.FastMapTo<PersonDto>();

// Var olan nesneye mapping
var existingTarget = new PersonDto();
source.FastMapTo(existingTarget);

// Toplu mapping
var sources = new List<Person> { /* ... */ };
var targets = sources.FastMapToList<PersonDto>();
```

### 4️⃣ Custom Mapping Ekleme (İsteğe Bağlı)
```csharp
// Custom mapping tanımlama
MapperExtensions.AddCustomMapping<Person, PersonDto>(
    "FullName",           // Source property
    "DisplayName",        // Target property
    person => $"{person.FirstName} {person.LastName}"  // Custom logic
);

// Kullanım
var person = new Person { FirstName = "John", LastName = "Doe" };
var dto = person.FastMapTo<PersonDto>();
// dto.DisplayName = "John Doe" olacak
```

### 5️⃣ Type Converter Ekleme (İsteğe Bağlı)
```csharp
// String'den int'e converter
MapperExtensions.AddTypeConverter<string, int>(
    value => int.Parse(value)
);

// Kullanım
var source = new Source { NumberAsString = "42" };
var target = source.FastMapTo<Target>();
// target.Number = 42 olacak
```

### 6️⃣ Combine Mapping (İsteğe Bağlı)
```csharp
// Birden fazla source'dan mapping
var person = new Person { FirstName = "John", LastName = "Doe" };
var address = new Address { City = "Istanbul", Country = "Turkey" };

var combined = new CombinedDto();
person.FastMapTo(combined);
address.FastMapTo(combined);
```

## 📋 Tam Örnek

```csharp
using FastMapper;

public class Program
{
    public static void Main()
    {
        // Custom mapping tanımla
        MapperExtensions.AddCustomMapping<Person, PersonDto>(
            "FullName", 
            "DisplayName", 
            p => $"{p.FirstName} {p.LastName}"
        );

        // Type converter tanımla
        MapperExtensions.AddTypeConverter<string, int>(
            value => int.Parse(value)
        );

        // Kullanım
        var person = new Person 
        { 
            FirstName = "John", 
            LastName = "Doe",
            AgeAsString = "30"
        };

        var dto = person.FastMapTo<PersonDto>();
        
        Console.WriteLine($"DisplayName: {dto.DisplayName}");
        Console.WriteLine($"Age: {dto.Age}");
    }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string AgeAsString { get; set; }
}

public class PersonDto
{
    public string DisplayName { get; set; }
    public int Age { get; set; }
}
```

## ⚡ Önemli Noktalar

1. **Global Configuration Yok**: AutoMapper gibi `Mapper.Initialize()` gerekmez
2. **Otomatik Property Mapping**: Aynı isimli property'ler otomatik map edilir
3. **Lazy Loading**: Mapping'ler ilk kullanımda compile edilir
4. **Thread Safe**: Tüm mapping'ler thread-safe
5. **Memory Optimized**: Expression tree compilation ile optimize edilir

## 🚫 Yapılmayanlar

- Global configuration
- Profile tanımlama
- Complex mapping rules
- Validation rules

## 📦 NuGet Paketi
Alternatif olarak NuGet paketini de kullanabilirsiniz:
```bash
dotnet add package FastMapper
```

## 🔧 Gereksinimler
- .NET Standard 2.0 runtime
- Minimum .NET Framework 4.6.1

## 📄 Lisans
MIT License - Detaylar için ana proje README'sine bakın.

---

**FastMapper - Ultra-Performans Object Mapper** 🚀
