using GSC.Common.Base;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementThresholdHistory : BaseEntity
    {
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
    }
}
