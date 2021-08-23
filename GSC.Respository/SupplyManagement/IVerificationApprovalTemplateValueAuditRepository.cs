using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;
using GSC.Common.GenericRespository;

namespace GSC.Respository.SupplyManagement
{
    public interface IVerificationApprovalTemplateValueAuditRepository : IGenericRepository<VerificationApprovalTemplateValueAudit>
    {
        IList<VerificationApprovalAuditDto> GetAudits(int VerificationApprovalTemplateValueId);
        void Save(VerificationApprovalTemplateValueAudit audit);
    }
}