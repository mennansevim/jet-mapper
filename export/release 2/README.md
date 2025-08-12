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
