using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class DomainClass : BaseEntity, ICommonAduit
    {
        public string DomainClassCode { get; set; }

        public string DomainClassName { get; set; }
        public int? CompanyId { get; set; }

        public bool? IsStatic { get; set; }
        public bool SystemType { get; set; }
    }
}