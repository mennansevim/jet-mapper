# FastMapper Kullanım Örnekleri (Flight & Reservation)

Bu doküman, uçuş ve rezervasyon senaryoları üzerinden sade ve kısa açıklamalı örnekler içerir. Tüm değişkenler İngilizcedir. 3 seviyeli iç içe entity örneği dahildir.

## 📋 İçindekiler

1. Alan Modeli (3 seviye)
2. Temel Mapping
3. Fluent API
4. Koşullu Mapping
5. Type Converter
6. Async Mapping
7. Diff Mapping
8. Snapshot & Restore
9. Mapping Validator
10. Partial Merge
11. Diagnostic & Profiling
12. Gerçek Dünya: Flight Offer → Reservation Quote

---

## 🧩 Alan Modeli (3 seviye)
Kısa açıklama: Reservation → Itinerary → FlightSegment (3 seviye) ve bağlı varlıklar.

```csharp
// Level 3: FlightSegment (uçağın bir bacağı)
public class FlightSegment
{
    public string FlightNumber { get; set; }
    public string MarketingCarrier { get; set; }
    public string OperatingCarrier { get; set; }
    public string DepartureAirport { get; set; }
    public string ArrivalAirport { get; set; }
    public DateTime DepartureTimeUtc { get; set; }
    public DateTime ArrivalTimeUtc { get; set; }
    public string AircraftType { get; set; }
}

// Level 2: Itinerary (çok bacaklı güzergâh)
public class Itinerary
{
    public string ItineraryId { get; set; }
    public List<FlightSegment> Segments { get; set; } = new();
}

// Level 1: Reservation (ana kök)
public class Reservation
{
    public string ReservationCode { get; set; }
    public Passenger Passenger { get; set; }
    public Itinerary Itinerary { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }
    public ReservationStatus Status { get; set; }
}

public class Passenger
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string LoyaltyTier { get; set; }
}

public enum ReservationStatus { Created, Ticketed, Cancelled }

// DTO'lar (listeleme/response için)
public class ReservationDto
{
    public string ReservationCode { get; set; }
    public string PassengerFullName { get; set; }
    public string Route { get; set; }
    public string TotalPrice { get; set; }
    public string StatusText { get; set; }
}

public class FlightSegmentDto
{
    public string FlightNumber { get; set; }
    public string Route { get; set; }
    public string Duration { get; set; }
}
```

---

## 🚀 Temel Mapping
Kısa açıklama: Basit nesneden DTO'ya hızlı eşleme.

```csharp
// FlightSegment → FlightSegmentDto
var segment = new FlightSegment
{
    FlightNumber = "FM123",
    MarketingCarrier = "FM",
    OperatingCarrier = "FM",
    DepartureAirport = "IST",
    ArrivalAirport = "LHR",
    DepartureTimeUtc = new DateTime(2025, 5, 1, 8, 0, 0, DateTimeKind.Utc),
    ArrivalTimeUtc = new DateTime(2025, 5, 1, 11, 0, 0, DateTimeKind.Utc),
    AircraftType = "A321"
};

var dto = segment.Map()
    .Map<FlightSegmentDto>(d => d.Route, s => $"{s.DepartureAirport}-{s.ArrivalAirport}")
    .Map<FlightSegmentDto>(d => d.Duration, s => (s.ArrivalTimeUtc - s.DepartureTimeUtc).ToString())
    .To<FlightSegmentDto>();
```

Mevcut nesneye mapping:
```csharp
var existing = new FlightSegmentDto();
segment.FastMapTo(existing);
```

Koleksiyon mapping:
```csharp
var segments = new List<FlightSegment> { segment };
var segmentDtos = segments.Cast<object>().FastMapToList<FlightSegmentDto>();
```

---

## 🔗 Fluent API
Kısa açıklama: Türev alanları tek zincirde üretin.

```csharp
// Reservation → ReservationDto
var reservation = new Reservation
{
    ReservationCode = "ABC123",
    Passenger = new Passenger { FirstName = "John", LastName = "Doe" },
    Itinerary = new Itinerary
    {
        ItineraryId = "ITI-1",
        Segments =
        {
            new FlightSegment { DepartureAirport = "IST", ArrivalAirport = "LHR", DepartureTimeUtc = DateTime.UtcNow, ArrivalTimeUtc = DateTime.UtcNow.AddHours(4), FlightNumber = "FM123" }
        }
    },
    TotalAmount = 245.90m,
    Currency = "USD",
    Status = ReservationStatus.Ticketed
};

var result = reservation.Map()
    .Map<ReservationDto>(d => d.PassengerFullName, r => $"{r.Passenger.FirstName} {r.Passenger.LastName}")
    .Map<ReservationDto>(d => d.Route, r =>
        r.Itinerary?.Segments?.Count > 0 ?
        $"{r.Itinerary.Segments.First().DepartureAirport}-{r.Itinerary.Segments.Last().ArrivalAirport}" : "")
    .Map<ReservationDto>(d => d.TotalPrice, r => $"{r.TotalAmount:F2} {r.Currency}")
    .Map<ReservationDto>(d => d.StatusText, r => r.Status.ToString())
    .To<ReservationDto>();
```

Hook'lar ile:
```csharp
var resultWithHooks = reservation.Map()
    .BeforeMap((source, target) => Console.WriteLine($"Start mapping {source.ReservationCode}"))
    .AfterMap((source, target) => Console.WriteLine($"Done mapping {source.ReservationCode}"))
    .To<ReservationDto>();
```

---

## 🎯 Koşullu Mapping
Kısa açıklama: Duruma göre hedef alanı doldur.

```csharp
var result2 = reservation.Map()
    .MapIf<ReservationDto>(d => d.StatusText,
        r => r.Status == ReservationStatus.Ticketed,
        r => "Ticket issued")
    .MapIf<ReservationDto>(d => d.StatusText,
        r => r.Status == ReservationStatus.Created,
        r => "Pending ticket")
    .MapIf<ReservationDto>(d => d.StatusText,
        r => r.Status == ReservationStatus.Cancelled,
        r => "Cancelled")
    .To<ReservationDto>();
```

Hedef property kontrolü ile önceliklendirme (MapIfElse):
```csharp
public class ReservationViewDto
{
    public string PrimaryInfo { get; set; }
    public string? SecondaryInfo { get; set; }
}

var view = reservation.Map()
    .MapIfElse<ReservationViewDto>(d => d.PrimaryInfo,
        (d => d.SecondaryInfo, r => r.Passenger.Email),      // if SecondaryInfo != null (örnek amaçlı)
        (d => d.PrimaryInfo, r => r.ReservationCode),        // else if PrimaryInfo != null (örnek)
        (d => d.PrimaryInfo, r => r.Passenger.FirstName))    // fallback
    .To<ReservationViewDto>();
```

---

## 🔄 Type Converter
Kısa açıklama: Sık kullanılan dönüşümler (DateTime, TimeSpan, Price).

```csharp
MapperExtensions.AddTypeConverter<DateTime, string>(dt => dt.ToString("yyyy-MM-dd HH:mm"));
MapperExtensions.AddTypeConverter<TimeSpan, string>(ts => ts.ToString());
MapperExtensions.AddTypeConverter<decimal, string>(amount => $"{amount:F2}");
```

JSON/String → Enum ve Enum Listesi:
```csharp
// Enum tanımı
public enum StatusEnum { Success, Pending, Failed }

// 1) String → Enum (case-insensitive)
public class ApiStatusSource { public string Status { get; set; } }    // "success"
public class ApiStatusTarget { public StatusEnum Status { get; set; } }

var s1 = new ApiStatusSource { Status = "success" };
var t1 = s1.FastMapTo<ApiStatusTarget>();         // t1.Status == StatusEnum.Success

// 2) CSV/JSON string → List<Enum>
public class ApiStatusListSource { public string Statuses { get; set; } } // "success,pending" veya "[\"success\",\"failed\"]"
public class ApiStatusListTarget { public List<StatusEnum> Statuses { get; set; } }

var s2 = new ApiStatusListSource { Statuses = "success,pending" };
var t2 = s2.FastMapTo<ApiStatusListTarget>();     // t2.Statuses: Success, Pending

// 3) JArray → Enum[] (Newtonsoft.Json.Linq.JArray)
public class ApiStatusArraySource { public Newtonsoft.Json.Linq.JArray Statuses { get; set; } }
public class ApiStatusArrayTarget { public StatusEnum[] Statuses { get; set; } }

var s3 = new ApiStatusArraySource { Statuses = Newtonsoft.Json.Linq.JArray.Parse("[\"success\",\"failed\"]") };
var t3 = s3.FastMapTo<ApiStatusArrayTarget>();    // t3.Statuses: Success, Failed
```

---

## ⚡ Async Mapping
Kısa açıklama: Büyük rezervasyon listelerini eşleyin, ilerleme alın.

```csharp
var reservations = GetBigReservationList();
var progress = new Progress<AsyncMapper.MappingProgress>(p =>
{
    Console.WriteLine($"Progress: {p.Percentage:F1}% ({p.ProcessedCount}/{p.TotalCount})");
});

var reservationDtos = await AsyncMapper.MapAsync<Reservation, ReservationDto>(reservations, progress, maxConcurrency: 4);
```

---

## 🔍 Diff Mapping
Kısa açıklama: İki rezervasyon arasındaki farklılıkları bulun.

```csharp
var original = new Reservation { ReservationCode = "ABC123", TotalAmount = 200m, Currency = "USD" };
var updated  = new Reservation { ReservationCode = "ABC123", TotalAmount = 230m, Currency = "USD" };

var diff = DiffMapper.FindDifferences(original, updated);
if (diff.HasDifferences)
{
    Console.WriteLine($"Differences: {diff.Differences.Count}");
}
```

---

## 💾 Snapshot & Restore
Kısa açıklama: Undo/redo ve geçici durum saklama için.

```csharp
var snapshot = AsyncMapper.CreateSnapshot(reservation);
var restored = AsyncMapper.RestoreFromSnapshot<Reservation>(snapshot.Id);

// Yönetim
var all = AsyncMapper.ListSnapshots();
AsyncMapper.DeleteSnapshot(snapshot.Id);
AsyncMapper.CleanupSnapshots(TimeSpan.FromDays(7));
```

---

## ✅ Mapping Validator
Kısa açıklama: Mapping yapılarını önceden doğrula.

```csharp
var validation = MappingValidator.ValidateMapping<Reservation, ReservationDto>();
Console.WriteLine($"IsValid: {validation.IsValid}, Errors: {validation.Errors.Count}, Warnings: {validation.Warnings.Count}");
```

---

## 🔄 Partial Merge
Kısa açıklama: Yalnızca belirli alanları güncelle.

```csharp
var target = new Reservation { ReservationCode = "ABC123", Passenger = new Passenger { Email = "old@mail.com" } };
var source = new Reservation { Passenger = new Passenger { Email = "new@mail.com" } };

var mergeResult = MergeMapper.PartialMerge(target, source, "Passenger.Email");
```

---

## 📊 Diagnostic & Profiling
Kısa açıklama: Performansı ölç ve raporla.

```csharp
var profile = DiagnosticMapper.StartPerformanceProfile("ReservationMapping");
for (int i = 0; i < 1000; i++)
{
    _ = reservation.FastMapTo<ReservationDto>();
}
var perf = DiagnosticMapper.EndPerformanceProfile("ReservationMapping");
Console.WriteLine($"Avg: {perf.AverageMappingTime}, Total: {perf.TotalTime}");
```

---

## 🌍 Gerçek Dünya: Flight Offer → Reservation Quote
Kısa açıklama: Dış API'den gelen flight offer'ları kullanıcıya gösterilecek quote DTO'larına eşle.

```csharp
public class FlightOffer
{
    public string OfferId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public Itinerary Itinerary { get; set; }
}

public class ReservationQuoteDto
{
    public string OfferId { get; set; }
    public string Route { get; set; }
    public string PriceText { get; set; }
}

var offer = new FlightOffer
{
    OfferId = "OF-001",
    Price = 199.99m,
    Currency = "USD",
    Itinerary = reservation.Itinerary
};

var quote = offer.Map()
    .Map<ReservationQuoteDto>(d => d.Route, o =>
        o.Itinerary.Segments.Count > 0 ?
        $"{o.Itinerary.Segments.First().DepartureAirport}-{o.Itinerary.Segments.Last().ArrivalAirport}" : "")
    .Map<ReservationQuoteDto>(d => d.PriceText, o => $"{o.Price:F2} {o.Currency}")
    .To<ReservationQuoteDto>();
```

---

## 🎯 İpuçları
Kısa açıklama: Kısa öneriler.

```csharp
// 1) App start: cache warm-up
_ = new Reservation().FastMapTo<ReservationDto>();

// 2) Error handling
try { _ = reservation.FastMapTo<ReservationDto>(); }
catch (MappingException ex) { /* fallback */ }

// 3) Pre-validate mapping
var v = MappingValidator.ValidateMapping<Reservation, ReservationDto>();
if (!v.IsValid) throw new InvalidOperationException("Invalid mapping configuration");
```

Bu örnekler uçuş/rezervasyon odaklıdır, kısa ve pratik tutulmuştur. 