using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class EmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var senderPassword = _config["EmailSettings:SenderPassword"];
        var senderLogin = _config["EmailSettings:SenderLogin"];

       // Console.WriteLine(senderLogin);

        using var client = new SmtpClient(smtpServer, smtpPort)
        {
            Credentials = new NetworkCredential(senderLogin, senderPassword), // Тут логин вместо email
            EnableSsl = false,
            UseDefaultCredentials = false
        };
     


        //using var client = new SmtpClient(smtpServer, smtpPort)
        //{
        //    DeliveryMethod = SmtpDeliveryMethod.Network,
        //    Credentials = new NetworkCredential(senderEmail, senderPassword),
        //    EnableSsl = false,

        //};

        var mailMessage = new MailMessage(senderEmail, email, subject, message)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
    }
}
