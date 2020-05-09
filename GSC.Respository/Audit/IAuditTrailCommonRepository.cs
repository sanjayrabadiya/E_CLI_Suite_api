using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;

namespace GSC.Respository.Audit
{
    public interface IAuditTrailCommonRepository : IGenericRepository<AuditTrailCommon>
    {
        IList<AuditTrailCommonDto> Search(AuditTrailCommonDto search);
    }
}