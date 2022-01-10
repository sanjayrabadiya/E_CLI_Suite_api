using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportVariableRepository : IGenericRepository<ManageMonitoringReportVariable>
    {
        MonitoringReportTemplateDto GetReportTemplateVariable(MonitoringReportTemplateDto designTemplateDto, int ManageMonitoringReportId);
        string GetValueForAudit(ManageMonitoringReportVariableDto manageMonitoringReportVariableDto);
        void DeleteChild(int manageMonitoringReportVariableId);
        void UpdateChild(List<ManageMonitoringReportVariableChild> children);
    }
}
