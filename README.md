# Interview Genie Projesi Kurulum Rehberi

Bu rehber, Interview Genie projesinin nasıl kurulacağını ve çalıştırılacağını adım adım açıklar. Proje, Backend ve Frontend olmak üzere iki ana bileşenden oluşur.

**Backend:** .NET Core 8 ile geliştirilen katmanlı mimariye sahip bir API projesi.  
**Frontend:** Angular 16 ile geliştirilen bir web arayüzü.

## Projeyi İndirme

Proje GitHub üzerinden indirilebilir:

```bash
git clone https://github.com/onurbahtiyar/Interview-Genie.git
```

İndirdiğinizde iki ana klasör göreceksiniz:
- Backend
- Frontend

## Gereksinimler

### Genel Gereksinimler
- **Git:** Projeyi klonlamak için gerekli.
- **Bir metin editörü veya IDE:** Örneğin, Visual Studio Code, Visual Studio veya JetBrains Rider.

### Backend Gereksinimleri
- **.NET Core 8 SDK:** Projeyi çalıştırmak için gerekli.
- **Entity Framework Core:** Migrasyonlar için gerekli.

### Frontend Gereksinimleri
- **Node.js ve NPM:** Angular ve paketlerin yönetimi için gerekli.
- **Angular CLI:** Angular uygulamasını çalıştırmak için.

## Backend Kurulumu

### 1. appsettings.json Dosyasını Düzenleyin

`Backend/Backend.API` dizininde bulunan `appsettings.json` dosyasını kendi yapılandırmanıza göre düzenleyin. Örnek yapılandırma:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=SUNUCU_ADRESİ;Port=PORT_NUMARASI;Database=VERİTABANI_ADI;Username=KULLANICI_ADI;Password=ŞİFRE"
  },
  "JwtSettings": {
    "Secret": "256Bit Gizli Anahtar",
    "Issuer": "GenieApp",
    "Audience": "GenieAppUsers",
    "ExpirationMinutes": 60
  },
  "AESSettings": {
    "Key": "Örnek Anahtar: OfJl0Cgao9Vl2wBvwihGnTssDclvgy7T",
    "IV": "Örnek IV: NkE7glBiglYElJKH"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedOrigins": [
    "http://localhost:4200"
  ],
  "Gemini": {
    "ApiKey": "Google Gemini API Anahtarı"
  }
}
```

> Not: Tüm alanları kendi bilgilerinizle güncelleyin.

### 2. Veritabanı Migrasyonlarını Uygulayın

Terminalde veya Paket Yöneticisi Konsolu'nda aşağıdaki komutları çalıştırın:

#### İlk Migrasyonu Oluşturun:
```bash
cd Backend
dotnet ef migrations add InitialCreate --project Backend.Infrastructure --startup-project Backend.API
```

#### Veritabanını Güncelleyin:
```bash
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.API
```

### 3. Backend Projesini Çalıştırın
```bash
dotnet run --project Backend.API
```

Backend API artık çalışır durumda olacaktır.

## Frontend Kurulumu

### 1. Gereksinimleri Kurun
- **Node.js ve NPM:** Node.js resmi sitesinden indirip kurun.
- **Angular CLI:** Terminalde aşağıdaki komutu çalıştırın:
```bash
npm install -g @angular/cli
```

### 2. Bağımlılıkları Yükleyin
```bash
cd Frontend
npm install
```

### 3. Ortam Dosyalarını Düzenleyin
`src/environments/` dizininde bulunan `environment.ts` ve `environment.prod.ts` dosyalarını düzenleyin.

#### environment.ts (Geliştirme Ortamı):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api', // Backend API URL'nizi buraya girin
};
```

#### environment.prod.ts (Üretim Ortamı):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.sizinalanadiniz.com/api', // Üretim Backend API URL'nizi buraya girin
};
```

### 4. Frontend Projesini Çalıştırın
```bash
ng serve
```

Uygulama varsayılan olarak http://localhost:4200 adresinde çalışacaktır.

## Proje Yapısı

### Backend
Backend projesi aşağıdaki katmanlardan oluşur:
- **API:** HTTP isteklerini alır ve yanıtlar.
- **Application:** İş mantığını yönetir.
- **Common:** Ortak yardımcı sınıflar içerir.
- **Configuration:** Yapılandırma ayarlarını yönetir.
- **Domain:** İş modellerini ve kurallarını içerir.
- **Infrastructure:** Veri erişimi ve entegrasyonları sağlar.
- **Repository:** Veri işlemlerini soyutlar.
- **Security:** Güvenlik ve kimlik doğrulamayı yönetir.

### Frontend
Frontend projesi katmanlı bir yapıya sahiptir:
- **core:** Guards, interceptors, services gibi temel bileşenler.
- **features:** Uygulamanın ana özelliklerini barındırır.
- **models:** Veri modelleri.
- **shared:** Ortak kullanılan bileşenler ve modüller.

## Önemli Notlar

- **Tailwind CSS Kullanımı:** Frontend projesinde Tailwind CSS kullanılmıştır. Stil dosyalarını derlemek için gerekli yapılandırmalar `angular.json` ve `tailwind.config.js` dosyalarında mevcuttur.
- **Güvenlik:** `appsettings.json` ve `environment` dosyalarındaki hassas bilgileri güvende tutun.
- **Bağımlılıklar:** Hem backend hem de frontend için gerekli tüm paketlerin yüklü olduğundan emin olun.
