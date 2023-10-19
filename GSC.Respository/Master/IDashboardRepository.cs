using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.ProjectRight;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IDashboardRepository
    {
        dynamic GetDashboardPatientStatus(int projectId, int countryId, int siteId);
        dynamic GetDashboardVisitGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardMonitoringReportGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardMonitoringReportGrid(int projectId, int countryId, int siteId);
        dynamic ScreenedToRandomizedGraph(int projectId, int countryId, int siteId);
        dynamic GetRandomizedProgressGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardQueryGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardLabelGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardFormsGraph(int projectId, int countryId, int siteId);
        dynamic GetQueryManagementTotalQueryStatus(int projectId, int countryId, int siteId);
        dynamic GetQueryManagementVisitWiseQuery(int projectId, int countryId, int siteId);
        dynamic GetQueryManagementRoleWiseQuery(int projectId, int countryId, int siteId);
        dynamic getVisitStatuschart(int projectId, int countryId, int siteId);
        DashboardInformConsentStatusDto GetDashboardInformConsentCount(int projectId, int countryId, int siteId);
        List<DashboardInformConsentStatusDto> GetDashboardInformConsentChart(int projectId, int countryId, int siteId);

        List<DashboardQueryGraphFinalDto> GetNewDashboardQueryGraphData(int projectId, int countryId, int siteId);

        dynamic GetCTMSMonitoringChart(int projectId, int countryId, int siteId);

        dynamic GetCTMSMonitoringPIChart(int projectId, int countryId, int siteId);

        List<CtmsMonitoringPlanDashoardDto> GetCTMSMonitoringPlanDashboard(int projectId, int countryId, int siteId);
        List<StudyPlanTaskDto> GetCTMSPreRequisiteDashboard(int projectId, int countryId, int siteId);

        List<CtmsActionPointGridDto> GetCTMSOpenActionDashboard(int projectId, int countryId, int siteId);
        List<CtmsMonitoringReportVariableValueQueryDto> GetCTMSQueriesDashboard(int projectId, int countryId, int siteId);

        dynamic GetCTMSMonitoringActionPointChartDashboard(int projectId, int countryId, int siteId);
        dynamic GetDashboardAesBySeverityGraph(int projectId, int countryId, int siteId);

        dynamic GetDashboardAEDetail(int projectId, int countryId, int siteId);

        dynamic GetDashboardAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardSAesBySeverityGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardSAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardPatientEngagementGraph(int projectId, int countryId, int siteId, int FilterFlag);
        dynamic GetDashboardByCriticalGraph(int projectId, int countryId, int siteId);
        dynamic GetDashboardByDiscontinuationGraph(int projectId, int countryId, int siteId);
        dynamic GetEnrolledGraph(int projectId, int countryId, int siteId);
        dynamic GetScreenedGraph(int projectId, int countryId, int siteId);
        dynamic GetRandomizedGraph(int projectId, int countryId, int siteId);
        List<PlanMetricsGridDto> GetDashboardNumberOfSubjectsGrid(bool isDeleted, int metricsId, int projectId, int countryId, int siteId);
        dynamic GetIMPShipmentDetailsCount(int projectId, int countryId, int siteId);
        List<TreatmentvsArms> GetTreatmentvsArmData(int projectId, int countryId, int siteId);
        List<FactoreDashboardModel> GetFactorDataReportDashbaord(int projectId, int countryId, int siteId);
        List<ImpShipmentGridDashboard> GetIMPShipmentDetailsData(int projectId, int countryId, int siteId);

        List<TreatmentvsArms> GetVisitWiseAllocationData(int projectId, int countryId, int siteId);

        List<FactoreDashboardModel> GetFactorDataReportDashbaordCount(int projectId, int countryId, int siteId);

    }
}
