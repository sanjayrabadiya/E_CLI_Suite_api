using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Report;

namespace GSC.Respository.Reports
{
    public interface IJobMonitoringRepository : IGenericRepository<JobMonitoring>
    {
        List<JobMonitoringDto> JobMonitoringList();
    }
}