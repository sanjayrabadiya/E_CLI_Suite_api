using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;

namespace GSC.Respository.SupplyManagement
{
    public interface IVerificationApprovalTemplateRepository : IGenericRepository<VerificationApprovalTemplate>
    {
        DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(DesignVerificationApprovalTemplateDto designTemplateDto, int ProductVerificationDetailId);
        VerificationApprovalTemplate AddVerificationApprovalTemplateHistory(VerificationApprovalTemplate verificationApprovalTemplate, VerificationApprovalTemplateDto verificationApprovalTemplateDto);
        void Addverificationhistory(VerificationApprovalTemplateDto verificationApprovalTemplateDto);

        VerificationApprovalTemplateHistoryDto getverificationApprovalTemplateHistory(VerificationApprovalTemplateHistoryDto verificationApprovalTemplateDto);

        void AddHistory(VerificationApprovalTemplateDto verificationApprovalTemplateDto);

        void Addvalues(VerificationApprovalTemplateDto verificationApprovalTemplateDto);

        void SendStatusApproval(VerificationApprovalTemplateDto verificationApprovalTemplateDto, int Id, VerificationApprovalTemplate verification);
    }
}