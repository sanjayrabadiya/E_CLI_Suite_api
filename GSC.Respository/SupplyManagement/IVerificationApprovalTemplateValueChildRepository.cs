using GSC.Common.GenericRespository;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.SupplyManagement
{
    public interface IVerificationApprovalTemplateValueChildRepository : IGenericRepository<VerificationApprovalTemplateValueChild>
    {
        void Save(VerificationApprovalTemplateValue verificationApprovalTemplateValue);
    }
}