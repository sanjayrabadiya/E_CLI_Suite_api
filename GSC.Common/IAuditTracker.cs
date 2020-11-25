using GSC.Common.Base;
using GSC.Common.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;


namespace GSC.Common
{
    public interface IAuditTracker
    {
        List<AuditTrailCommon> GetAuditTracker(IList<EntityEntry> entities, DbContext context);
    }
}
