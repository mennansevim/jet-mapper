# Contributing to JetMapper

🎉 Öncelikle, JetMapper projesine katkıda bulunmayı düşündüğünüz için teşekkür ederiz!

## Nasıl Katkıda Bulunabilirsiniz?

### 🐛 Bug Raporlama

Bug bulduysanız, lütfen bir [GitHub Issue](https://github.com/mennansevim/jet-mapper/issues) açın ve aşağıdaki bilgileri ekleyin:

- Bug'ın açık bir tanımı
- Yeniden üretme adımları
- Beklenen davranış
- Gerçekleşen davranış
- .NET versiyonu ve işletim sistemi
- Mümkünse kod örnekleri

### ✨ Özellik Önerme

Yeni bir özellik öneriniz varsa:

1. Önce [GitHub Issues](https://github.com/mennansevim/jet-mapper/issues) sayfasında benzer bir öneri olup olmadığını kontrol edin
2. Yoksa yeni bir issue açın ve özelliği detaylıca açıklayın
3. Özelliğin neden faydalı olduğunu ve nasıl kullanılacağını belirtin

### 🔧 Pull Request Süreci

1. **Fork** edin ve yeni bir branch oluşturun:
   ```bash
   git checkout -b feature/amazing-feature
   ```

2. **Değişikliklerinizi yapın:**
   - Kod stiline uygun yazın (.editorconfig'e uyun)
   - Gerekli testleri ekleyin
   - Kodunuzun tüm testleri geçtiğinden emin olun

3. **Commit** edin:
   ```bash
   git commit -m "feat: amazing new feature"
   ```

4. **Push** edin:
   ```bash
   git push origin feature/amazing-feature
   ```

5. **Pull Request** açın ve:
   - Değişikliklerinizi açıklayın
   - İlgili issue'ları referans verin
   - Testlerin geçtiğini doğrulayın

## 📝 Kod Standartları

- **Naming Conventions**: PascalCase (class, method), camelCase (variables, parameters)
- **Code Style**: .editorconfig dosyasına uyun
- **Comments**: Karmaşık logic'ler için açıklayıcı yorumlar ekleyin
- **Tests**: Yeni özellikler için unit test yazın
- **Documentation**: Public API'lar için XML dokümantasyonu ekleyin

## 🧪 Testler

Testleri çalıştırmak için:

```bash
dotnet test
```

Benchmark'ları çalıştırmak için:

```bash
cd benchmarks/JetMapper.Benchmarks
dotnet run -c Release
```

## 📚 Commit Mesajları

Semantic commit mesajları kullanın:

- `feat:` - Yeni özellik
- `fix:` - Bug düzeltmesi
- `docs:` - Dokümantasyon değişikliği
- `style:` - Kod formatı (logic değişikliği yok)
- `refactor:` - Kod refactoring
- `test:` - Test ekleme/düzeltme
- `chore:` - Diğer değişiklikler

Örnek:
```
feat: add async batch mapping support
fix: null reference exception in FluentMapper
docs: update README with new examples
```

## 🔍 Code Review Süreci

Pull request'iniz gözden geçirilecek ve:

- Kod kalitesi kontrolü yapılacak
- Testler çalıştırılacak
- Performance etkileri değerlendirilecek
- Dokümantasyon kontrolü yapılacak

## ❓ Sorularınız mı var?

[GitHub Discussions](https://github.com/mennansevim/jet-mapper/discussions) bölümünü kullanabilir veya bir issue açabilirsiniz.

## 📄 License

Bu projeye katkıda bulunarak, katkılarınızın [MIT License](LICENSE) altında lisanslanacağını kabul etmiş olursunuz.

---

Katkılarınız için teşekkür ederiz! 🚀

**Made with ❤️ for the .NET community**

