# İETT Yönetim Sistemi

## Proje hakkında

İETT Yönetim Sistemi; araç, hat, durak, sefer, şoför, sertifika, vatandaş şikâyeti ve denetim süreçlerini tek bir web uygulamasında yönetmek için geliştirilmiştir. Uygulama; yönetici, denetimci ve şoför kullanıcılarına rollerine göre farklı ekranlar sunar. Vatandaşlar ise oturum açmadan şikâyet oluşturabilir.

Proje katmanlı bir yapıya sahiptir. React istemcisi HTTP üzerinden ASP.NET Core Web API'ye istek gönderir. API katmanı JWT kimliğini ve rol yetkilerini doğruladıktan sonra işlemleri Business katmanına aktarır. Business katmanı iş kurallarını yürütür; DataAccess katmanı Entity Framework Core ve `IETTDbContext` aracılığıyla SQL Server'a erişir. Entity katmanı veritabanı varlıklarını, enum'ları ve istemciyle paylaşılan DTO'ları içerir. Şoföre iletilen şikâyetler, yeni sefer görevleri ve performans değerlendirmeleri JWT ile korunan SignalR hub üzerinden anlık olarak bildirilir.

## Kullanılan teknolojiler

- ASP.NET Core Web API, .NET 8
- Entity Framework Core 8 ve SQL Server sağlayıcısı
- SQL Server; yerel geliştirmede SQL Server Express LocalDB kullanılabilir
- React 19 ve React DOM
- Vite 8 ve `@vitejs/plugin-react`
- ASP.NET Core SignalR ve `@microsoft/signalr`
- JWT Bearer Authentication ve `System.IdentityModel.Tokens.Jwt`
- ASP.NET Core Identity `PasswordHasher<TUser>`
- Swagger/OpenAPI (`Swashbuckle.AspNetCore`)
- OXLint

Paketlerin kesin sürümleri ilgili `.csproj`, `package.json` ve `package-lock.json` dosyalarında yer alır.

## Proje yapısı

| Dizin | Sorumluluk |
| --- | --- |
| `IETT.Api` | Controller'lar, JWT ve CORS yapılandırması, bağımlılık enjeksiyonu, Swagger, statik sertifika dosyaları ve SignalR hub. |
| `IETT.Business` | Kimlik doğrulama, token üretimi ve araç, hat, sefer, şoför, denetimci, şikâyet ve inceleme iş kuralları. |
| `IETT.DataAccess` | EF Core DbContext, repository uygulamaları, migration'lar ve isteğe bağlı geliştirme veri betikleri. |
| `IETT.Entity` | Veritabanı varlıkları, DTO'lar, enum'lar ve ortak entity arayüzü. |
| `IETT.Core` | DataAccess tarafından kullanılan genel repository sözleşmesi. |
| `iett-admin-panel` | React/Vite tabanlı, rol bazlı web arayüzü ve API servisleri. |

## Roller ve yetkiler

### Admin

- Admin dashboard üzerinden araç, şoför, hat, sefer, şikâyet, denetim ve kullanıcı sayılarını görüntüler.
- Araçları listeler, ekler, günceller, siler ve durumlarını değiştirir.
- Hatları ve bunlara bağlı durakları görüntüler; hat ekler, günceller ve siler.
- Şoförleri ve şoför sertifikalarını görüntüler.
- Seferleri görüntüler.
- Vatandaş şikâyetlerini ve ayrıntılarını görüntüler.
- Tüm denetim kayıtlarını ve kullanıcı listesini görüntüler.

### Inspector / Denetimci

- Kendi dashboard özetini görüntüler.
- Kendi garaj kapsamındaki şoförleri, görevleri, sertifikaları ve incelemeleri görüntüler.
- Kendi garajındaki şoförlere sefer oluşturur; planlanmış seferleri günceller veya iptal eder.
- Araçları ve durumlarını görüntüler; araç durumunu değiştirebilir.
- Hatları ve durakları görüntüler.
- Bekleyen şoför sertifikalarını onaylar veya gerekçe belirterek reddeder.
- Kendisine atanmış şikâyet incelemelerinde karar verir.
- Şoför performans değerlendirmesi oluşturur ve kendi değerlendirme geçmişini görüntüler.

### Driver / Şoför

- Kendi dashboard özetini, seferlerini, kendisine iletilmiş şikâyetleri ve performans geçmişini görüntüler.
- Sertifikalarını görüntüler ve PDF dosyası olarak yeni sertifika yükler.
- Denetimci tarafından onaylanan bir şikâyet kendisine iletildiğinde, yeni sefer atandığında veya performans değerlendirmesi oluşturulduğunda SignalR üzerinden anlık bildirim alır.
- Sağ üstteki bildirim merkezinde okunmamış bildirim sayısını görür; tek bildirimi veya tüm bildirimleri okundu işaretleyebilir ve bildirime tıklayarak ilgili ekrana geçebilir.

### Citizen / Vatandaş

Kodda `Citizen` adlı kimlik doğrulamalı bir rol veya vatandaş dashboard'u yoktur. Vatandaş işlevi anonimdir: giriş ekranındaki bağlantıdan şikâyet türlerini görüntüler ve kapı numarası, hat kodu, olay zamanı, şikâyet türü ve açıklama ile şikâyet oluşturur. Başarılı kayıt sonunda takip kodu alır.

## Temel özellikler

- Araçların eklenmesi, güncellenmesi, silinmesi, listelenmesi ve durumlarının yönetilmesi.
- Hatların eklenmesi, güncellenmesi, silinmesi ve hat duraklarının sıralı olarak görüntülenmesi.
- Admin için sefer listeleme; denetimci için garaj kapsamındaki seferleri oluşturma, güncelleme ve iptal etme; şoför için kendi seferlerini görüntüleme.
- JWT ile giriş ve API/controller seviyesinde rol bazlı yetkilendirme.
- Şoför sertifikası PDF yükleme, listeleme, denetimci onayı ve gerekçeli ret akışı.
- Oturum gerektirmeyen vatandaş şikâyeti oluşturma ve takip kodu üretme.
- Kapı numarası ve hat kodunun tekil kayıtlarla eşleşmesi; olay zamanının aracın ilgili hattaki sefer aralığına düşmesi durumunda kesin sefer ve dolayısıyla şoför eşleştirmesi.
- Kesin sefer eşleşmesi bulunduğunda şikâyetin, şoförün garajındaki açık inceleme sayısı en az olan denetimciye otomatik atanması. Eşitlikte en küçük denetimci kimliği seçilir.
- Denetimcinin şikâyeti onaylama veya reddetme kararı. Onaylanan şikâyet şoförün şikâyet listesine aktarılır; reddedilen şikâyet aktarılmaz.
- Onaylanan şikâyetin ilgili şoföre SignalR ile anlık bildirilmesi.
- Yeni sefer görevi ve performans değerlendirmesinin ilgili şoföre SignalR ile anlık bildirilmesi.
- Driver bildirimlerinin kullanıcı bazlı olarak tarayıcı `localStorage` alanında en fazla 50 kayıtla tutulması; okunmamış sayaç ve okundu işaretleme desteği.
- Admin için şikâyet ve denetim kayıtlarının görüntülenmesi.
- Admin, denetimci ve şoföre özel dashboard ekranları.
- Denetimci tarafından şoför performans değerlendirmesi oluşturulması ve geçmişin ilgili rollerde görüntülenmesi.

Şikâyet için tek bir araç, tek bir hat veya tek bir sefer belirlenemezse şikâyet yine kaydedilir; ancak kesin sefer bulunmadığından otomatik inceleme ataması yapılmaz.

Bildirimler veritabanında kalıcı olarak saklanmaz. Yalnızca SignalR bağlantısı açıkken alınan bildirimler ilgili Driver kullanıcısının tarayıcısında tutulur; kullanıcı çevrimdışıyken kaçırılan event'ler sonradan yeniden oynatılmaz.

## Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js ve npm. Frontend'in Vite 8 kullanması nedeniyle güncel ve Vite 8 tarafından desteklenen bir Node.js sürümü gerekir.
- SQL Server Express LocalDB veya erişilebilir başka bir SQL Server kurulumu
- Migration komutları için `dotnet-ef` aracı
- İsteğe bağlı geliştirme SQL betiklerini çalıştırmak için SQLCMD destekli SSMS veya `sqlcmd`
- PowerShell; örnek kullanıcılar için güvenli parola hash'i üreten betik PowerShell ile yazılmıştır

## Kurulum ve çalıştırma

### 1. Repository'yi klonlama

```powershell
git clone <repository-url>
Set-Location IETTYonetimSistemi
```

### 2. Backend bağımlılıklarını yükleme

```powershell
dotnet restore .\IETTYonetimSistemi.sln
dotnet tool install --global dotnet-ef --version 8.*
```

`dotnet-ef` zaten kuruluysa ikinci komut yerine şu komut kullanılabilir:

```powershell
dotnet tool update --global dotnet-ef --version 8.*
```

### 3. Development ortamını ayarlama

`launchSettings.json` içindeki `http` ve `https` profilleri `ASPNETCORE_ENVIRONMENT=Development` değerini zaten tanımlar. Aynı terminalde açıkça ayarlamak için:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

### 4. Connection string ve JWT anahtarını yapılandırma

API, `ConnectionStrings:DefaultConnection` ve `Jwt:Key` anahtarlarını okur. Depodaki `appsettings.json` yerel bir makine adına bağlı örnek değerler içerdiği için kendi SQL Server örneğinizle değiştirilmelidir. Gizli değerleri dosyaya yazmadan ortam değişkenleriyle geçmek için örnek:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\MSSQLLocalDB;Database=IETTCodeFirstDB;Trusted_Connection=True;TrustServerCertificate=True;"
$env:Jwt__Key = "<uzun-ve-rastgele-development-jwt-anahtari>"
```

`Jwt:Issuer` ve `Jwt:Audience` değerleri `appsettings.json` içinde sırasıyla `IETT.Api` ve `IETT.React` olarak tanımlıdır.

> Mevcut `IETT.Api.csproj` içinde `UserSecretsId` bulunmadığından proje şu anda `dotnet user-secrets` için başlatılmış değildir. Ayrıca örnek kullanıcı parolasını okuyan bir User Secrets anahtarı kodda yoktur. Bu nedenle README, gerçekte kullanılmayan bir parola anahtarı veya `dotnet user-secrets set` komutu önermemektedir.

User Secrets tercih edilecekse önce proje kodunun bu yapılandırmayı okuyacak şekilde geliştirilmesi gerekir. Mevcut örnek kullanıcı kurulumu aşağıdaki parola hash'i ve SQLCMD akışını kullanır.

### 5. Migration'ları veritabanına uygulama

Connection string aynı terminalde ayarlıyken:

```powershell
dotnet ef database update --project .\IETT.DataAccess\IETT.DataAccess.csproj --startup-project .\IETT.Api\IETT.Api.csproj
```

Migration'lar şemayı ve dört şoför durumunu (`Working`, `On Leave`, `Off Duty`, `On Trip`) oluşturur. Rol, kullanıcı, garaj, operatör, hat, durak veya şikâyet türü örnekleri migration tarafından eklenmez.

### 6. Development seed işlemi

Kaynak kodda `SeedData__Enabled` ayarını okuyan otomatik bir seeder yoktur. Dolayısıyla aşağıdaki değişkeni ayarlamak mevcut uygulamada hiçbir işlem başlatmaz:

```powershell
$env:SeedData__Enabled = "true" # Mevcut kod tarafından okunmaz.
```

Depoda bunun yerine isteğe bağlı, idempotent SQL betikleri vardır. `SeedDevelopmentPersonnel.sql`; `Driver` ve `Inspector` rollerinin, gerekli garajların, `İETT`/`ÖHO` operatörlerinin ve betiğin beklediği çalışan şoför durumunun veritabanında önceden, tekil olarak bulunmasını ister. Taze migration yalnızca İngilizce şoför durumlarını eklediğinden bu ön koşullar ayrıca hazırlanmadıkça personel betiği çalışmaz.

Örnek kullanıcıların ortak development parolasını gizli girişle belirleyip ASP.NET Core Identity uyumlu hash üretmek için:

```powershell
.\IETT.DataAccess\Scripts\GenerateDevelopmentPasswordHash.ps1
```

Betik parolayı ve doğrulamasını etkileşimli olarak ister; standart çıktıya yalnızca hash'i yazar. Bu hash'i düz metin parolayı paylaşmadan SQLCMD değişkeni olarak kullanın:

```powershell
$samplePasswordHash = .\IETT.DataAccess\Scripts\GenerateDevelopmentPasswordHash.ps1
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "IETTCodeFirstDB" -E -v SamplePasswordHash="$samplePasswordHash" -i .\IETT.DataAccess\Scripts\SeedDevelopmentPersonnel.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "IETTCodeFirstDB" -E -i .\IETT.DataAccess\Scripts\SeedDevelopmentVehicles.sql
Remove-Variable samplePasswordHash
```

Farklı bir SQL Server kullanıyorsanız `-S`, kimlik doğrulama ve veritabanı parametrelerini kendi kurulumunuza göre değiştirin. Personel betiği geliştirme veritabanı içindir; Production ortamında çalıştırılmamalıdır. Araç betiği de yalnızca development/test için tasarlanmıştır.

### 7. Backend'i çalıştırma

HTTP profili, frontend'in varsayılan API adresiyle uyumludur:

```powershell
dotnet run --project .\IETT.Api\IETT.Api.csproj --launch-profile http
```

### 8. Frontend bağımlılıklarını yükleme

```powershell
Set-Location .\iett-admin-panel
npm install
```

Kilit dosyasına birebir kurulum için `npm install` yerine `npm ci` kullanılabilir.

### 9. Frontend environment ayarları

`.env.development` ve `.env.example` dosyalarında kullanılan gerçek anahtar şudur:

```dotenv
VITE_API_BASE_URL=http://localhost:5147
```

Yerel ve gizli bir override gerekiyorsa Git tarafından yok sayılan `iett-admin-panel/.env.local` dosyasını oluşturun. Değerin sonunda `/api` bulunmamalıdır; frontend bunu kendisi ekler. SignalR adresi de aynı taban adresten türetilir.

### 10. Frontend'i çalıştırma

```powershell
npm run dev
```

Vite varsayılan olarak `http://localhost:5173` adresinde açılır. API CORS ilkesi de yalnızca bu origin'e izin verir.

## Örnek geliştirme hesapları

`SeedDevelopmentPersonnel.sql` aşağıdaki kullanıcı adlarını oluşturur:

| Rol | Kullanıcı adları |
| --- | --- |
| Inspector | `elif.yildiz`, `selin.koc` |
| Driver | `zeynep.arslan`, `can.aydin`, `derya.sahin`, `burak.celik`, `ece.aksoy`, `onur.sen` |

Betik Admin veya Citizen kullanıcısı oluşturmaz. Parola bu README'de veya SQL dosyasında paylaşılmaz; geliştirici tarafından `GenerateDevelopmentPasswordHash.ps1` ile etkileşimli olarak belirlenir ve veritabanına yalnızca Identity uyumlu hash gönderilir. Mevcut kodda bu parola için bir User Secrets anahtarı tanımlı değildir.

## API ve frontend adresleri

`IETT.Api/Properties/launchSettings.json`, frontend environment dosyaları ve `apiConfig.js` ile doğrulanan varsayılan adresler:

| Bileşen | Adres |
| --- | --- |
| API (HTTP profili) | `http://localhost:5147` |
| API (HTTPS profili) | `https://localhost:7034` ve `http://localhost:5147` |
| Swagger (HTTP profili, yalnızca Development) | `http://localhost:5147/swagger` |
| Swagger (HTTPS profili, yalnızca Development) | `https://localhost:7034/swagger` |
| Vite geliştirme sunucusu | `http://localhost:5173` |
| SignalR hub yolu | `/hubs/notifications` |
| Varsayılan SignalR hub adresi | `http://localhost:5147/hubs/notifications` |

IIS Express profili ayrıca `http://localhost:18584` ve `https://localhost:44325` adreslerini tanımlar. Frontend varsayılan olarak HTTP proje profilini kullanır.

## Güvenlik notları

- Gerçek parola, JWT imzalama anahtarı veya hassas connection string repository'ye eklenmemelidir.
- Frontend'e aktarılacak gizli olmayan yerel değerler için Git tarafından yok sayılan `.env.local` kullanılabilir. `VITE_` önekli değerlerin tarayıcı paketine dahil edildiği ve gizli sayılamayacağı unutulmamalıdır.
- Backend sırları ortam değişkenleriyle verilmelidir. User Secrets kullanılacaksa API projesi önce `UserSecretsId` ve ilgili configuration anahtarlarıyla açıkça yapılandırılmalıdır.
- Depodaki `appsettings.json` içindeki geliştirme JWT anahtarı Production sırrı olarak kullanılmamalıdır.
- `SeedDevelopmentPersonnel.sql` ve `SeedDevelopmentVehicles.sql` uygulama başlangıcında otomatik çalışmaz. `SeedData__Enabled` mevcut kodda desteklenmez.
- Development veri betikleri Production ortamında çalıştırılmamalıdır. Uygulamada Production'da devreye giren otomatik bir development seeder bulunmaz.
- Swagger yalnızca `ASPNETCORE_ENVIRONMENT=Development` olduğunda etkinleşir.
- Yüklenen sertifikalar PDF ile sınırlandırılır ve `IETT.Api/wwwroot/uploads/driver-certificates` altında sunulur; Production kurulumu bu dizinin erişim ve saklama politikasını ayrıca yönetmelidir.

## Durdurma ve yeniden çalıştırma

Çalışan backend veya frontend terminalinde `Ctrl+C` ile süreci durdurun.

Repository kökünden backend'i yeniden başlatma:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "<connection-string>"
$env:Jwt__Key = "<jwt-anahtari>"
dotnet run --project .\IETT.Api\IETT.Api.csproj --launch-profile http
```

Ayrı bir terminalde frontend'i yeniden başlatma:

```powershell
Set-Location .\iett-admin-panel
npm run dev
```
