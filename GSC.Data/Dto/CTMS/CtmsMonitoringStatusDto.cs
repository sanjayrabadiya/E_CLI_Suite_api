﻿using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringStatusDto : BaseDto
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringSiteStatus? Status { get; set; }
    }

    public class CtmsMonitoringStatusGridDto : BaseAuditDto
    {
        public int CtmsMonitoringId { get; set; }
        public MonitoringSiteStatus? Status { get; set; }
        public string StatusName { get; set; }
        public string ActivityName { get; set; }
    }
}
