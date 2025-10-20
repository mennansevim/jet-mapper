using System;
using System.Linq;
using FastMapper;

namespace ExampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("       ⚡ FastMapper");
            Console.WriteLine();
            Console.WriteLine("       Basit. Hızlı. Güçlü.");
            Console.WriteLine();
            Console.WriteLine();
            
            // Tek Satır
            Console.WriteLine("  ────────────────────────────────────────");
            Console.WriteLine();
            Console.WriteLine("  Tek Satır");
            Console.WriteLine();
            
            var user = new User { FirstName = "Ahmet", LastName = "Yılmaz", Age = 25 };
            var dto = user.FastMapTo<UserDto>();
            
            Console.WriteLine($"  {dto.FirstName} {dto.LastName}, {dto.Age} yaş");
            Console.WriteLine();
            
            // Koleksiyonlar
            Console.WriteLine("  ────────────────────────────────────────");
            Console.WriteLine();
            Console.WriteLine("  Koleksiyonlar");
            Console.WriteLine();
            
            var users = new[] 
            {
                new User { FirstName = "Ahmet", LastName = "Yılmaz", Age = 25 },
                new User { FirstName = "Ayşe", LastName = "Demir", Age = 30 },
                new User { FirstName = "Mehmet", LastName = "Kaya", Age = 28 }
            };
            
            var dtos = users.FastMapToList<User, UserDto>();
            
            foreach (var d in dtos)
                Console.WriteLine($"  • {d.FirstName} {d.LastName}");
            
            Console.WriteLine();
            
            // Set / SetIf / Existing / TypeConverter / CustomMapping
            Console.WriteLine("  ────────────────────────────────────────");
            Console.WriteLine();
            Console.WriteLine("  Özellikler");
            Console.WriteLine();

            // Set
            var setDto = user.Builder()
                .MapTo<UserDto>()
                .Set(t => t.FullName, s => $"{s.FirstName} {s.LastName}")
                .Create();
            Console.WriteLine($"  Set → {setDto.FullName}");

            // SetIf
            var setIfDto = user.Builder()
                .MapTo<UserDto>()
                .SetIf(t => t.Age, s => s.Age > 0, s => s.Age)
                .Create();
            Console.WriteLine($"  SetIf → Age: {setIfDto.Age}");

            // Existing object mapping
            var existing = new UserDto { FirstName = "Eski", LastName = "Veri" };
            user.FastMapTo(existing);
            Console.WriteLine($"  Existing → {existing.FirstName} {existing.LastName}");

            // TypeConverter (int -> string)
            MapperExtensions.AddTypeConverter<int, string>(n => n.ToString());
            var report = user.FastMapTo<ReportDto>();
            Console.WriteLine($"  TypeConverter → Age: {report.Age}");

            // Custom mapping (FirstName -> DisplayName)
            MapperExtensions.ClearAllCaches();
            // Clear sonrası type converter'ı yeniden ekle
            MapperExtensions.AddTypeConverter<int, string>(n => n.ToString());
            MapperExtensions.AddCustomMapping<User, ReportDto>(
                "FirstName", "DisplayName",
                src => $"{((User)src).FirstName} {((User)src).LastName}"
            );
            var report2 = user.FastMapTo<ReportDto>();
            Console.WriteLine($"  Custom → {report2.DisplayName}");

            // Temizlik
            MapperExtensions.ClearAllCustomMappings();

            Console.WriteLine();
            
            // Performans
            Console.WriteLine("  ────────────────────────────────────────");
            Console.WriteLine();
            Console.WriteLine("  Performans");
            Console.WriteLine();
            
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)
                _ = user.FastMapTo<UserDto>();
            watch.Stop();
            
            Console.WriteLine($"  100,000 mapping → {watch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Ortalama → {(watch.ElapsedMilliseconds / 100000.0):F4}ms");
            Console.WriteLine();
            
            // Kapanış
            Console.WriteLine("  ────────────────────────────────────────");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("       Basit. Hızlı. Güçlü.");
            Console.WriteLine();
            Console.WriteLine("       github.com/mennan/fast-mapper");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
    
    public class User
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
    }
    
    public class UserDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string FullName { get; set; } = "";
    }

    public class ReportDto
    {
        public string DisplayName { get; set; } = "";
        public string Age { get; set; } = ""; // int -> string converter
    }
}
