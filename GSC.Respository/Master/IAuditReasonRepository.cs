using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public interface IAuditReasonRepository : IGenericRepository<AuditReason>
    {
        List<DropDownDto> GetAuditReasonDropDown(AuditModule auditModule);
        string Duplicate(AuditReason objSave);
    }
}