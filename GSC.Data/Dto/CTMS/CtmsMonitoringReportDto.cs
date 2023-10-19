using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringReportDto : BaseDto
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringReportStatus? ReportStatus { get; set; }
    }
}
