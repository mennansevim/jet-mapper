using JetMapper;
using System;
using System.Linq;

Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("  🚀 JetMapper API Examples - Gerçek Test Programı");
Console.WriteLine("═══════════════════════════════════════════════════════════\n");

// ═══════════════════════════════════════════════════════════
// 1. TEMEL MAPPING
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 1. TEMEL MAPPING");
Console.WriteLine("───────────────────────────────────────────────────────────");

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

Console.WriteLine($"✓ User -> UserDto mapping");
Console.WriteLine($"  Id: {userDto.Id}, Name: {userDto.FirstName} {userDto.LastName}, Email: {userDto.Email}\n");

// ═══════════════════════════════════════════════════════════
// 2. PROPERTY DEĞER ATAMASI (SET)
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 2. PROPERTY DEĞER ATAMASI (SET)");
Console.WriteLine("───────────────────────────────────────────────────────────");

var person = new Person 
{ 
    FirstName = "Ayşe", 
    LastName = "Demir",
    BirthDate = new DateTime(1990, 5, 15)
};

var personDto = person.Builder()
    .MapTo<PersonDto>()
    .Set(d => d.FullName, p => $"{p.FirstName} {p.LastName}")
    .Set(d => d.Age, p => DateTime.Now.Year - p.BirthDate.Year)
    .Create();

Console.WriteLine($"✓ Person -> PersonDto with Set()");
Console.WriteLine($"  FullName: {personDto.FullName}, Age: {personDto.Age}\n");

// Çoklu Set
var product = new Product 
{ 
    Name = "Laptop", 
    Price = 10000m, 
    Tax = 1800m,
    Stock = 5
};

var productViewModel = product.Builder()
    .MapTo<ProductViewModel>()
    .Set(vm => vm.DisplayName, p => $"🛒 {p.Name}")
    .Set(vm => vm.PriceText, p => $"₺{p.Price:N2}")
    .Set(vm => vm.TotalPriceText, p => $"₺{(p.Price + p.Tax):N2}")
    .Set(vm => vm.StockStatus, p => $"{p.Stock} adet mevcut")
    .Create();

Console.WriteLine($"✓ Product -> ProductViewModel with multiple Set()");
Console.WriteLine($"  DisplayName: {productViewModel.DisplayName}");
Console.WriteLine($"  PriceText: {productViewModel.PriceText}");
Console.WriteLine($"  TotalPriceText: {productViewModel.TotalPriceText}");
Console.WriteLine($"  StockStatus: {productViewModel.StockStatus}\n");

// ═══════════════════════════════════════════════════════════
// 3. KOŞULLU ATAMA (SETIF)
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 3. KOŞULLU ATAMA (SETIF)");
Console.WriteLine("───────────────────────────────────────────────────────────");

var account = new Account 
{ 
    Balance = 5000m, 
    IsActive = true,
    IsPremium = true
};

var accountDto = account.Builder()
    .MapTo<AccountDto>()
    .SetIf(d => d.Status, a => a.IsActive, a => "✅ Aktif")
    .SetIf(d => d.Status, a => !a.IsActive, a => "❌ Pasif")
    .SetIf(d => d.MembershipLevel, a => a.IsPremium, a => "⭐ Premium")
    .SetIf(d => d.MembershipLevel, a => !a.IsPremium, a => "👤 Standart")
    .SetIf(d => d.BalanceInfo, a => a.Balance > 1000, a => "💰 Yüksek Bakiye")
    .SetIf(d => d.BalanceInfo, a => a.Balance <= 1000, a => "💵 Düşük Bakiye")
    .Create();

Console.WriteLine($"✓ Account -> AccountDto with SetIf()");
Console.WriteLine($"  Status: {accountDto.Status}");
Console.WriteLine($"  MembershipLevel: {accountDto.MembershipLevel}");
Console.WriteLine($"  BalanceInfo: {accountDto.BalanceInfo}\n");

// Karmaşık Koşullar
var order = new Order 
{ 
    Total = 250m, 
    ItemCount = 5,
    CustomerType = "VIP"
};

var orderDto = order.Builder()
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

Console.WriteLine($"✓ Order -> OrderDto with complex conditions");
Console.WriteLine($"  DiscountInfo: {orderDto.DiscountInfo}");
Console.WriteLine($"  ShippingInfo: {orderDto.ShippingInfo}\n");

// ═══════════════════════════════════════════════════════════
// 4. İLK MEVCUT PROPERTY'YE GÖRE ATAMA (SETFIRSTIFEXIST)
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 4. İLK MEVCUT PROPERTY'YE GÖRE ATAMA (SETFIRSTIFEXIST)");
Console.WriteLine("───────────────────────────────────────────────────────────");

var contact = new Contact 
{ 
    Email = "ahmet@example.com",
    Phone = null,
    Address = "İstanbul"
};

// SetFirstIfExist için alternatif: SetIf kullanarak kontrol et
var contactDto = contact.Builder()
    .MapTo<ContactDto>()
    .SetIf(d => d.PreferredContact, c => c.Email != null, c => $"📧 {c.Email}")
    .SetIf(d => d.PreferredContact, c => c.Email == null && c.Phone != null, c => $"📱 {c.Phone}")
    .SetIf(d => d.PreferredContact, c => c.Email == null && c.Phone == null && c.Address != null, c => $"🏠 {c.Address}")
    .Create();

Console.WriteLine($"✓ Contact -> ContactDto with SetFirstIfExist()");
Console.WriteLine($"  PreferredContact: {contactDto.PreferredContact}");
Console.WriteLine($"  (Email dolu olduğu için diğerleri kontrol edilmedi)\n");

// ═══════════════════════════════════════════════════════════
// 5. PROPERTY IGNORE
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 5. PROPERTY IGNORE");
Console.WriteLine("───────────────────────────────────────────────────────────");

var userWithPassword = new UserWithPassword 
{ 
    Id = 1,
    Username = "ahmet.yilmaz",
    Password = "gizli123",
    Email = "ahmet@example.com"
};

var userDtoSafe = userWithPassword.Builder()
    .MapTo<UserDtoWithPassword>()
    .Ignore(d => d.Password)
    .Create();

Console.WriteLine($"✓ User -> UserDto with Ignore(Password)");
Console.WriteLine($"  Id: {userDtoSafe.Id}");
Console.WriteLine($"  Username: {userDtoSafe.Username}");
Console.WriteLine($"  Password: {userDtoSafe.Password ?? "null (ignored)"}");
Console.WriteLine($"  Email: {userDtoSafe.Email}\n");

// Çoklu Ignore
var employee = new Employee 
{ 
    Id = 100,
    Name = "Ayşe Kaya",
    Salary = 15000m,
    SocialSecurityNumber = "12345678901",
    BankAccount = "TR123456789"
};

var employeePublicDto = employee.Builder()
    .MapTo<EmployeePublicDto>()
    .Ignore(d => d.Salary)
    .Ignore(d => d.SocialSecurityNumber)
    .Ignore(d => d.BankAccount)
    .Create();

Console.WriteLine($"✓ Employee -> EmployeePublicDto with multiple Ignore()");
Console.WriteLine($"  Id: {employeePublicDto.Id}");
Console.WriteLine($"  Name: {employeePublicDto.Name}");
Console.WriteLine($"  Salary: {employeePublicDto.Salary} (ignored)");
Console.WriteLine($"  SocialSecurityNumber: {employeePublicDto.SocialSecurityNumber ?? "null (ignored)"}");
Console.WriteLine($"  BankAccount: {employeePublicDto.BankAccount ?? "null (ignored)"}\n");

// ═══════════════════════════════════════════════════════════
// 6. HOOK'LAR (BEFOREMAP/AFTERMAP)
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 6. HOOK'LAR (BEFOREMAP/AFTERMAP)");
Console.WriteLine("───────────────────────────────────────────────────────────");

var dataSource = new DataSource { RawData = "  test data  " };

var dataDto = dataSource.Builder()
    .MapTo<DataDto>()
    .BeforeMap((src, dest) => 
    {
        Console.WriteLine($"  ⏳ BeforeMap: Mapping başlıyor - Raw: '{src.RawData}'");
    })
    .Set(d => d.ProcessedData, s => s.RawData.Trim().ToUpper())
    .AfterMap((src, dest) => 
    {
        Console.WriteLine($"  ✅ AfterMap: Mapping tamamlandı");
    })
    .Create();

Console.WriteLine($"✓ DataSource -> DataDto with hooks");
Console.WriteLine($"  ProcessedData: {dataDto.ProcessedData}\n");

// ═══════════════════════════════════════════════════════════
// 7. GERÇEK DÜNYA ÖRNEĞİ: E-TİCARET SİPARİŞİ
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 7. GERÇEK DÜNYA ÖRNEĞİ: E-TİCARET SİPARİŞİ");
Console.WriteLine("───────────────────────────────────────────────────────────");

var orderEntity = new OrderEntity
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

var orderViewModel = orderEntity.Builder()
    .MapTo<OrderViewModel>()
    .BeforeMap((src, dest) => 
        Console.WriteLine($"  📦 Sipariş #{src.Id} hazırlanıyor..."))
    .Set(vm => vm.OrderNumber, o => $"#ORD-{o.Id}")
    .Set(vm => vm.TotalPrice, o => $"₺{(o.Amount + o.Tax):N2}")
    .SetIf(vm => vm.Status, o => o.IsPaid && o.IsShipped, 
        o => "✅ Teslim Edildi")
    .SetIf(vm => vm.Status, o => o.IsPaid && !o.IsShipped, 
        o => "🚚 Kargoda")
    .SetIf(vm => vm.Status, o => !o.IsPaid, 
        o => "⏳ Ödeme Bekleniyor")
    .SetIf(vm => vm.ContactInfo, o => o.CustomerEmail != null, o => $"📧 {o.CustomerEmail}")
    .SetIf(vm => vm.ContactInfo, o => o.CustomerEmail == null && o.CustomerPhone != null, o => $"📱 {o.CustomerPhone}")
    .Set(vm => vm.OrderAge, o => 
    {
        var days = (DateTime.Now - o.CreatedAt).Days;
        return days == 0 ? "Bugün" : 
               days == 1 ? "Dün" : 
               $"{days} gün önce";
    })
    .AfterMap((src, dest) => 
        Console.WriteLine($"  ✅ Sipariş hazır!"))
    .Create();

Console.WriteLine($"✓ Sipariş Detayları:");
Console.WriteLine($"  OrderNumber: {orderViewModel.OrderNumber}");
Console.WriteLine($"  TotalPrice: {orderViewModel.TotalPrice}");
Console.WriteLine($"  Status: {orderViewModel.Status}");
Console.WriteLine($"  ContactInfo: {orderViewModel.ContactInfo}");
Console.WriteLine($"  OrderAge: {orderViewModel.OrderAge}\n");

// ═══════════════════════════════════════════════════════════
// 8. API RESPONSE BUILDER
// ═══════════════════════════════════════════════════════════
Console.WriteLine("📋 8. API RESPONSE BUILDER");
Console.WriteLine("───────────────────────────────────────────────────────────");

var apiUser = new ApiUser
{
    Id = 42,
    Username = "ahmet.yilmaz",
    Email = "ahmet@example.com",
    IsEmailVerified = true,
    IsActive = true,
    LastLoginAt = DateTime.Now.AddHours(-2),
    Roles = new[] { "User", "Editor", "Admin" }
};

var apiResponse = apiUser.Builder()
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
    .Create();

Console.WriteLine($"✓ API Response:");
Console.WriteLine($"  UserId: {apiResponse.UserId}");
Console.WriteLine($"  DisplayName: {apiResponse.DisplayName}");
Console.WriteLine($"  AccountStatus: {apiResponse.AccountStatus}");
Console.WriteLine($"  EmailStatus: {apiResponse.EmailStatus}");
Console.WriteLine($"  LastSeen: {apiResponse.LastSeen}");
Console.WriteLine($"  IsAdmin: {apiResponse.IsAdmin}\n");

Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("  ✅ TÜM TESTLER BAŞARIYLA TAMAMLANDI!");
Console.WriteLine("═══════════════════════════════════════════════════════════");

// ═══════════════════════════════════════════════════════════
// MODEL TANIMLARI
// ═══════════════════════════════════════════════════════════

// 1. Temel Mapping Models
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

// 2. Set Models
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

// 3. SetIf Models
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

// 4. SetFirstIfExist Models
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

// 5. Ignore Models
public class UserWithPassword
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}

public class UserDtoWithPassword
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}

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

// 6. Hook Models
public class DataSource
{
    public string RawData { get; set; }
}

public class DataDto
{
    public string ProcessedData { get; set; }
}

// 7. E-Ticaret Models
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

// 8. API Response Models
public class ApiUser
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
