using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using GSC.Helper;
using GSC.Shared.Generic;

namespace GSC.Respository.Audit
{
    public interface IVolunteerAuditTrailRepository : IGenericRepository<VolunteerAuditTrail>
    {
        void Save(AuditModule moduleId, AuditTable tableId, AuditAction action, int recordId, int? parentRecordId,
            List<VolunteerAuditTrail> changes);

        IList<VolunteerAuditTrailDto> Search(VolunteerAuditTrailDto search);
    }
}