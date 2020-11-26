using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Configuration
{
    public class EmailSetting : BaseEntity, ICommonAduit
    {
        public string EmailFrom { get; set; }

        public string PortName { get; set; }
        public string DomainName { get; set; }

        public string EmailPassword { get; set; }
        public bool MailSsl { get; set; }

        public int CompanyId { get; set; }
    }
}