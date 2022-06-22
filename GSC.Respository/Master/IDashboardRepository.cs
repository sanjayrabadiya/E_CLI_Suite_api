using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IDashboardRepository
    {
        dynamic GetDashboardVisitGraph(int projectId, int countryId, int siteId);
        dynamic ScreenedToRandomizedGraph(int projectId, int countryId, int siteId);
        dynamic GetRandomizedProgressGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardQueryGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardLabelGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardFormsGraph(int projectId, int countryId, int siteId);
        dynamic GetQueryManagementTotalQueryStatus(int projectId, int countryId, int siteId);
        dynamic GetQueryManagementVisitWiseQuery(int projectId, int countryId, int siteId);
        dynamic GetQueryManagementRoleWiseQuery(int projectId, int countryId, int siteId);
    }
}
