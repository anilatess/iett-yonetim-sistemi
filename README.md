# 🚌 İETT Yönetim Sistemi

İETT Yönetim Sistemi; araç, hat, sefer, şoför, denetim, vatandaş şikâyeti, performans ve sertifika süreçlerinin rol bazlı olarak yönetilebildiği full-stack bir web uygulamasıdır.

Proje; gerçek hayattaki kurumsal bir toplu taşıma yönetim sistemi senaryosu temel alınarak geliştirilmiştir.

---

## 🌐 Canlı Demo

**Frontend**

https://iett-yonetim-sistemi-dun.vercel.app

**API / Swagger**

https://iett-api.onrender.com/swagger

> **Not:** Backend Render Free üzerinde çalışmaktadır. Uzun süre kullanılmadığında servis uyku moduna geçebilir. Bu nedenle ilk açılış birkaç saniye daha uzun sürebilir.

---

## 🎯 Projenin Amacı

Bu projenin temel amacı; farklı kullanıcı rollerinin yalnızca kendi yetkileri doğrultusunda işlem yapabildiği, gerçek hayattaki toplu taşıma operasyonlarını modelleyen bir yönetim sistemi geliştirmektir.

Projede özellikle şu konular üzerinde çalışılmıştır:

- Rol bazlı yetkilendirme
- Kullanıcı yönetimi
- Araç yönetimi
- Hat ve durak yönetimi
- Sefer yönetimi
- Garaj bazlı veri erişimi
- Vatandaş şikâyet sistemi
- Şikâyet inceleme ve yönlendirme akışı
- Şoför performans değerlendirmesi
- Sertifika yönetimi
- SignalR ile gerçek zamanlı bildirimler
- Hangfire ile arka plan görevleri
- JWT Authentication
- Azure SQL production veritabanı

---

## 🧱 Kullanılan Teknolojiler

### Backend

- C#
- ASP.NET Core Web API
- .NET 8
- Entity Framework Core
- JWT Authentication
- Role Based Authorization
- SignalR
- Hangfire
- Swagger / OpenAPI

### Frontend

- React
- JavaScript
- Vite
- CSS
- REST API
- SignalR Client

### Database

- Microsoft SQL Server
- Entity Framework Core Code First
- Azure SQL Database

### Deployment

- Vercel
- Render
- Azure SQL
- Docker

---

## 🏗️ Mimari

Backend tarafında katmanlı mimari kullanılmıştır.

```text
IETT.Api
   ↓
IETT.Business
   ↓
IETT.DataAccess
   ↓
IETT.Entity
   ↓
SQL Server / Azure SQL
```

### Katmanlar

**IETT.Api**

Controller'lar, JWT yapılandırması, SignalR Hub, Hangfire ve API configuration işlemlerini içerir.

**IETT.Business**

İş kurallarının ve servislerin bulunduğu katmandır.

**IETT.DataAccess**

Entity Framework Core, repository yapısı ve veritabanı erişim işlemlerini içerir.

**IETT.Entity**

Entity modellerini ve DTO'ları içerir.

**IETT.Core**

Projede ortak kullanılan altyapı bileşenlerini içerir.

---

# 👥 Roller

## Admin

Admin sistem genelindeki yönetim ekranlarına erişebilir.

- Kullanıcıları görüntüleyebilir.
- Araçları yönetebilir.
- Hatları ve durakları inceleyebilir.
- Tüm seferleri görüntüleyebilir.
- Denetim süreçlerini inceleyebilir.
- Sistem genelindeki operasyonları takip edebilir.

---

## Denetimci

Denetimci yalnızca kendi garajı ve sorumluluk alanıyla ilişkili veriler üzerinde işlem yapabilir.

- Kendi garajındaki şoförleri görüntüleyebilir.
- Kendi garajındaki seferleri yönetebilir.
- Yeni sefer oluşturabilir.
- Sefer bilgilerini güncelleyebilir.
- Sefer iptal edebilir.
- Kendisine atanmış şikâyetleri inceleyebilir.
- Şikâyeti onaylayabilir veya reddedebilir.
- Şoför performans değerlendirmesi oluşturabilir.

---

## Şoför

Şoför yalnızca kendi hesabıyla ilişkili verilere erişebilir.

- Kendi seferlerini görüntüleyebilir.
- Kendisine iletilen şikâyetleri görebilir.
- Performans değerlendirmelerini inceleyebilir.
- Sertifikalarını görüntüleyebilir.
- Sertifika yükleyebilir.
- SignalR üzerinden gerçek zamanlı bildirim alabilir.

---

## Vatandaş

Vatandaş için kullanıcı hesabı gerekmez.

- Şikâyet oluşturabilir.
- Şikâyet takip kodu alabilir.
- Takip kodu ile şikâyetin durumunu sorgulayabilir.

---

# 🧪 Demo / Test Rehberi

Projeyi incelemek isteyen bir geliştirici veya işe alım uzmanının aşağıdaki sırayı takip etmesi önerilir.

## 🔐 Demo Hesapları

### Admin

```text
Kullanıcı Adı: admin
Şifre: 123456
```

### Denetimci

```text
Kullanıcı Adı: murat.denetimci
Şifre: 123456
```

### Şoför

```text
Kullanıcı Adı: ali.kaya
Şifre: 123456
```



---

# ✅ Test Senaryosu 1 — Admin

Canlı uygulamayı açın:

https://iett-yonetim-sistemi-dun.vercel.app

Admin hesabıyla giriş yapın.

### Kontrol Edilebilecek Alanlar

1. Dashboard ekranını görüntüleyin.
2. Kullanıcılar ekranını açın.
3. Kullanıcıların rollerini inceleyin.
4. Araçlar ekranını açın.
5. Araç durumlarını inceleyin.
6. Hatlar ekranını açın.
7. Bir hattın detayına girip durakları görüntüleyin.
8. Seferler ekranını inceleyin.
9. Denetimler ekranından sistemdeki incelemeleri görüntüleyin.

Bu senaryo sistemin genel yönetim tarafını göstermektedir.

---

# ✅ Test Senaryosu 2 — Denetimci

Admin hesabından çıkış yapıp:

```text
Kullanıcı Adı: murat.denetimci
Şifre: 123456
```

ile giriş yapın.

### Test Adımları

1. Denetimci Dashboard ekranını açın.
2. Bağlı olduğu garaj bilgisini inceleyin.
3. Kendi garajındaki şoförleri görüntüleyin.
4. Seferler ekranına girin.
5. Garajdaki seferleri görüntüleyin.
6. Uygun olması halinde yeni bir sefer oluşturun.
7. Mevcut bir seferi güncelleyin.
8. Şikâyet İncelemeleri ekranını açın.
9. Açık bir şikâyeti inceleyin.
10. Şikâyeti onaylayın veya reddedin.
11. Performans Değerlendirme ekranından bir şoför için değerlendirme oluşturun.

Bu senaryo garaj bazlı yetkilendirmeyi ve operasyon yönetimini göstermektedir.

---

# ✅ Test Senaryosu 3 — Şoför

Denetimci hesabından çıkış yapıp:

```text
Kullanıcı Adı: ali.kaya
Şifre: 123456
```

ile giriş yapın.

### Test Adımları

1. Şoför Dashboard ekranını görüntüleyin.
2. Günlük seferleri inceleyin.
3. Seferlerim ekranını açın.
4. Şikâyetlerim ekranını kontrol edin.
5. Performansım ekranını açın.
6. Denetimci tarafından verilen değerlendirmeleri inceleyin.
7. Sertifikalarım ekranını açın.
8. Sertifika yükleme alanını inceleyin.
9. Sağ üstteki bildirim merkezini kontrol edin.

Bu senaryo şoförün yalnızca kendi verilerine erişebildiğini göstermektedir.

---

# ✅ Test Senaryosu 4 — Vatandaş Şikâyeti

Bu işlem için giriş yapılması gerekmez.

Ana ekrandan:

**Şikâyet Oluştur**

seçeneğine girin.

Vatandaş aşağıdaki bilgileri girer:

- Araç kapı numarası
- Hat kodu
- Şikâyet türü
- Olay tarih ve saati
- Açıklama

Backend bu bilgiler üzerinden ilgili seferi bulur.

Temel eşleştirme:

```text
Kapı Numarası
      +
Hat Kodu
      +
Olay Tarih / Saat
      ↓
İlgili Sefer
```

Sefer üzerinden sistem otomatik olarak:

```text
Araç
  ↓
Sefer
  ↓
Hat
  ↓
Şoför
  ↓
Garaj
  ↓
Denetimci
```

ilişkilerini çözümler.

Şikâyet uygun denetimciye otomatik olarak atanır.

---

# ✅ Test Senaryosu 5 — Şikâyet Takibi

Şikâyet oluşturulduktan sonra sistem benzersiz bir takip kodu üretir.

Ana sayfadan:

**Şikâyet Takibi**

ekranını açın.

Takip kodunu girin.

Şikâyet sonuçlanmadıysa:

```text
Süreç devam ediyor
```

gösterilir.

Denetimci incelemeyi tamamladıktan sonra vatandaş nihai sonucu görebilir.

---

# ✅ Test Senaryosu 6 — Uçtan Uca Akış

Projenin en önemli iş akışı aşağıdaki şekildedir:

```text
Vatandaş
   ↓
Şikâyet Oluşturur
   ↓
İlgili Sefer Bulunur
   ↓
Araç + Hat + Şoför Eşleştirilir
   ↓
Garaj Bulunur
   ↓
Denetimci Atanır
   ↓
Denetimci İnceleme Yapar
   ↓
Onay / Red
```

### Şikâyet Onaylanırsa

```text
Denetimci
   ↓
Şikâyeti Onaylar
   ↓
Şoföre İletilir
   ↓
Şoför Şikâyetlerim Ekranında Görür
   ↓
SignalR Bildirimi Alır
```

### Şikâyet Reddedilirse

```text
Denetimci
   ↓
Şikâyeti Reddeder
   ↓
İnceleme Kapatılır
   ↓
Vatandaş Takip Ekranından Sonucu Görür
```

---

## 🔔 SignalR

Projede gerçek zamanlı bildirimler için SignalR kullanılmaktadır.

SignalR Hub:

```text
/hubs/notifications
```

Başlıca bildirim senaryoları:

- Şikâyetin şoföre iletilmesi
- Yeni sefer atanması
- Yeni performans değerlendirmesi oluşturulması

Kullanılan event'ler:

```text
ComplaintForwarded
TripAssigned
PerformanceEvaluated
```

SignalR bağlantıları JWT ile doğrulanmaktadır.

---

## ⏱️ Hangfire

Zamanlanmış arka plan işlemleri için Hangfire kullanılmaktadır.

Recurring job:

```text
investigation-deadline-reminders
```

Bu job açık incelemeleri belirli aralıklarla kontrol eder ve deadline süreçlerini yönetir.

Hangfire storage olarak SQL Server kullanılmaktadır.

---

## 🔐 Authentication & Authorization

Projede JWT tabanlı authentication kullanılmaktadır.

Login endpoint:

```http
POST /api/Auth/login
```

Başarılı giriş sonrasında kullanıcıya JWT token üretilir.

Temel roller:

```text
Admin
Inspector
Driver
```

Frontend üzerindeki rol kontrollerine ek olarak backend endpoint'leri de role-based authorization ile korunmaktadır.

---

## 🗄️ Veritabanı

Production ortamında Azure SQL Database kullanılmaktadır.

Başlıca tablolar:

- Users
- Roles
- Drivers
- Inspectors
- Garages
- Operators
- Vehicles
- VehicleStatuses
- BusRoutes
- BusStops
- BusRouteStops
- Trips
- TripStatuses
- Complaints
- ComplaintTypes
- ComplaintStatuses
- Investigations
- DriverPerformance
- DriverCertificates

---

## ☁️ Production Mimarisi

```text
                 Kullanıcı
                     │
                     ▼
              Vercel Frontend
                React + Vite
                     │
                     ▼
              Render Backend
            ASP.NET Core Web API
                 .NET 8
                /     \
               ▼       ▼
         Azure SQL   SignalR
               │
               ▼
           Hangfire
```

### Frontend

```text
https://iett-yonetim-sistemi-dun.vercel.app
```

### Backend

```text
https://iett-api.onrender.com
```

### Swagger

```text
https://iett-api.onrender.com/swagger
```

---

## 🐳 Docker

Backend Render üzerinde Docker container olarak çalışmaktadır.

Repository kökünde bulunan:

```text
Dockerfile
```

ASP.NET Core API'nin build ve runtime işlemleri için kullanılmaktadır.

---

## 📁 Proje Yapısı

```text
iett-yonetim-sistemi
│
├── IETT.Api
├── IETT.Business
├── IETT.Core
├── IETT.DataAccess
├── IETT.Entity
├── iett-admin-panel
├── Dockerfile
├── IETTYonetimSistemi.sln
└── README.md
```

---

## 💻 Local Development

### Repository'yi Klonlama

```bash
git clone https://github.com/anilatess/iett-yonetim-sistemi.git
cd iett-yonetim-sistemi
```

### Backend

```bash
dotnet restore
dotnet run --project IETT.Api
```

Backend:

```text
http://localhost:5147
```

Swagger:

```text
http://localhost:5147/swagger
```

### Frontend

```bash
cd iett-admin-panel
npm install
npm run dev
```

Frontend:

```text
http://localhost:5173
```

---

## 🌍 Environment Variables

Production secret bilgileri repository içerisinde tutulmamaktadır.

### Backend

```text
ConnectionStrings__DefaultConnection
```

### Frontend

```text
VITE_API_BASE_URL
```

Production API adresi:

```text
https://iett-api.onrender.com
```

---

## 🚀 Öne Çıkan Teknik Özellikler

- Full-stack web uygulaması
- Katmanlı backend mimarisi
- Repository Pattern
- Entity Framework Core
- Code First
- JWT Authentication
- Role Based Authorization
- Garaj bazlı veri izolasyonu
- React rol bazlı kullanıcı arayüzü
- REST API
- Swagger / OpenAPI
- SignalR gerçek zamanlı bildirimler
- Hangfire background jobs
- Vatandaş şikâyet sistemi
- Şikâyet takip kodu
- Otomatik şoför / sefer eşleştirme
- Otomatik denetimci atama
- Performans değerlendirme sistemi
- Sertifika yönetimi
- Docker
- Render
- Vercel
- Azure SQL
- Production CORS yapılandırması

---


## ℹ️ Demo Hakkında

Bu proje eğitim, staj ve portföy amacıyla geliştirilmiştir.

Canlı ortamda bulunan kayıtlar demo/test verileridir.

Render Free kullanıldığı için backend uzun süre kullanılmadığında uyku moduna geçebilir. İlk isteğin yanıtlanması bu nedenle normalden biraz daha uzun sürebilir.

---

## 🔗 Bağlantılar

**Canlı Uygulama**

https://iett-yonetim-sistemi-dun.vercel.app

**Swagger / API**

https://iett-api.onrender.com/swagger

**GitHub**

https://github.com/anilatess/iett-yonetim-sistemi

---

## 👨‍💻 Geliştirici

**İsmail Anıl Ateş**

Computer Programming

GitHub:

https://github.com/anilatess

---

> Bu proje İETT'nin resmi üretim sistemi değildir. Eğitim, staj ve portföy amacıyla geliştirilmiş bağımsız bir yazılım projesidir.