using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class Activity : BaseEntity, ICommonAduit
    {
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public AuditModule ModuleId { get; set; }
        public int? CompanyId { get; set; }
    }
}