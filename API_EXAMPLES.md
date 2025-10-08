# FastMapper - Yeni API Kullanım Örnekleri

Bu doküman, FastMapper'ın yeni Builder Pattern API'sinin kullanım örneklerini içerir.

## 📋 İçindekiler

1. [Temel Mapping](#1-temel-mapping)
2. [Property Değer Ataması (Set)](#2-property-değer-ataması-set)
3. [Koşullu Atama (SetIf)](#3-koşullu-atama-setif)
4. [İlk Mevcut Property'ye Göre Atama (SetFirstIfExist)](#4-ilk-mevcut-propertyye-göre-atama-setfirstifexist)
5. [Property Ignore](#5-property-ignore)
6. [Hook'lar (BeforeMap/AfterMap)](#6-hooklar-beforemap-aftermap)
7. [Kombinasyon Örnekleri](#7-kombinasyon-örnekleri)

---

## 1. Temel Mapping

### Basit Mapping
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

// Kullanım
var user = new User 
{ 
    Id = 1, 
    FirstName = "Ahmet", 
    LastName = "Yılmaz",
    Email = "ahmet@example.com"
};

var userDto = user.Builder()
    .MapTo<UserDto>()
    .Create();

// Sonuç: Tüm property'ler otomatik eşlenir
```

---

## 2. Property Değer Ataması (Set)

### Basit Set Örneği
```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
}

public class PersonDto
{
    public string FullName { get; set; }
    public int Age { get; set; }
}

// Kullanım
var person = new Person 
{ 
    FirstName = "Ayşe", 
    LastName = "Demir",
    BirthDate = new DateTime(1990, 5, 15)
};

var dto = person.Builder()
    .MapTo<PersonDto>()
    .Set(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .Set(d => d.Age, p => DateTime.Now.Year - p.BirthDate.Year)
    .Create();

// Sonuç: FullName = "Ayşe Demir", Age = 35
```

### Çoklu Set Örneği
```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Tax { get; set; }
    public int Stock { get; set; }
}

public class ProductViewModel
{
    public string DisplayName { get; set; }
    public string PriceText { get; set; }
    public string TotalPriceText { get; set; }
    public string StockStatus { get; set; }
}

// Kullanım
var product = new Product 
{ 
    Name = "Laptop", 
    Price = 10000m, 
    Tax = 1800m,
    Stock = 5
};

var viewModel = product.Builder()
    .MapTo<ProductViewModel>()
    .Set(vm => vm.DisplayName, p => $"🛒 {p.Name}")
    .Set(vm => vm.PriceText, p => $"{p.Price:C}")
    .Set(vm => vm.TotalPriceText, p => $"{(p.Price + p.Tax):C}")
    .Set(vm => vm.StockStatus, p => $"{p.Stock} adet mevcut")
    .Create();

// Sonuç:
// DisplayName = "🛒 Laptop"
// PriceText = "₺10,000.00"
// TotalPriceText = "₺11,800.00"
// StockStatus = "5 adet mevcut"
```

---

## 3. Koşullu Atama (SetIf)

### Basit SetIf Örneği
```csharp
public class Account
{
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public bool IsPremium { get; set; }
}

public class AccountDto
{
    public string Status { get; set; }
    public string MembershipLevel { get; set; }
    public string BalanceInfo { get; set; }
}

// Kullanım
var account = new Account 
{ 
    Balance = 5000m, 
    IsActive = true,
    IsPremium = true
};

var dto = account.Builder()
    .MapTo<AccountDto>()
    .SetIf(d => d.Status, a => a.IsActive, a => "✅ Aktif")
    .SetIf(d => d.Status, a => !a.IsActive, a => "❌ Pasif")
    .SetIf(d => d.MembershipLevel, a => a.IsPremium, a => "⭐ Premium")
    .SetIf(d => d.MembershipLevel, a => !a.IsPremium, a => "👤 Standart")
    .SetIf(d => d.BalanceInfo, a => a.Balance > 1000, a => "💰 Yüksek Bakiye")
    .SetIf(d => d.BalanceInfo, a => a.Balance <= 1000, a => "💵 Düşük Bakiye")
    .Create();

// Sonuç:
// Status = "✅ Aktif"
// MembershipLevel = "⭐ Premium"
// BalanceInfo = "💰 Yüksek Bakiye"
```

### Karmaşık Koşullar
```csharp
public class Order
{
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public string CustomerType { get; set; }
}

public class OrderDto
{
    public string DiscountInfo { get; set; }
    public string ShippingInfo { get; set; }
}

// Kullanım
var order = new Order 
{ 
    Total = 250m, 
    ItemCount = 5,
    CustomerType = "VIP"
};

var dto = order.Builder()
    .MapTo<OrderDto>()
    .SetIf(d => d.DiscountInfo, 
        o => o.Total > 200 && o.CustomerType == "VIP", 
        o => "%20 VIP İndirimi")
    .SetIf(d => d.DiscountInfo, 
        o => o.Total > 200 && o.CustomerType != "VIP", 
        o => "%10 İndirimi")
    .SetIf(d => d.ShippingInfo, 
        o => o.Total > 100, 
        o => "🚚 Ücretsiz Kargo")
    .SetIf(d => d.ShippingInfo, 
        o => o.Total <= 100, 
        o => "📦 Kargo: ₺15")
    .Create();

// Sonuç:
// DiscountInfo = "%20 VIP İndirimi"
// ShippingInfo = "🚚 Ücretsiz Kargo"
```

---

## 4. İlk Mevcut Property'ye Göre Atama (SetFirstIfExist)

### Öncelik Sırasıyla Atama
```csharp
public class Contact
{
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}

public class ContactDto
{
    public string PreferredContact { get; set; }
}

// Kullanım
var contact = new Contact 
{ 
    Email = "ahmet@example.com",
    Phone = null,
    Address = "İstanbul"
};

var dto = contact.Builder()
    .MapTo<ContactDto>()
    .SetFirstIfExist(d => d.PreferredContact,
        (d => d.Email, c => $"📧 {c.Email}"),      // Önce email kontrolü
        (d => d.Phone, c => $"📱 {c.Phone}"),      // Sonra phone
        (d => d.Address, c => $"🏠 {c.Address}"))  // En son address
    .Create();

// Sonuç: PreferredContact = "📧 ahmet@example.com"
// (Email dolu olduğu için diğerleri kontrol edilmedi)
```

### VAT Oranı Önceliklendirme
```csharp
public class Invoice
{
    public decimal? Vat18 { get; set; }
    public decimal? Vat20 { get; set; }
    public decimal? Vat8 { get; set; }
}

public class InvoiceDto
{
    public decimal VatRate { get; set; }
}

// Kullanım
var invoice = new Invoice 
{ 
    Vat18 = null,
    Vat20 = 20m,
    Vat8 = 8m
};

var dto = invoice.Builder()
    .MapTo<InvoiceDto>()
    .SetFirstIfExist(d => d.VatRate,
        (d => d.Vat18, i => i.Vat18.Value),  // Önce Vat18
        (d => d.Vat20, i => i.Vat20.Value),  // Sonra Vat20
        (d => d.Vat8, i => i.Vat8.Value))    // En son Vat8
    .Create();

// Sonuç: VatRate = 20 (Vat20 ilk dolu olan)
```

---

## 5. Property Ignore

### Basit Ignore Örneği
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // Bu alanı ignore edeceğiz
    public string Email { get; set; }
}

// Kullanım
var user = new User 
{ 
    Id = 1,
    Username = "ahmet.yilmaz",
    Password = "gizli123",
    Email = "ahmet@example.com"
};

var dto = user.Builder()
    .MapTo<UserDto>()
    .Ignore(d => d.Password)  // Şifreyi DTO'ya kopyalama
    .Create();

// Sonuç:
// Id = 1
// Username = "ahmet.yilmaz"
// Password = null (ignore edildi)
// Email = "ahmet@example.com"
```

### Çoklu Ignore
```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public string SocialSecurityNumber { get; set; }
    public string BankAccount { get; set; }
}

public class EmployeePublicDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public string SocialSecurityNumber { get; set; }
    public string BankAccount { get; set; }
}

// Kullanım
var employee = new Employee 
{ 
    Id = 100,
    Name = "Ayşe Kaya",
    Salary = 15000m,
    SocialSecurityNumber = "12345678901",
    BankAccount = "TR123456789"
};

var publicDto = employee.Builder()
    .MapTo<EmployeePublicDto>()
    .Ignore(d => d.Salary)                 // Maaş bilgisi
    .Ignore(d => d.SocialSecurityNumber)   // TC kimlik
    .Ignore(d => d.BankAccount)            // Banka hesabı
    .Create();

// Sonuç:
// Id = 100
// Name = "Ayşe Kaya"
// Salary = 0 (ignore edildi)
// SocialSecurityNumber = null (ignore edildi)
// BankAccount = null (ignore edildi)
```

---

## 6. Hook'lar (BeforeMap/AfterMap)

### BeforeMap Örneği
```csharp
public class DataSource
{
    public string RawData { get; set; }
}

public class DataDto
{
    public string ProcessedData { get; set; }
}

// Kullanım
var source = new DataSource { RawData = "  test data  " };

var dto = source.Builder()
    .MapTo<DataDto>()
    .BeforeMap((src, dest) => 
    {
        Console.WriteLine($"Mapping başlıyor: {src.RawData}");
        // Veri temizleme, doğrulama vb.
    })
    .Set(d => d.ProcessedData, s => s.RawData.Trim().ToUpper())
    .Create();

// Konsol: "Mapping başlıyor:   test data  "
// Sonuç: ProcessedData = "TEST DATA"
```

### AfterMap Örneği
```csharp
public class Order
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
}

public class OrderDto
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public DateTime ProcessedAt { get; set; }
}

// Kullanım
var order = new Order { OrderId = 123, Total = 500m };

var dto = order.Builder()
    .MapTo<OrderDto>()
    .AfterMap((src, dest) => 
    {
        var orderDto = dest as OrderDto;
        if (orderDto != null)
        {
            orderDto.ProcessedAt = DateTime.Now;
        }
        Console.WriteLine($"Sipariş {src.OrderId} işlendi");
    })
    .Create();

// Konsol: "Sipariş 123 işlendi"
// Sonuç: ProcessedAt otomatik atandı
```

### BeforeMap + AfterMap Kombinasyonu
```csharp
var dto = order.Builder()
    .MapTo<OrderDto>()
    .BeforeMap((src, dest) => Console.WriteLine("⏳ Başlıyor..."))
    .Set(d => d.OrderId, o => o.OrderId)
    .Set(d => d.Total, o => o.Total)
    .AfterMap((src, dest) => Console.WriteLine("✅ Tamamlandı!"))
    .Create();

// Konsol:
// ⏳ Başlıyor...
// ✅ Tamamlandı!
```

---

## 7. Kombinasyon Örnekleri

### Gerçek Dünya Örneği: E-Ticaret Sipariş
```csharp
public class OrderEntity
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Tax { get; set; }
    public bool IsPaid { get; set; }
    public bool IsShipped { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderViewModel
{
    public string OrderNumber { get; set; }
    public string TotalPrice { get; set; }
    public string Status { get; set; }
    public string ContactInfo { get; set; }
    public string OrderAge { get; set; }
}

// Kullanım
var order = new OrderEntity
{
    Id = 12345,
    Amount = 500m,
    Tax = 90m,
    IsPaid = true,
    IsShipped = false,
    CustomerEmail = "musteri@example.com",
    CustomerPhone = "555-1234",
    CreatedAt = DateTime.Now.AddDays(-2)
};

var viewModel = order.Builder()
    .MapTo<OrderViewModel>()
    .BeforeMap((src, dest) => 
        Console.WriteLine($"📦 Sipariş #{src.Id} hazırlanıyor..."))
    
    .Set(vm => vm.OrderNumber, o => $"#ORD-{o.Id}")
    .Set(vm => vm.TotalPrice, o => $"{(o.Amount + o.Tax):C}")
    
    .SetIf(vm => vm.Status, o => o.IsPaid && o.IsShipped, 
        o => "✅ Teslim Edildi")
    .SetIf(vm => vm.Status, o => o.IsPaid && !o.IsShipped, 
        o => "🚚 Kargoda")
    .SetIf(vm => vm.Status, o => !o.IsPaid, 
        o => "⏳ Ödeme Bekleniyor")
    
    .SetFirstIfExist(vm => vm.ContactInfo,
        (vm => vm.CustomerEmail, o => $"📧 {o.CustomerEmail}"),
        (vm => vm.CustomerPhone, o => $"📱 {o.CustomerPhone}"))
    
    .Set(vm => vm.OrderAge, o => 
    {
        var days = (DateTime.Now - o.CreatedAt).Days;
        return days == 0 ? "Bugün" : 
               days == 1 ? "Dün" : 
               $"{days} gün önce";
    })
    
    .AfterMap((src, dest) => 
        Console.WriteLine($"✅ Sipariş hazır!"))
    
    .Create();

// Konsol:
// 📦 Sipariş #12345 hazırlanıyor...
// ✅ Sipariş hazır!
//
// Sonuç:
// OrderNumber = "#ORD-12345"
// TotalPrice = "₺590.00"
// Status = "🚚 Kargoda"
// ContactInfo = "📧 musteri@example.com"
// OrderAge = "2 gün önce"
```

### API Response Builder
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastLoginAt { get; set; }
    public string[] Roles { get; set; }
}

public class UserApiResponse
{
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public string AccountStatus { get; set; }
    public string EmailStatus { get; set; }
    public string LastSeen { get; set; }
    public bool IsAdmin { get; set; }
}

// Kullanım
var user = new User
{
    Id = 42,
    Username = "ahmet.yilmaz",
    Email = "ahmet@example.com",
    IsEmailVerified = true,
    IsActive = true,
    LastLoginAt = DateTime.Now.AddHours(-2),
    Roles = new[] { "User", "Editor", "Admin" }
};

var response = user.Builder()
    .MapTo<UserApiResponse>()
    
    .Set(r => r.UserId, u => u.Id)
    .Set(r => r.DisplayName, u => $"@{u.Username}")
    
    .SetIf(r => r.AccountStatus, u => u.IsActive, 
        u => "✅ Aktif")
    .SetIf(r => r.AccountStatus, u => !u.IsActive, 
        u => "🔒 Askıda")
    
    .SetIf(r => r.EmailStatus, u => u.IsEmailVerified, 
        u => "✉️ Doğrulandı")
    .SetIf(r => r.EmailStatus, u => !u.IsEmailVerified, 
        u => "⚠️ Doğrulanmadı")
    
    .Set(r => r.LastSeen, u =>
    {
        var diff = DateTime.Now - u.LastLoginAt;
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} dakika önce";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} saat önce";
        return $"{(int)diff.TotalDays} gün önce";
    })
    
    .Set(r => r.IsAdmin, u => u.Roles.Contains("Admin"))
    
    .Ignore(r => r.AccountStatus)  // Özel hesaplama kullanıldığı için
    
    .Create();

// Sonuç:
// UserId = 42
// DisplayName = "@ahmet.yilmaz"
// AccountStatus = null (ignored)
// EmailStatus = "✉️ Doğrulandı"
// LastSeen = "2 saat önce"
// IsAdmin = true
```

---

## 🎯 API Özeti

| Method | Açıklama | Örnek |
|--------|----------|-------|
| `Builder()` | Mapping işlemini başlatır | `user.Builder()` |
| `MapTo<T>()` | Hedef tipi belirler | `.MapTo<UserDto>()` |
| `Set()` | Property değeri atar | `.Set(d => d.Name, s => s.FullName)` |
| `SetIf()` | Koşullu değer atar | `.SetIf(d => d.Status, s => s.IsActive, s => "Active")` |
| `SetFirstIfExist()` | İlk mevcut property'ye göre atar | `.SetFirstIfExist(d => d.Contact, ...)` |
| `Ignore()` | Property'yi atlamaz | `.Ignore(d => d.Password)` |
| `BeforeMap()` | Mapping öncesi çalışır | `.BeforeMap((s, d) => {...})` |
| `AfterMap()` | Mapping sonrası çalışır | `.AfterMap((s, d) => {...})` |
| `Create()` | Mapping'i tamamlar | `.Create()` |

---

## 💡 İpuçları

1. **Set() kullanın** - Basit değer atamaları için
2. **SetIf() kullanın** - Tek koşul varsa
3. **SetFirstIfExist() kullanın** - Öncelik sırasına göre atama yapacaksanız
4. **Ignore() kullanın** - Hassas verileri kopyalamamak için
5. **BeforeMap/AfterMap** - Loglama, validasyon için

---

## 🚀 Geriye Dönük Uyumluluk

Eski API hala çalışır (deprecated warning ile):

```csharp
// ESKİ API (hala çalışır)
var result = person.Map()
    .MapTo<PersonDto>()
    .Map(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .To();

// YENİ API (önerilen)
var result = person.Builder()
    .MapTo<PersonDto>()
    .Set(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .Create();
```

Her iki API de aynı sonucu verir! 🎉

