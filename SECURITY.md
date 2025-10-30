# Security Policy

## 🔒 Güvenlik Politikası

JetMapper'ın güvenliğini ciddiye alıyoruz. Bu dokümanda, güvenlik açıkları nasıl raporlanır ve nasıl ele alınır açıklanmaktadır.

## 📋 Desteklenen Versiyonlar

Şu anda aşağıdaki versiyonlar için güvenlik güncellemeleri sağlanmaktadır:

| Versiyon | Destekleniyor          |
| -------- | --------------------- |
| 1.2.x    | :white_check_mark:    |
| 1.1.x    | :white_check_mark:    |
| 1.0.x    | :x:                   |
| < 1.0    | :x:                   |

## 🚨 Güvenlik Açığı Bildirme

### Lütfen güvenlik açıklarını public issue olarak bildirmeyin!

Güvenlik açığı bulduysanız, lütfen aşağıdaki adımları izleyin:

### 1. Özel Bildirim

Güvenlik sorunlarını **mennansevim@gmail.com** adresine gönderin.

E-postanızda şunları ekleyin:

- Açığın detaylı açıklaması
- Sorunu yeniden üretme adımları
- Etkilenen versiyonlar
- Potansiyel etki analizi
- Varsa, önerilen düzeltme

### 2. Beklenen Yanıt Süresi

- **24 saat içinde**: İlk yanıt ve onay
- **48 saat içinde**: Sorunun değerlendirilmesi
- **7 gün içinde**: Düzeltme için plan ve tahmini süre
- **30 gün içinde**: Düzeltme yayınlanması (kritik durumlar için daha erken)

### 3. Koordineli Açıklama

Güvenlik açığı düzeltildikten sonra:

1. Düzeltme yayınlanır
2. Güvenlik danışmanlığı yayınlanır
3. Katkınız (istediğiniz takdirde) kabul edilir

## 🛡️ Güvenlik En İyi Pratikleri

JetMapper'ı kullanırken aşağıdaki güvenlik pratiklerini öneririz:

### 1. Hassas Veri İşleme

```csharp
// Hassas özellikleri ignore edin
var dto = user.Builder()
    .MapTo<UserDto>()
    .Ignore(d => d.Password)
    .Ignore(d => d.CreditCard)
    .Ignore(d => d.SSN)
    .Create();
```

### 2. Tip Güvenliği

```csharp
// Mapping öncesi validasyon yapın
ValidationResult validation = MappingValidator.ValidateMapping<Source, Destination>();
if (!validation.IsValid)
{
    // Hataları ele alın
}
```

### 3. Güncel Kalın

```bash
# En son güvenlik güncellemelerini alın
dotnet add package JetMapper
```

### 4. Dependency Kontrolü

Projenizdeki tüm dependency'leri düzenli olarak güncelleyin:

```bash
dotnet list package --outdated
```

## 🔍 Güvenlik Kontrol Listesi

Üretim ortamına geçmeden önce:

- [ ] Hassas datalar mapping'den exclude edilmiş mi?
- [ ] Type validation yapılıyor mu?
- [ ] En son JetMapper versiyonu kullanılıyor mu?
- [ ] Custom converter'lar input validation yapıyor mu?
- [ ] Lifecycle hook'ları güvenli kod içeriyor mu?

## 📝 Güvenlik Güncellemeleri

Güvenlik güncellemeleri hakkında bilgi almak için:

1. Bu repository'yi "Watch" edin
2. [GitHub Security Advisories](https://github.com/mennansevim/jet-mapper/security/advisories) sayfasını takip edin
3. [Release Notes](https://github.com/mennansevim/jet-mapper/releases) kontrol edin

## 🏆 Hall of Fame

Güvenlik açıklarını sorumlu bir şekilde bildiren katkıda bulunanlar (izinleriyle):

- *Henüz bildirim yok*

## 📞 İletişim

Güvenlik soruları için:

- **Email**: mennansevim@gmail.com
- **Opsiyonel**: PGP Key kullanarak şifreli email gönderebilirsiniz

## ⚖️ Politika Değişiklikleri

Bu güvenlik politikası zaman zaman güncellenebilir. Önemli değişiklikler için bildiri yapılacaktır.

---

**Son Güncelleme**: Ekim 2025

Güvenliğimizi artırmaya yardımcı olduğunuz için teşekkür ederiz! 🙏

