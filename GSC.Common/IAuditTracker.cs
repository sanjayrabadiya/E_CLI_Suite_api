﻿
using GSC.Data.Dto.Audit;
using System.Collections.Generic;


namespace GSC.Common
{
    public interface IAuditTracker
    {
        List<TrackerResult> GetAuditTracker();
    }
}
