using GSC.Common.Base;
using GSC.Common.Common;
namespace GSC.Data.Entities.CTMS
{
    public class PassthroughSiteContract : BaseEntity, ICommonAduit
    {
        public int SiteContractId { get; set; }
        public int PassThroughCostActivityId { get; set; }
        public decimal PassThroughTotalRate { get; set; }
        public decimal PayableTotal {  get; set; }

        public SiteContract SiteContract { get; set; }
        public PassThroughCostActivity PassThroughCostActivity { get; set; }

    }
}
