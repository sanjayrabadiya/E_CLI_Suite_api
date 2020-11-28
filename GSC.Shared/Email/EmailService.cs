using Serilog;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GSC.Shared.Email
{
    public class EmailService : IEmailService
    {
        public void SendMail(EmailMessage emailMessage)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var mail = new MailMessage();
                    var smtpServer = new SmtpClient(emailMessage.DomainName);

                    mail.From = new MailAddress(emailMessage.EmailFrom);

                    mail.To.Add(emailMessage.SendTo);
                    mail.Subject = emailMessage.Subject;
                    mail.Body = emailMessage.MessageBody;
                    mail.IsBodyHtml = emailMessage.IsBodyHtml;
                    if (!mail.IsBodyHtml)
                    {
                        mail.Body = mail.Body.Replace("\r\n", "\r");
                        mail.Body = mail.Body.Replace("\r", "\r\n");
                    }
                    if (emailMessage.Attachments != null && emailMessage.Attachments.Count > 0)
                    {
                        mail.Attachments.Add(emailMessage.Attachments[0]);
                    }
                    smtpServer.Port = Convert.ToInt32(emailMessage.PortName);
                    smtpServer.Credentials = new NetworkCredential(emailMessage.EmailFrom, emailMessage.EmailPassword);
                    smtpServer.EnableSsl = emailMessage.MailSsl;
                    smtpServer.Send(mail);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "");
                }
            });
        }
    }

    public interface IEmailService
    {
        void SendMail(EmailMessage emailMessage);
    }
}