using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IMetricsRepository : IGenericRepository<PlanMetrics>
    {
        List<PlanMetricsGridDto> GetMetricsList(bool isDeleted, int typesId);
        string Duplicate(PlanMetrics objSave);
    }
}
