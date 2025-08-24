# Meeting App .NET & Angular Project

Bu proje, toplantı yönetimi için geliştirilmiş bir .NET Core backend ve Angular frontend uygulamasıdır. Uygulama, kullanıcı kaydı, toplantı oluşturma, güncelleme ve iptal etme işlemleri için otomatik e-posta bildirimleri gönderebilmektedir.

## Proje Yapısı

- `meeting-app-backend`: .NET Core API projesi (port 5000'de çalışır)
- `ekran goruntuleri`: Uygulama ekran görüntülerini içerir

## Backend Projesini Çalıştırma

Backend uygulaması Docker ile çalıştırılır. Aşağıdaki komutları sırasıyla çalıştırın:

```bash
# meeting-app-backend dizinine gidin
cd meeting-app-backend

# Docker container'larını başlatın
docker compose up -d

# .NET uygulamasını 5000 portunda çalıştırın
docker exec meeting-app-backend-dotnet-dev-1 bash -c "cd /app && dotnet run --project Company.Project.Api/Company.Project.Api.csproj --urls=\"http://0.0.0.0:5000\""
```

Yukarıdaki adımları tamamladığınızda, backend uygulaması http://localhost:5000 adresinde çalışacaktır.

## Ekran Görüntüleri

Uygulama e-posta bildirimleri için örnek ekran görüntüleri:

### Kayıt E-postası
![Kayıt E-postası](/ekran%20goruntuleri/register-mail.png)

### Toplantı Oluşturma E-postası
![Toplantı Oluşturma](/ekran%20goruntuleri/create-mail.png)

### Toplantı Güncelleme E-postası
![Toplantı Güncelleme](/ekran%20goruntuleri/update-mail.png)

### Toplantı İptal E-postası
![Toplantı İptal](/ekran%20goruntuleri/cancel-mail.png)
