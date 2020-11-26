using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Unit : BaseEntity, ICommonAduit
    {
        public string UnitName { get; set; }
        public int? CompanyId { get; set; }
    }
}