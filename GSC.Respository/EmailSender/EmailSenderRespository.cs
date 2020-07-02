using System.Linq;
using System.Text.RegularExpressions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.EmailSender
{
    public class EmailSenderRespository : GenericRespository<EmailTemplate, GscContext>, IEmailSenderRespository
    {
        private readonly GscContext _context;
        private readonly IEmailService _emailService;

        public EmailSenderRespository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IEmailService emailService)
            : base(uow, jwtTokenAccesser)
        {
            _emailService = emailService;
            _context = uow.Context;
        }

        public void SendRegisterEMail(string toMail, string password, string userName)
        {
            var emailMessage = ConfigureEmail("empreg", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password);
            _emailService.SendMail(emailMessage);
        }


        public void SendChangePasswordEMail(string toMail, string password, string userName)
        {
            var emailMessage = ConfigureEmail("resetpass", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password);
            _emailService.SendMail(emailMessage);
        }

        public void SendForgotPasswordEMail(string toMail, string password, string userName)
        {
            var emailMessage = ConfigureEmail("forgotpass", userName);
            emailMessage.SendTo = toMail;
            emailMessage.MessageBody = ReplaceBody(emailMessage.MessageBody, userName, password);
            _emailService.SendMail(emailMessage);
        }

        private EmailMessage ConfigureEmail(string keyName, string userName)
        {
            var user = _context.Users.Where(x => x.UserName == userName && x.DeletedDate == null).FirstOrDefault();
            var result = All.Include(x => x.EmailSetting).FirstOrDefault(x =>
               x.DeletedDate == null && x.KeyName == keyName);
            var emailMessage = new EmailMessage();

            if (result != null)
            {
                emailMessage.SendFrom = result.EmailSetting.EmailFrom;
                emailMessage.Subject = result.SubjectName;
                emailMessage.MessageBody = result.Body;
                emailMessage.Cc = result.Bcc;
                emailMessage.Bcc = result.Bcc;
                emailMessage.IsBodyHtml = true;
                emailMessage.Attachments = null;
                emailMessage.EmailFrom = result.EmailSetting.EmailFrom;
                emailMessage.PortName = result.EmailSetting.PortName;
                emailMessage.DomainName = result.EmailSetting.DomainName;
                emailMessage.EmailPassword = result.EmailSetting.EmailPassword;
                emailMessage.MailSsl = result.EmailSetting.MailSsl;
            }

            return emailMessage;
        }

        private string ReplaceBody(string body, string userName, string password)
        {
            body = Regex.Replace(body, "##name##", userName, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>username</strong>##", "<strong>" + userName + "</strong>",
                RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##password##", password, RegexOptions.IgnoreCase);
            body = Regex.Replace(body, "##<strong>password</strong>##", "<strong>" + password + "</strong>",
                RegexOptions.IgnoreCase);
            return body;
        }
    }
}