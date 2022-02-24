using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringReportVariableValueAuditRepository : IGenericRepository<CtmsMonitoringReportVariableValueAudit>
    {
        IList<CtmsMonitoringReportVariableValueAuditDto> GetAudits(int CtmsMonitoringReportVariableValueId);
        void Save(CtmsMonitoringReportVariableValueAudit audit);
    }
}