using System.Net;
using System.Net.Mail;

namespace Company.Project.Api.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Hoş Geldiniz!";
        var body = $@"<html>
            <body>
                <h2>Merhaba {userName},</h2>
                <p>Toplantı yönetim platformumuza hoş geldiniz!</p>
                <p>Kaydınız başarıyla tamamlanmıştır.</p>
                <p>Herhangi bir sorunuz olursa bizimle iletişime geçebilirsiniz.</p>
                <p>Teşekkürler,<br>Toplantı Yönetim Ekibi</p>
            </body>
        </html>";

        await SendEmailAsync(toEmail, subject, body);
    }
    
    public async Task SendMeetingNotificationAsync(string email, string name, string meetingTitle, DateTime startDate, DateTime endDate)
    {
        var subject = $"Toplantı Bildirimi: {meetingTitle}";
        var body = $@"<html>
            <body>
                <h2>Merhaba {name},</h2>
                <p>Aşağıdaki toplantı bilgileriniz için bir hatırlatma:</p>
                <div style='padding: 15px; background-color: #f5f5f5; border-left: 5px solid #2196F3; margin-bottom: 15px;'>
                    <h3>{meetingTitle}</h3>
                    <p><strong>Başlangıç:</strong> {startDate:dd MMMM yyyy HH:mm}</p>
                    <p><strong>Bitiş:</strong> {endDate:dd MMMM yyyy HH:mm}</p>
                </div>
                <p>Lütfen toplantıya zamanında katılmayı unutmayınız.</p>
                <p>Teşekkürler,<br>Toplantı Yönetim Ekibi</p>
            </body>
        </html>";

        await SendEmailAsync(email, subject, body);
    }
    
    public async Task SendMeetingUpdateNotificationAsync(string email, string name, string meetingTitle, DateTime startDate, DateTime endDate)
    {
        var subject = $"Toplantı Güncellemesi: {meetingTitle}";
        var body = $@"<html>
            <body>
                <h2>Merhaba {name},</h2>
                <p><strong>Katılımcısı olduğunuz toplantı güncellendi!</strong></p>
                <div style='padding: 15px; background-color: #f5f5f5; border-left: 5px solid #FFA726; margin-bottom: 15px;'>
                    <h3>{meetingTitle}</h3>
                    <p><strong>Yeni Başlangıç:</strong> {startDate:dd MMMM yyyy HH:mm}</p>
                    <p><strong>Yeni Bitiş:</strong> {endDate:dd MMMM yyyy HH:mm}</p>
                </div>
                <p>Lütfen takviminizdeki ilgili kayıtları güncelleyiniz.</p>
                <p>Teşekkürler,<br>Toplantı Yönetim Ekibi</p>
            </body>
        </html>";

        await SendEmailAsync(email, subject, body);
    }
    
    public async Task SendMeetingCancelNotificationAsync(string email, string name, string meetingTitle)
    {
        var subject = $"Toplantı İptal Bildirimi: {meetingTitle}";
        var body = $@"<html>
            <body>
                <h2>Merhaba {name},</h2>
                <p><strong>Aşağıdaki toplantı iptal edilmiştir:</strong></p>
                <div style='padding: 15px; background-color: #f5f5f5; border-left: 5px solid #F44336; margin-bottom: 15px;'>
                    <h3>{meetingTitle}</h3>
                </div>
                <p>Lütfen takviminizdeki ilgili kayıtları güncelleyiniz.</p>
                <p>Teşekkürler,<br>Toplantı Yönetim Ekibi</p>
            </body>
        </html>";

        await SendEmailAsync(email, subject, body);
    }
    
    public async Task SendMeetingDeleteNotificationAsync(string email, string name, string meetingTitle)
    {
        var subject = $"Toplantı Silindi: {meetingTitle}";
        var body = $@"<html>
            <body>
                <h2>Merhaba {name},</h2>
                <p><strong>Aşağıdaki toplantı sistemden tamamen silinmiştir:</strong></p>
                <div style='padding: 15px; background-color: #f5f5f5; border-left: 5px solid #9C27B0; margin-bottom: 15px;'>
                    <h3>{meetingTitle}</h3>
                </div>
                <p>Lütfen takviminizdeki ilgili kayıtları güncelleyiniz.</p>
                <p>Teşekkürler,<br>Toplantı Yönetim Ekibi</p>
            </body>
        </html>";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            // Konfigürasyondan değerleri al ve log'a yaz
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            _logger.LogInformation("E-posta gönderimi başlatılıyor. SMTP Server: {Server}, Port: {Port}, Username: {Username}, SenderEmail: {SenderEmail}", 
                smtpServer, smtpPort, smtpUsername, senderEmail);

            // Gmail kullanırken SMTP istemci ayarları
            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 20000 // 20 saniye
            };

            // Mesaj oluştur
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            _logger.LogInformation("Mesaj oluşturuldu - Konu: {Subject}, Alıcı: {ToEmail}", subject, toEmail);

            // Alıcıyı ekle ve gönder
            message.To.Add(toEmail);
            await client.SendMailAsync(message);
            _logger.LogInformation("Email başarıyla gönderildi. Alıcı: {Email}", toEmail);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP hatası: {StatusCode}, {Message}. Alıcı: {Email}", smtpEx.StatusCode, smtpEx.Message, toEmail);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderimi sırasında beklenmedik hata: {Message}. Alıcı: {Email}", ex.Message, toEmail);
            throw;
        }
    }
}
