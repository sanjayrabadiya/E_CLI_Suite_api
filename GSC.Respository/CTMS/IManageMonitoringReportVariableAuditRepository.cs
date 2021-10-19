using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportVariableAuditRepository : IGenericRepository<ManageMonitoringReportVariableAudit>
    {
        //IList<VerificationApprovalAuditDto> GetAudits(int VerificationApprovalTemplateValueId);
        void Save(ManageMonitoringReportVariableAudit audit);
    }
}