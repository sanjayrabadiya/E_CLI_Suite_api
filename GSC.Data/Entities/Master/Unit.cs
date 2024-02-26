using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Master
{
    public class Unit : BaseEntity, ICommonAduit
    {
        public string UnitName { get; set; }
        public int? CompanyId { get; set; }

        public int? AppScreenId { get; set; }
        public AppScreen AppScreen { get; set; }
    }
}