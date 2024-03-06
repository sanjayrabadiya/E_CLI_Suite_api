using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;
namespace GSC.Respository.CTMS
{
    public interface IOverTimeMetricsRepository : IGenericRepository<OverTimeMetrics>
    {
        List<OverTimeMetricsGridDto> GetTasklist(bool isDeleted, int metricsId, int projectId, int countryId, int siteId);
        string PlannedCheck(OverTimeMetrics objSave);
        string UpdatePlanning(OverTimeMetrics overTimeMetricsDto);
        List<OverTimeMetrics> UpdateAllActualNo(bool isDeleted, int metricsId, int projectId, int countryId, int siteId);
        List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int parentProjectId);
    }
}
