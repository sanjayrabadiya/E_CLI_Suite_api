﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Threading.Tasks;

namespace GSC.Respository.SupplyManagement
{
    public interface IVerificationApprovalTemplateRepository : IGenericRepository<VerificationApprovalTemplate>
    {
        DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(DesignVerificationApprovalTemplateDto designTemplateDto, int ProductVerificationDetailId);

        void SendForApprovalEmail(VerificationApprovalTemplateDto verificationApprovalTemplateDto, ProductReceipt productReceipt);

        void SendTemplateApproveRejectEmail(VerificationApprovalTemplateDto verificationApprovalTemplateDto, ProductReceipt productReceipt);

        Task SendForApprovalVerificationTemplateScheduleEmail();

    }
}