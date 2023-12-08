using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyLocation : BaseEntity, ICommonAduit
    {
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
