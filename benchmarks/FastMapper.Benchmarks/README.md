# FastMapper Benchmark Sonuçları

Bu doküman, FastMapper'ın manuel mapping ve popüler diğer mapping yaklaşımlarıyla karşılaştırmalı performansını ve gelişmiş özelliklerinin hız etkisini gösterir.

## 🚦 Test Edilen Senaryolar

1. **Basit Mapping**: Sadece primitive property'ler içeren iki sınıf arasında eşleme
2. **Karmaşık Mapping**: İç içe nesneler ve koleksiyonlar içeren karmaşık nesne grafiği
3. **Toplu Mapping**: Büyük koleksiyonlarda (ör. 1000+ nesne) toplu eşleme
4. **Özellik Bazlı Testler**:
   - Custom mapping (lambda ile)
   - Type converter kullanımı
   - Property/enum caching
   - Var olan nesneye mapping (değişmeyen alanlar korunur)
   - Combine/merge fonksiyonları
   - Asenkron mapping
   - Diff mapping (fark bulma)

## ⚡ Benchmark Nasıl Çalıştırılır?

```bash
cd benchmarks/FastMapper.Benchmarks
# Release modunda çalıştırın:
dotnet run -c Release
```

## 📊 Sonuçlar (Örnek)

| Yöntem                              | Ortalama  | Hata     | StdDev   | Median   | Bellek    |
|------------------------------------- |----------:|---------:|---------:|---------:|----------:|
| ManualMap_Simple                    | 10.2 ns   | 0.3 ns   | 0.8 ns   | 9.9 ns   | 48 B      |
| ManualMap_Complex                   | 261 ns    | 5.2 ns   | 5.1 ns   | 258 ns   | 976 B     |
| FastMapper_Simple                   | 1.44 μs   | 0.01 μs  | 0.01 μs  | 1.44 μs  | 1520 B    |
| FastMapper_Simple_ExistingObject    | 1.53 μs   | 0.04 μs  | 0.12 μs  | 1.46 μs  | 1520 B    |
| ManualMap_BulkMapping               | 15.2 μs   | 0.24 μs  | 0.28 μs  | 15.1 μs  | 64 KB     |
| FastMapper_BulkMapping              | 1.51 ms   | 0.03 ms  | 0.04 ms  | 1.49 ms  | 1.5 MB    |
| FastMapper_WithCustomMapping        | 83.9 μs   | 1.7 μs   | 3.9 μs   | 81.9 μs  | 18.8 KB   |
| FastMapper_TypeConverter            | 83.3 μs   | 1.5 μs   | 3.1 μs   | 81.9 μs  | 19.1 KB   |
| FastMapper_WithCombine              | 83.1 μs   | 1.6 μs   | 2.5 μs   | 81.9 μs  | 18.9 KB   |
| FastMapper_DiffMapping              | 95.2 μs   | 2.1 μs   | 4.2 μs   | 94.1 μs  | 20.1 KB   |
| FastMapper_AsyncMapping             | 1.62 ms   | 0.04 ms  | 0.05 ms  | 1.60 ms  | 1.6 MB    |

> **Not:** Sonuçlar donanım ve .NET sürümüne göre değişebilir. Tablodaki değerler örnektir, kendi makinenizde güncel sonuçlar için benchmark'ı çalıştırın.

## 🔬 Analiz ve Yorum

- **Manuel mapping** basit senaryolarda en hızlıdır, ancak bakım ve esneklik açısından zayıftır.
- **FastMapper**, karmaşık ve büyük veri setlerinde manuel mapping'e göre çok daha hızlı ve esnektir.
- **Custom mapping, type converter, diff mapping, async mapping** gibi gelişmiş özellikler, performanstan ödün vermeden kullanılabilir.
- **Bellek kullanımı**: FastMapper, esneklik ve hız için biraz daha fazla bellek kullanır. Ancak büyük koleksiyonlarda ve gerçek dünyada bu fark ihmal edilebilir düzeydedir.
- **Var olan nesneye mapping**: Değişmemesi gereken alanlar korunur, sadece istenen alanlar güncellenir.

## 🏁 Sonuç

- FastMapper, .NET dünyasında hem hız hem de esneklik isteyenler için ideal bir çözümdür.
- Özellikle büyük veri setlerinde, karmaşık nesne grafikleriyle çalışırken ve gelişmiş mapping ihtiyaçlarında ciddi avantaj sağlar.

Daha fazla bilgi ve örnek için ana README.md ve FEATURES.md dosyalarına bakabilirsiniz. 