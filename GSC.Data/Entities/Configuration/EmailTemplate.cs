using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Configuration
{
    public class EmailTemplate : BaseEntity
    {
        public string KeyName { get; set; }
        public int EMailSettingId { get; set; }
        public string SubjectName { get; set; }
        public string Bcc { get; set; }
        public string Body { get; set; }
        public int CompanyId { get; set; }
        public EmailSetting EmailSetting { get; set; }
    }
}