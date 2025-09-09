using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var port = int.Parse(_config["EmailSettings:Port"] ?? "0");
        var senderEmail = _config["EmailSettings:SenderEmail"] ?? "null";
        var senderName = _config["EmailSettings:SenderName"];
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];
        var mail = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        using var smtp = new SmtpClient(smtpServer)
        {
            Port = port,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        await smtp.SendMailAsync(mail);
    }
}
