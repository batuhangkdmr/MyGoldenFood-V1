# 🌿 MyGoldenFood - Frozen Food E-Commerce Platform

[![.NET Core](https://img.shields.io/badge/.NET%20Core-3.1-blue.svg)](https://dotnet.microsoft.com/download/dotnet-core/3.1)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-3.1.32-green.svg)](https://docs.microsoft.com/en-us/ef/)
[![Cloudinary](https://img.shields.io/badge/Cloudinary-1.20.0-orange.svg)](https://cloudinary.com/)
[![SignalR](https://img.shields.io/badge/SignalR-1.1.0-purple.svg)](https://docs.microsoft.com/en-us/aspnet/core/signalr/)

> **Doğanın Altın Değerindeki Lezzetlerini Keşfedin!** 🍓

MyGoldenFood, donuk gıda sektöründe hizmet veren modern bir e-ticaret platformudur. Berry grubu meyvelerinin üretimi, paketlenmesi ve satışı konusunda uzmanlaşmış bir firmanın dijital vitrinidir.

## 🚀 Özellikler

### 🌐 **Çok Dilli Destek**
- **9 Dil Desteği**: Türkçe, İngilizce, Rusça, Almanca, Fransızca, İspanyolca, Japonca, Korece, Arapça
- **Otomatik Çeviri**: DeepL API entegrasyonu ile gerçek zamanlı çeviri
- **JSON Tabanlı Lokalizasyon**: Hızlı ve esnek çeviri yönetimi

### 🛒 **E-Ticaret Özellikleri**
- **Ürün Kataloğu**: Berry grubu meyvelerinin detaylı listesi
- **Tarifler**: Ürünlerle ilgili yemek tarifleri
- **Faydalar**: Sağlık ve beslenme bilgileri
- **Blog Sistemi**: İçerik yönetimi ve SEO optimizasyonu

### 📱 **Responsive Tasarım**
- **Mobil Uyumlu**: Tüm cihazlarda mükemmel görünüm
- **Lazy Loading**: Hızlı sayfa yükleme
- **Cloudinary CDN**: Optimize edilmiş görsel yönetimi
- **Skeleton Loading**: Kullanıcı deneyimi iyileştirmeleri

### 🔧 **Teknik Özellikler**
- **Real-time Güncellemeler**: SignalR ile canlı veri senkronizasyonu
- **Admin Paneli**: Kolay içerik yönetimi
- **Mail Sistemi**: İletişim formu entegrasyonu
- **SEO Optimizasyonu**: XML sitemap ve meta tag yönetimi

## 🏗️ Teknoloji Stack

### **Backend**
- **.NET Core 3.1** - Ana framework
- **ASP.NET Core MVC** - Web uygulaması
- **Entity Framework Core 3.1.32** - ORM
- **SQL Server** - Veritabanı
- **SignalR 1.1.0** - Real-time iletişim

### **Frontend**
- **Bootstrap 4** - CSS framework
- **jQuery** - JavaScript kütüphanesi
- **Animate.css** - Animasyonlar
- **Font Awesome** - İkonlar

### **Harici Servisler**
- **Cloudinary** - Görsel yönetimi ve CDN
- **DeepL API** - Otomatik çeviri
- **SMTP** - E-posta gönderimi

### **Geliştirme Araçları**
- **Visual Studio 2019/2022**
- **SQL Server Management Studio**
- **Git** - Versiyon kontrolü

## 📦 Kurulum

### **Gereksinimler**
- .NET Core 3.1 SDK
- SQL Server 2016 veya üzeri
- Visual Studio 2019/2022 (önerilen)

### **Adım 1: Projeyi Klonlayın**
```bash
git clone https://github.com/yourusername/MyGoldenFood.git
cd MyGoldenFood
```

### **Adım 2: Bağımlılıkları Yükleyin**
```bash
dotnet restore
```

### **Adım 3: Veritabanını Yapılandırın**
1. `appsettings.json` dosyasında connection string'i güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=MyGoldenFood;Trusted_Connection=true;"
  }
}
```

2. Veritabanını oluşturun:
```bash
dotnet ef database update
```

### **Adım 4: Harici Servisleri Yapılandırın**
`appsettings.json` dosyasında aşağıdaki ayarları güncelleyin:

```json
{
  "CloudinarySettings": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": "587",
    "Username": "your_email@gmail.com",
    "Password": "your_app_password"
  },
  "DeepLTranslationSettings": {
    "ApiKey": "your_deepl_api_key"
  }
}
```

### **Adım 5: Uygulamayı Çalıştırın**
```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde çalışacaktır.

## 🗂️ Proje Yapısı

```
MyGoldenFood/
├── 📁 Controllers/           # MVC Controller'ları
│   ├── HomeController.cs     # Ana sayfa ve genel işlemler
│   ├── AdminController.cs    # Admin paneli
│   ├── ProductController.cs  # Ürün yönetimi
│   ├── TariflerController.cs # Tarif yönetimi
│   └── ...
├── 📁 Models/                # Veri modelleri
│   ├── Product.cs           # Ürün modeli
│   ├── Recipe.cs            # Tarif modeli
│   ├── Benefit.cs           # Fayda modeli
│   └── ...
├── 📁 Views/                 # Razor view'ları
│   ├── Home/                # Ana sayfa view'ları
│   ├── Admin/               # Admin paneli view'ları
│   └── Shared/              # Paylaşılan view'lar
├── 📁 Services/              # İş mantığı servisleri
│   ├── CloudinaryService.cs # Görsel yönetimi
│   ├── TranslationService.cs # Çeviri servisi
│   ├── MailService.cs       # E-posta servisi
│   └── ...
├── 📁 Hub/                   # SignalR Hub'ları
│   ├── ProductHub.cs        # Ürün güncellemeleri
│   ├── FaydalariHub.cs      # Fayda güncellemeleri
│   └── TariflerHub.cs       # Tarif güncellemeleri
├── 📁 ApplicationDbContext/  # Veritabanı bağlamı
│   └── AppDbContext.cs      # EF Core DbContext
├── 📁 Migrations/            # Veritabanı migrasyonları
├── 📁 wwwroot/              # Statik dosyalar
│   ├── css/                 # CSS dosyaları
│   ├── js/                  # JavaScript dosyaları
│   ├── images/              # Görsel dosyalar
│   ├── translations/        # Çeviri dosyaları
│   └── sitemap.xml          # SEO sitemap
└── 📄 Program.cs            # Uygulama giriş noktası
```

## 🌐 Çok Dilli Destek

### **Desteklenen Diller**
- 🇹🇷 **Türkçe** (Varsayılan)
- 🇺🇸 **İngilizce**
- 🇷🇺 **Rusça**
- 🇩🇪 **Almanca**
- 🇫🇷 **Fransızca**
- 🇪🇸 **İspanyolca**
- 🇯🇵 **Japonca**
- 🇰🇷 **Korece**
- 🇸🇦 **Arapça**

### **Çeviri Dosyaları**
Çeviriler `wwwroot/translations/` klasöründe JSON formatında saklanır:
- `tr.json` - Türkçe
- `en.json` - İngilizce
- `ru.json` - Rusça
- `de.json` - Almanca
- `fr.json` - Fransızca
- `es.json` - İspanyolca
- `ja.json` - Japonca
- `ko.json` - Korece
- `ar.json` - Arapça

### **Yeni Çeviri Ekleme**
1. `wwwroot/translations/` klasöründe yeni dil dosyası oluşturun
2. Mevcut JSON yapısını kopyalayın
3. Çevirileri güncelleyin
4. `LanguageController.cs`'de yeni dili ekleyin

## 🚀 Deployment

### **Self-Contained Publish**
```bash
dotnet publish --configuration Release --output "publish-final" --self-contained true --runtime win-x64
```

### **IIS Deployment**
1. Publish klasörünü IIS'e kopyalayın
2. `web.config` dosyasını yapılandırın
3. Application Pool'u .NET Core 3.1 olarak ayarlayın
4. Veritabanı bağlantısını kontrol edin

### **Docker Support** (Gelecek)
```dockerfile
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY publish-final/ /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "MyGoldenFood.dll"]
```

## 🔧 Yapılandırma

### **Cloudinary Ayarları**
```json
{
  "CloudinarySettings": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  }
}
```

### **E-posta Ayarları**
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": "587",
    "Username": "your_email@gmail.com",
    "Password": "your_app_password"
  }
}
```

### **DeepL Çeviri Ayarları**
```json
{
  "DeepLTranslationSettings": {
    "ApiKey": "your_deepl_api_key"
  }
}
```

## 📊 Performans Optimizasyonları

### **Görsel Optimizasyonu**
- **Cloudinary CDN**: Otomatik format dönüştürme (WebP)
- **Lazy Loading**: Sayfa yükleme hızını artırma
- **Skeleton Loading**: Kullanıcı deneyimi iyileştirme

### **Caching Stratejileri**
- **Memory Cache**: Sık kullanılan veriler için
- **Browser Cache**: Statik dosyalar için
- **Service Worker**: Offline deneyim

### **SEO Optimizasyonu**
- **XML Sitemap**: Arama motoru indeksleme
- **Meta Tags**: Sayfa başlıkları ve açıklamaları
- **Structured Data**: Zengin snippet'ler

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some AmazingFeature'`)
4. Branch'inizi push edin (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın.

## 📞 İletişim

- **Website**: [mygoldenfood.com](https://mygoldenfood.com)
- **E-posta**: info@mygoldenfood.com
- **Proje Sahibi**: My Golden Food Gıda San. ve Tic. A.Ş.

## 🙏 Teşekkürler

- [Cloudinary](https://cloudinary.com/) - Görsel yönetimi
- [DeepL](https://www.deepl.com/) - Otomatik çeviri
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Font Awesome](https://fontawesome.com/) - İkonlar

---

**🌿 Doğanın Altın Değerindeki Lezzetlerini Keşfedin!** 🍓

*Bu proje, donuk gıda sektöründe kaliteli ve sağlıklı ürünler sunmak amacıyla geliştirilmiştir.*
