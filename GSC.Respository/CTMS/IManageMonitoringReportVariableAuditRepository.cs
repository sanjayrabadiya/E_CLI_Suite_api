using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportVariableAuditRepository : IGenericRepository<ManageMonitoringReportVariableAudit>
    {
        IList<ManageMonitoringReportVariableAuditDto> GetAudits(int ManageMonitoringReportVariableId);
        void Save(ManageMonitoringReportVariableAudit audit);
    }
}