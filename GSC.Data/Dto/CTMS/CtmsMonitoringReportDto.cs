using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringReportDto : BaseDto
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringReportStatus? ReportStatus { get; set; }
    }
}
