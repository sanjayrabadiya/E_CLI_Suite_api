using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringStatusRepository : IGenericRepository<CtmsMonitoringStatus>
    {
        List<CtmsMonitoringStatusGridDto> GetCtmsMonitoringStatusList(int CtmsMonitoringId);
        CtmsMonitoringStatusGridDto GetSiteStatus(int ProjectId);
        string GetFormApprovedOrNot(int projectId, int siteId, int tabNumber);

        void UpdateSiteStatus(CtmsMonitoringStatusDto ctmsMonitoringDto);
    }
}