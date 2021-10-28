using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentReviewDetailsAuditRepository : IGenericRepository<EconsentReviewDetailsAudit>
    {
        void GenerateICFDetailReport(EconsentReviewDetailsAuditParameterDto details);
    }
}
