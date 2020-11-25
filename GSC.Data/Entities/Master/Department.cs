using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Department : BaseEntity, ICommonAduit
    {
        public string DepartmentCode { get; set; }

        public string DepartmentName { get; set; }

        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }
}