using GSC.Shared.Audit;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Shared.Audit
{
    public interface IAuditTracker
    {
        List<AuditTrailViewModel> GetAuditTracker();
    }
}
