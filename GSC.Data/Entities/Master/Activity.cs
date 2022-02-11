using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class Activity : BaseEntity, ICommonAduit
    {
        public string ActivityCode { get; set; }
        public int CtmsActivityId { get; set; }
        public int AppScreenId { get; set; }
        public int? CompanyId { get; set; }
        public CtmsActivity CtmsActivity { get; set; }
        public AppScreen AppScreen { get; set; }
    }
}