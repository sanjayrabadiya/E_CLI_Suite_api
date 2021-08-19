using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.SupplyManagement
{
    public interface IVerificationApprovalTemplateRepository : IGenericRepository<VerificationApprovalTemplate>
    {
        DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(DesignVerificationApprovalTemplateDto designTemplateDto, int verificationApprovalTemplateId);
    }
}