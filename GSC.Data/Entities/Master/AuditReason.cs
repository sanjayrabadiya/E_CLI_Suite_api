using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class AuditReason : BaseEntity
    {
        public string ReasonName { get; set; }
        public AuditModule ModuleId { get; set; }
        public bool IsOther { get; set; }
        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }
}