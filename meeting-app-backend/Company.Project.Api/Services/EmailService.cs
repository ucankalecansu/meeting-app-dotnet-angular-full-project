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

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var subject = "Meeting App'e Hoş Geldiniz";
        var body = $@"
            <h2>Hoş Geldiniz {name}!</h2>
            <p>Meeting App'e kaydolduğunuz için teşekkür ederiz. Artık toplantı yönetimi çok daha kolay olacak.</p>
            <p>İyi çalışmalar dileriz!</p>
            <p>Meeting App Ekibi</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendMeetingNotificationAsync(string email, string name, string meetingTitle, DateTime startDate, DateTime endDate)
    {
        var subject = $"Toplantı Bildirimi: {meetingTitle}";
        var body = $@"
            <h2>Toplantı Bildirimi</h2>
            <p>Sayın {name},</p>
            <p>Aşağıdaki toplantı bilgilerinizi size hatırlatmak isteriz:</p>
            <ul>
                <li><strong>Toplantı Adı:</strong> {meetingTitle}</li>
                <li><strong>Başlangıç:</strong> {startDate:dd.MM.yyyy HH:mm}</li>
                <li><strong>Bitiş:</strong> {endDate:dd.MM.yyyy HH:mm}</li>
            </ul>
            <p>İyi çalışmalar dileriz!</p>
            <p>Meeting App Ekibi</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to: {Email}", toEmail);
            throw;
        }
    }
}
