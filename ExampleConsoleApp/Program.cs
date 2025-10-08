using System;
using FastMapper;

namespace ExampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("🚀 FastMapper - Builder Pattern API Örneği");
            Console.WriteLine("===========================================\n");

            // Test verisi oluştur
            var sourcePerson = new SourcePerson
            {
                Id = 123,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                BirthDate = new DateTime(1990, 5, 15),
                IsActive = true,
                Email = "ahmet.yilmaz@email.com",
                PhoneNumber = "+90 555 123 45 67",
                Address = new SourceAddress
                {
                    Street = "Atatürk Caddesi",
                    City = "İstanbul",
                    Country = "Türkiye",
                    PostalCode = "34000"
                }
            };

            Console.WriteLine("📝 Kaynak Veri:");
            Console.WriteLine($"ID: {sourcePerson.Id}");
            Console.WriteLine($"Ad: {sourcePerson.FirstName}");
            Console.WriteLine($"Soyad: {sourcePerson.LastName}");
            Console.WriteLine($"Doğum Tarihi: {sourcePerson.BirthDate:dd.MM.yyyy}");
            Console.WriteLine($"Aktif: {sourcePerson.IsActive}");
            Console.WriteLine($"E-posta: {sourcePerson.Email}");
            Console.WriteLine($"Telefon: {sourcePerson.PhoneNumber}");
            Console.WriteLine($"Adres: {sourcePerson.Address.Street}, {sourcePerson.Address.City}");

            Console.WriteLine("\n🔄 Mapping İşlemi Başlıyor...\n");

            // YENİ API (Builder Pattern - Set ile değer ataması)
            var newResult = sourcePerson.Builder()
                .MapTo<TargetPerson>()
                .Set(t => t.Identifier, s => s.Id)
                .Set(t => t.FullName, s => $"{s.FirstName} {s.LastName}")
                .Set(t => t.Age, s => DateTime.Now.Year - s.BirthDate.Year)
                .Set(t => t.Status, s => s.IsActive ? "Aktif" : "Pasif")
                .Set(t => t.ContactInfo, s => $"{s.Email} | {s.PhoneNumber}")
                .Set(t => t.TargetAddress, s => s.Address)
                .Set(t => t.CreatedDateString, s => DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"))
                .Create();

            Console.WriteLine($"✅ Sonuç: {newResult.FullName} ({newResult.Age} yaşında) - {newResult.Status}");

            // Koşullu Mapping Örneği
            Console.WriteLine("\n🎯 Koşullu Mapping Örneği:");
            var conditionalResult = sourcePerson.Builder()
                .MapTo<TargetPerson>()
                .Set(t => t.Identifier, s => s.Id)
                .Set(t => t.FullName, s => $"{s.FirstName} {s.LastName}")
                .SetIf(t => t.Status, s => s.IsActive, s => "✅ Aktif Kullanıcı")
                .SetIf(t => t.Age, s => s.BirthDate != default(DateTime), s => DateTime.Now.Year - s.BirthDate.Year)
                .SetIf(t => t.ContactInfo, s => !string.IsNullOrEmpty(s.Email), s => $"📧 {s.Email}")
                .Create();

            Console.WriteLine($"✅ Koşullu Sonuç: {conditionalResult.FullName} - {conditionalResult.Status}");

            // Property Ignore Örneği
            Console.WriteLine("\n🚫 Property Ignore Örneği:");
            var ignoreResult = sourcePerson.Builder()
                .MapTo<TargetPerson>()
                .Set(t => t.Identifier, s => s.Id)
                .Set(t => t.FullName, s => $"{s.FirstName} {s.LastName}")
                .Ignore(t => t.Age)
                .Ignore(t => t.Status)
                .Ignore(t => t.ContactInfo)
                .Create();

            Console.WriteLine($"✅ Ignore Sonuç: {ignoreResult.FullName} - Age: {ignoreResult.Age}, Status: {ignoreResult.Status}");

            Console.WriteLine("\n🎉 Tüm örnekler başarıyla tamamlandı!");
            Console.WriteLine("\n💡 Builder Pattern API'nin avantajları:");
            Console.WriteLine("   • Builder() ile başlatma - basit");
            Console.WriteLine("   • MapTo<TTarget>() ile hedef tip belirtme - bir kez");
            Console.WriteLine("   • Set() ile property değer ataması - semantik ve açık");
            Console.WriteLine("   • SetIf() ile koşullu atama - esnek");
            Console.WriteLine("   • Create() ile sonlandırma - basit ve parametresiz");
            Console.WriteLine("   • Daha temiz ve okunabilir kod");
            Console.WriteLine("   • Type-safe: Compile-time'da tip kontrolü");
            Console.WriteLine("   • Geriye dönük uyumluluk korunur (eski API hala çalışır)");
        }
    }

    // Kaynak modeller
    public class SourcePerson
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public SourceAddress Address { get; set; }
    }

    public class SourceAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    // Hedef modeller
    public class TargetPerson
    {
        public int Identifier { get; set; }
        public string FullName { get; set; }
        public int Age { get; set; }
        public string Status { get; set; }
        public string ContactInfo { get; set; }
        public TargetAddress TargetAddress { get; set; }
        public string CreatedDateString { get; set; }
    }

    public class TargetAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }
}

