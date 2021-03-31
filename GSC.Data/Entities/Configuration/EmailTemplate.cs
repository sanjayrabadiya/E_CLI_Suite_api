using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Configuration
{
    public class EmailTemplate : BaseEntity, ICommonAduit
    {
        public string KeyName { get; set; }
        public int EMailSettingId { get; set; }
        public string SubjectName { get; set; }
        public string Bcc { get; set; }
        public string Body { get; set; }
        public int CompanyId { get; set; }
        public EmailSetting EmailSetting { get; set; }
        public string? DLTTemplateId { get; set; }
    }
}