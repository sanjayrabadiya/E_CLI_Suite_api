
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using System.Collections.Generic;


namespace GSC.Common
{
    public interface IAuditTracker
    {
        List<AuditTrailCommon> GetAuditTracker();
    }
}
