using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.CTMS
{
    public class PassThroughCostActivity : BaseEntity, ICommonAduit
    {
        public string ActivityName { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
