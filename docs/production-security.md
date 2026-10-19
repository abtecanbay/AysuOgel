# Üretim ortamı güvenlik kontrol listesi

Bu not, geliştirme ortamındaki User Secrets ile üretim ortamındaki sırları birbirinden ayırmak içindir. Gerçek parola veya bağlantı dizesi bu dosyaya yazılmamalıdır.

## 1. Yapılandırma

ASP.NET Core ortam değişkenleri çift alt çizgi (__) ile iç içe yapılandırma anahtarlarını temsil eder:

- ConnectionStrings__DefaultConnection: üretim SQL Server bağlantı dizesi
- Admin__Username: yönetici kullanıcı adı
- Admin__Password: güçlü ve benzersiz yönetici parolası

Bu değerleri barındırma sağlayıcısının gizli yapılandırma alanında veya sunucu ortam değişkenlerinde tanımlayın. appsettings.json, kaynak kodu, Git commit'leri, ekran görüntüleri veya loglara gerçek parola koymayın. Geliştirme için dotnet user-secrets kullanılabilir; User Secrets üretim için bir gizli bilgi kasası değildir.

## 2. SQL Server hesabı

Uygulama için sa hesabını kullanmayın. Veritabanı yöneticisi, üretimde uygulamaya özel bir login/user oluşturmalı ve yalnızca uygulamanın çalışması için gerekli izinleri vermelidir. Örnek aşağıdaki gibidir; parola yer tutucusunu gerçek ve güçlü bir parola ile değiştirin ve SQL dosyasını çalıştırdıktan sonra gizli bilgi içermeyecek şekilde saklayın:

```sql
USE [master];
GO
CREATE LOGIN [AysuOgelApp] WITH PASSWORD = 'REPLACE_WITH_A_LONG_RANDOM_SECRET';
GO
USE [AysuOgelDb];
GO
CREATE USER [AysuOgelApp] FOR LOGIN [AysuOgelApp];
GO
ALTER ROLE [db_datareader] ADD MEMBER [AysuOgelApp];
ALTER ROLE [db_datawriter] ADD MEMBER [AysuOgelApp];
GO
```

Bu hesapla EF Core migration çalıştırmak için ayrıca DDL/yetki gerekebilir. Üretimde migration'ları ayrı, kontrollü bir dağıtım hesabıyla çalıştırın; web uygulamasına kalıcı olarak db_owner vermeyin. Veritabanı adını kendi gerçek ortamınızla doğrulayın.

## 3. Döndürülmesi gereken sırlar

Daha önce geliştirme sohbetinde veya terminal çıktısında paylaşılmış olabilecek SQL Server sa parolasını güvenli kabul etmeyin. Sunucudaki parolayı değiştirin; eski parolayı kullanan bağlantıları güncelleyin. Git geçmişinde gerçek bir sır commit edildiyse yalnızca dosyayı silmek yeterli değildir: sırrı döndürün ve geçmişin temizlenmesi gerekip gerekmediğini değerlendirin.

## 4. Dağıtım ve veri güvenliği

- HTTPS zorunlu olsun; ters proxy arkasında çalışılıyorsa forwarded headers ayarını yalnızca güvenilen proxy'ler için yapılandırın.
- Yönetici hesabı için benzersiz ve güçlü parola kullanın. Giriş sınırı bu projede IP başına 15 dakikada 5 POST denemesidir; proxy/load balancer yapılandırması ve gerçek istemci IP'si ayrıca doğrulanmalıdır.
- Veritabanı yedeklerini şifreleyin, geri yükleme testi yapın ve erişimi sınırlandırın.
- Yüklenen görseller yalnızca JPG/PNG/WEBP, en fazla 5 MB olarak kabul edilir. Üretimde wwwroot/uploads klasörünü kalıcı depolama/yedekleme planına dahil edin ve dosya çalıştırma izinlerini kapalı tutun.
- KVKK metnindeki işletme unvanı, adres, başvuru kanalı, gerçek veri akışları ve saklama sürelerini işletmenin uygulamalarıyla doğrulamadan canlıya çıkmayın.

## 5. Canlıya geçmeden test

- dotnet build ile tüm solution'ı derleyin.
- Yanlış yönetici parolasıyla ardışık 6. POST isteğinin HTTP 429 aldığını doğrulayın; bekleme penceresi bitince tekrar deneyin.
- JPG, PNG ve WEBP yüklemelerini; sahte uzantılı dosyaları, SVG dosyalarını ve 5 MB üstünü test edin.
- İletişim formu, randevu oluşturma, mesajlar, admin CRUD işlemleri ve mobil görünümü test edin.
- Üretimde gerçek sırların Git'te bulunmadığını ve loglara yazılmadığını doğrulayın.