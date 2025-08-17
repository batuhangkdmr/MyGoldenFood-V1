# MyGoldenFood - Windows Hosting Deployment Guide

## 🚀 Deployment Requirements

- **Windows Server 2016** or later
- **IIS 10** or later
- **ASP.NET Core Hosting Bundle 3.1.x**
- **.NET Core 3.1 Runtime**
- **SQL Server 2019** or later

## 📋 Installation Steps

### 1. Install Prerequisites

1. **ASP.NET Core Hosting Bundle 3.1.x** yükleyin
   - [Microsoft Download Center](https://dotnet.microsoft.com/download/dotnet/3.1)
   - Hosting Bundle'ı seçin

2. **.NET Core 3.1 Runtime** yükleyin
   - Windows x64 Runtime

3. **IIS** yükleyin
   - ASP.NET Core Module ile birlikte

### 2. Deploy Application

1. **Tüm dosyaları** IIS website klasörüne kopyalayın
2. **Application Pool**'u .NET Core 3.1 için yapılandırın
3. **Application Pool identity**'sini database erişimi olan kullanıcıya ayarlayın

### 3. Configure IIS

1. **Yeni website** veya application oluşturun
2. **Physical path**'i deployment klasörüne ayarlayın
3. **Domain bindings**'leri yapılandırın
4. **Application Pool**'u "No Managed Code" yapın

### 4. Database Configuration

- **SQL Server connection string**'i `appsettings.json`'da doğru olduğundan emin olun
- **Database user permissions**'ları kontrol edin
- **Database connectivity**'yi test edin

### 5. File Permissions

**IIS_IUSRS**'a şu klasörler için read/write access verin:
- Application folder
- `logs` folder
- `wwwroot` folder

### 6. SSL Certificate

- **SSL sertifikası** yükleyin
- **HTTPS binding**'i IIS'de yapılandırın
- Gerekirse `web.config`'i güncelleyin

## 📁 Configuration Files

- **`web.config`** - IIS yapılandırması ve optimizasyonları
- **`appsettings.json`** - Uygulama ayarları
- **`MyGoldenFood.runtimeconfig.json`** - Runtime yapılandırması

## 📊 Monitoring

- **Loglar** `logs\stdout` klasörüne yazılır
- **Application pool recycling**'i izleyin
- **Windows Event Logs**'u kontrol edin

## ⚡ Performance Optimizations

- **HTTP compression** aktif
- **Static file caching** yapılandırıldı
- **In-process hosting model** kullanılıyor
- **Optimized garbage collection** ayarları

## 🔧 Troubleshooting

1. **`logs\stdout`** klasöründeki logları kontrol edin
2. **Application Pool** yapılandırmasını doğrulayın
3. **Database connectivity**'yi test edin
4. **File permissions**'ları kontrol edin
5. **Windows Event Logs**'u inceleyin

## 📦 Included Files

✅ **Tüm gerekli DLL'ler** (Entity Framework, SignalR, Cloudinary, MailKit)  
✅ **Multi-language resource dosyaları** (8 dil)  
✅ **Views ve wwwroot klasörleri**  
✅ **Optimized web.config**  
✅ **Production appsettings.json**  
✅ **Runtime optimizations**  
✅ **Logging yapılandırması**  
✅ **Compression ve caching**  
✅ **HTTPS redirect**  

## 🌐 Multi-Language Support

Proje şu dilleri destekler:
- 🇹🇷 Türkçe (tr)
- 🇺🇸 İngilizce (en)
- 🇩🇪 Almanca (de)
- 🇫🇷 Fransızca (fr)
- 🇷🇺 Rusça (ru)
- 🇯🇵 Japonca (ja)
- 🇰🇷 Korece (ko)
- 🇸🇦 Arapça (ar)

## 📞 Support

Teknik destek için geliştirme ekibiyle iletişime geçin.

---

**Proje artık Windows hosting için tamamen optimize edilmiş durumda!** 🎉 