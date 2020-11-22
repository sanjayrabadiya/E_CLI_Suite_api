using GSC.Common.Base;

namespace GSC.Data.Entities.Configuration
{
    public class AppSetting : BaseEntity
    {
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
        public int? CompanyId { get; set; }
    }
}