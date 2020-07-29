using GSC.Common.GenericRespository;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using System.Collections.Generic;

namespace GSC.Respository.Medra
{
    public interface IMeddraCodingAuditRepository : IGenericRepository<MeddraCodingAudit>
    {
        MeddraCodingAudit SaveAudit(string note, int meddraCodingId, int? meddraLowLevelTermId, int? meddraSocTermId, string Action, int? ReasonId, string ReasonOth);
        IList<MeddraCodingAuditDto> GetMeddraAuditDetails(int MeddraCodingId);
    }
}