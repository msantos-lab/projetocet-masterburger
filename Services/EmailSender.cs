using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;

namespace MasterBurger.Services {
  public class EmailSender : IEmailSender {
    public async Task SendEmailAsync(string toEmail, string subject, string body) {
      var emailMessage = new MimeMessage();
      emailMessage.From.Add(new MailboxAddress("MasterBurger", "monica.santos.24244@formandos.cinel.pt"));
      emailMessage.To.Add(new MailboxAddress("", toEmail));
      emailMessage.Subject = subject;

      var bodyBuilder = new BodyBuilder {
        HtmlBody = body
      };
      emailMessage.Body = bodyBuilder.ToMessageBody();

      using (var client = new SmtpClient()) {
        await client.ConnectAsync("smtp.office365.com", 587, false);
        await client.AuthenticateAsync("monica.santos.24244@formandos.cinel.pt", "301013Fm");
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
      }
    }
  }
}
