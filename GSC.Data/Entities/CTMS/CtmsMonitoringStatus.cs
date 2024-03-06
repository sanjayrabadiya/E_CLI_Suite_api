using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringStatus : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringSiteStatus? Status { get; set; }
        public CtmsMonitoring CtmsMonitoring { get; set; }
    }
}
