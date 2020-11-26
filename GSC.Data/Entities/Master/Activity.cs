using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Activity : BaseEntity, ICommonAduit
    {
        public string ActivityName { get; set; }
        public int? CompanyId { get; set; }
    }
}