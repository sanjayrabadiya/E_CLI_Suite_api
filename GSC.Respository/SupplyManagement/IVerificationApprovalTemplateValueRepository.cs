using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.SupplyManagement
{
    public interface IVerificationApprovalTemplateValueRepository : IGenericRepository<VerificationApprovalTemplateValue>
    {
        string GetValueForAudit(VerificationApprovalTemplateValueDto verificationApprovalTemplateValueDto);
        void DeleteChild(int verificationApprovalTemplateValueId);
    }
}