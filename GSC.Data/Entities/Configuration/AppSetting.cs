using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Configuration
{
    public class AppSetting : BaseEntity, ICommonAduit
    {
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
        public int? CompanyId { get; set; }
    }
}