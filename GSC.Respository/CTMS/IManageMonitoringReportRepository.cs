using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportRepository : IGenericRepository<ManageMonitoringReport>
    {
        List<ManageMonitoringReportGridDto> GetMonitoringReport(int projectId);
    }
}
