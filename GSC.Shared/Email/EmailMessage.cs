using System.Collections.Generic;
using System.Net.Mail;

namespace GSC.Shared.Email
{
    public class EmailMessage
    {
        private List<Attachment> _attachments;
        public string SendFrom { get; set; }
        public string SendTo { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public bool IsBodyHtml { get; set; }

        public List<Attachment> Attachments
        {
            get => _attachments ?? (_attachments = new List<Attachment>());
            set => _attachments = value;
        }

        public string EmailFrom { get; set; }
        public string PortName { get; set; }
        public string DomainName { get; set; }
        public string EmailPassword { get; set; }
        public bool MailSsl { get; set; }
        public string? DLTTemplateId { get; set; }
    }
}