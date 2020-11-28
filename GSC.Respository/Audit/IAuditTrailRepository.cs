using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using GSC.Helper;
using GSC.Shared.Generic;

namespace GSC.Respository.Audit
{
    public interface IAuditTrailRepository : IGenericRepository<AuditTrail>
    {
        void Save(AuditModule moduleId, AuditTable tableId, AuditAction action, int recordId, int? parentRecordId,
            List<AuditTrail> changes);

        IList<AuditTrailDto> Search(AuditTrailDto search);
    }
}