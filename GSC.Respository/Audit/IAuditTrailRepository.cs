using System.Collections.Generic;
using GSC.Common.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.Report;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Audit
{
    public interface IAuditTrailRepository : IGenericRepository<AuditTrail>
    {
        IList<AuditTrailDto> Search(AuditTrailDto search);
        void SearchProjectDesign(ProjectDatabaseSearchDto search);
    }
}