using System.Net;
using System.Net.Mail;

public class EmailService
{

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
        var port = int.Parse(Environment.GetEnvironmentVariable("Port")!);
        var senderEmail = Environment.GetEnvironmentVariable("SenderEmail");
        var senderName = Environment.GetEnvironmentVariable("SenderName");
        var username = Environment.GetEnvironmentVariable("Username");
        var password = Environment.GetEnvironmentVariable("Password");
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
