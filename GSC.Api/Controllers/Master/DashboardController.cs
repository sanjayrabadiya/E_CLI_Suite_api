using System.Collections.Generic;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Master;
using GSC.Helper;
using GSC.Respository.AdverseEvent;
using GSC.Respository.CTMS;
using GSC.Respository.Etmf;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        private readonly ICtmsMonitoringReportReviewRepository _ctmsMonitoringReportReviewRepository;
        private readonly IAEReportingRepository _aEReportingRepository;
        private readonly IProjectDocumentReviewRepository _projectDocumentReviewRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ICtmsWorkflowApprovalRepository _ctmsWorkflowApprovalRepository;

        public DashboardController(
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IAEReportingRepository aEReportingRepository,
            IProjectDocumentReviewRepository projectDocumentReviewRepository,
            IDashboardRepository dashboardRepository,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository,
            ICtmsWorkflowApprovalRepository ctmsWorkflowApprovalRepository
            )
        {
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _aEReportingRepository = aEReportingRepository;
            _projectDocumentReviewRepository = projectDocumentReviewRepository;
            _dashboardRepository = dashboardRepository;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
            _ctmsWorkflowApprovalRepository = ctmsWorkflowApprovalRepository;
        }

        #region Dashboard Overview Code

        [HttpGet]
        [Route("GetMyTaskList/{ProjectId}/{SiteId:int?}")]
        public IActionResult GetMyTaskList(int ProjectId, int? SiteId)
        {
            DashboardDetailsDto objdashboard = new DashboardDetailsDto();
            objdashboard.eTMFApproveData = _projectArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId);
            objdashboard.eTMFSendData = _projectWorkplaceArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId);
            objdashboard.eTMFSubSecApproveData = _projectSubSecArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId);
            objdashboard.eTMFSubSecSendData = _projectSubSecArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId);
            objdashboard.eTMFSendBackData = _projectWorkplaceArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId);
            objdashboard.eTMFSubSecSendBackData = _projectSubSecArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId);
            objdashboard.eConsentData = _econsentReviewDetailsRepository.GetEconsentMyTaskList(ProjectId);
            objdashboard.eAdverseEventData = _aEReportingRepository.GetAEReportingMyTaskList(ProjectId, (int)(SiteId != null ? SiteId : ProjectId));
            objdashboard.manageMonitoringReportSendData = _ctmsMonitoringReportReviewRepository.GetSendTemplateList(ProjectId, SiteId > 0 ? SiteId : 0);
            objdashboard.manageMonitoringReportSendBackData = _ctmsMonitoringReportReviewRepository.GetSendBackTemplateList(ProjectId, SiteId > 0 ? SiteId : 0);

            return Ok(objdashboard);
        }


        [HttpGet]
        [Route("GetDashboardMyTaskList/{ProjectId}/{SiteId:int?}")]
        public IActionResult GetDashboardMyTaskList(int ProjectId, int? SiteId)
        {
            DashboardMyTaskDto objdashboard = new DashboardMyTaskDto()
            {
                MyTaskList = new List<DashboardDto>()
            };
            objdashboard.MyTaskList.AddRange(_projectArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectWorkplaceArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectWorkplaceArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId));

            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId));

            objdashboard.MyTaskList.AddRange(_econsentReviewDetailsRepository.GetEconsentMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_aEReportingRepository.GetAEReportingMyTaskList(ProjectId, (int)(SiteId != null ? SiteId : ProjectId)));
            objdashboard.MyTaskList.AddRange(_ctmsMonitoringReportReviewRepository.GetSendTemplateList(ProjectId, SiteId > 0 ? SiteId : 0));
            objdashboard.MyTaskList.AddRange(_ctmsMonitoringReportReviewRepository.GetSendBackTemplateList(ProjectId, SiteId > 0 ? SiteId : 0));
            objdashboard.MyTaskList.AddRange(_ctmsWorkflowApprovalRepository.GetCtmsApprovalMyTask(ProjectId));
            return Ok(objdashboard);
        }

        [HttpGet]
        [Route("GetDashboardPatientStatus/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardPatientStatus(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetDashboardPatientStatus(projectId, countryId, siteId));
        }

        [HttpGet]
        [Route("GetDashboardVisitGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardVisitGraph(int projectId, int countryId, int siteId)
        {
            var screeningVisits = _dashboardRepository.GetDashboardVisitGraph(projectId, countryId, siteId);
            return Ok(screeningVisits);
        }

        [HttpGet]
        [Route("GetDashboardMonitoringReportGrid/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardMonitoringReportGrid(int projectId, int countryId, int siteId)
        {
            var reportGrid = _dashboardRepository.GetDashboardMonitoringReportGrid(projectId, countryId, siteId);
            return Ok(reportGrid);
        }

        [HttpGet]
        [Route("GetDashboardMonitoringReportGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardMonitoringReportGraph(int projectId, int countryId, int siteId)
        {
            var screeningVisits = _dashboardRepository.GetDashboardMonitoringReportGraph(projectId, countryId, siteId);
            return Ok(screeningVisits);
        }


        [HttpGet]
        [Route("ScreenedToRandomizedGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult ScreenedToRandomizedGraph(int projectId, int countryId, int siteId)
        {
            var DaysDiffList = _dashboardRepository.ScreenedToRandomizedGraph(projectId, countryId, siteId);
            return Ok(DaysDiffList);
        }

        [HttpGet]
        [Route("GetRandomizedProgressGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetRandomizedProgressGraph(int projectId, int countryId, int siteId)
        {
            var progresses = _dashboardRepository.GetRandomizedProgressGraph(projectId, countryId, siteId);
            return Ok(progresses);
        }

        [HttpGet]
        [Route("GetDashboardQueryGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardQueryGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardQueryGraph(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetDashboardLabelGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardLabelGraph(int projectId, int countryId, int siteId)
        {
            var labelGraphs = _dashboardRepository.GetDashboardLabelGraph(projectId, countryId, siteId);
            return Ok(labelGraphs);
        }

        [HttpGet]
        [Route("GetDashboardFormsGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardFormsGraph(int projectId, int countryId, int siteId)
        {
            var formGrpah = _dashboardRepository.GetDashboardFormsGraph(projectId, countryId, siteId);
            return Ok(formGrpah);
        }
        #endregion

        #region Dashboard Query Management Code

        [HttpGet]
        [Route("GetQueryManagementTotalQueryStatus/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetQueryManagementTotalQueryStatus(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetQueryManagementTotalQueryStatus(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetQueryManagementVisitWiseQuery/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetQueryManagementVisitWiseQuery(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetQueryManagementVisitWiseQuery(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetQueryManagementRoleWiseQuery/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetQueryManagementRoleWiseQuery(int projectId, int countryId, int siteId)
        {
            var result = _dashboardRepository.GetQueryManagementRoleWiseQuery(projectId, countryId, siteId);
            return Ok(result);
        }

        [HttpGet]
        [Route("getVisitStatuschart/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult getVisitStatuschart(int projectId, int countryId, int siteId)
        {
            var result = _dashboardRepository.getVisitStatuschart(projectId, countryId, siteId);
            return Ok(result);
        }

        #endregion

        //Add By Tinku on 07/06/2022 for dasboard tranning data
        [HttpGet]
        [Route("GetNewDashboardTraining/{projectid}/{countryid}/{siteid}")]
        public IActionResult GetDashboardProjectTraining(int projectid, int countryid, int siteid)
        {
            return Ok(_projectDocumentReviewRepository.GetNewDashboardTranningData(projectid, countryid, siteid));
        }

        [HttpGet]
        [Route("GetTrainingCount")]
        public IActionResult GetTrainingCount()
        {
            return Ok(_projectDocumentReviewRepository.CountTranningNotification());
        }

        [HttpGet]
        [Route("GetDashboardInformConsentCount/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardInformConsentCount(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardInformConsentCount(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetDashboardInformConsentChart/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardInformConsentChart(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardInformConsentChart(projectId, countryId, siteId);
            return Ok(queries);
        }

        //Add By prakash on 13/10/2022 for dasboard query graph data
        //Change by vipul on 14/10/2022 for add filter country and studyid

        [HttpGet]
        [Route("GetNewDashboardQueryGraphData/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetNewDashboardQueryGraphData(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetNewDashboardQueryGraphData(projectId, countryId, siteId));
        }

        [HttpGet]
        [Route("GetCTMSMonitoringChart/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetCTMSMonitoringChart(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSMonitoringChart(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetCTMSMonitoringPIChart/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetCTMSMonitoringPIChart(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSMonitoringPIChart(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetCTMSMonitoringPlanDashboard/{ProjectId}/{countryId}/{SiteId}")]
        public IActionResult GetCTMSMonitoringPlanDashboard(int projectId,int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSMonitoringPlanDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetCTMSPreRequisiteDashboard/{ProjectId}/{countryId}/{SiteId}")]
        public IActionResult GetCTMSPreRequisiteDashboard(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSPreRequisiteDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetCTMSMonitoringActionPointChartDashboard/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetCTMSMonitoringActionPointChartDashboard(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSMonitoringActionPointChartDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetCTMSMonitoringActionPointGridDashboard/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetCTMSMonitoringActionPointGridDashboard(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSOpenActionDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetCTMSMonitoringQueriesGridDashboard/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetCTMSMonitoringQueriesGridDashboard(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetCTMSQueriesDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetDashboardAesBySeverityGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardAesBySeverityGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardAesBySeverityGraph(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetDashboardAesBySeverityandCausalityGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardAesBySeverityandCausalityGraph(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetDashboardSAesBySeverityGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardSAesBySeverityGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardSAesBySeverityGraph(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetDashboardSAesBySeverityandCausalityGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardSAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardSAesBySeverityandCausalityGraph(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetDashboardPatientEngagementGraph/{ProjectId}/{countryId}/{siteId}/{FilterFlag}")]
        public IActionResult GetDashboardPatientEngagementGraph(int projectId, int countryId, int siteId, int FilterFlag)
        {
            var queries = _dashboardRepository.GetDashboardPatientEngagementGraph(projectId, countryId, siteId, FilterFlag);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetDashboardAEDetail/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardAEDetail(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetDashboardAEDetail(projectId, countryId, siteId));
        }

        [HttpGet]
        [Route("GetDashboardByCriticalGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardByCriticalGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardByCriticalGraph(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetDashboardByDiscontinuationGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardByDiscontinuationGraph(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.GetDashboardByDiscontinuationGraph(projectId, countryId, siteId);
            return Ok(queries);
        }

        [HttpGet]
        [Route("GetEnrolledGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetEnrolledGraph(int projectId, int countryId, int siteId)
        {
            var result = _dashboardRepository.GetEnrolledGraph(projectId, countryId, siteId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetScreenedGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetScreenedGraph(int projectId, int countryId, int siteId)
        {
            var result = _dashboardRepository.GetScreenedGraph(projectId, countryId, siteId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetRandomizedGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetRandomizedGraph(int projectId, int countryId, int siteId)
        {
            var result = _dashboardRepository.GetRandomizedGraph(projectId, countryId, siteId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetDashboardNumberOfSubjectsGrid/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardNumberOfSubjectsGrid(bool isDeleted, int metricsId, int projectId, int countryId, int siteId)
        {
            var subjectsGrid = _dashboardRepository.GetDashboardNumberOfSubjectsGrid(isDeleted, metricsId, projectId, countryId, siteId);
            return Ok(subjectsGrid);
        }

        [HttpGet]
        [Route("GetIMPShipmentDetailsCount/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetIMPShipmentDetailsCount(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetIMPShipmentDetailsCount(projectId, countryId, siteId));
        }
        [HttpGet]
        [Route("GetTreatmentvsArmData/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetTreatmentvsArmData(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetTreatmentvsArmData(projectId, countryId, siteId));
        }

        [HttpGet]
        [Route("GetFactorDataReportDashbaord/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetFactorDataReportDashbaord(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetFactorDataReportDashbaord(projectId, countryId, siteId));
        }
        
        [HttpGet]
        [Route("GetIMPShipmentDetailsData/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetIMPShipmentDetailsData(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetIMPShipmentDetailsData(projectId, countryId, siteId));
        }
        [HttpGet]
        [Route("GetVisitWiseAllocationData/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetVisitWiseAllocationData(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetVisitWiseAllocationData(projectId, countryId, siteId));
        }
        [HttpGet]
        [Route("GetKitCountReport/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetKitCountReport(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetKitCountReport(projectId, countryId, siteId));
        }
        [HttpGet]
        [Route("GetProductWiseVerificationReport/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetProductWiseVerificationReport(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetProductWiseVerificationReport(projectId, countryId, siteId));
        }
        [HttpGet]
        [Route("GetkitCreatedDataReport/{projectId}/{countryId}/{siteId}")]
        public IActionResult GetkitCreatedDataReport(int projectId, int countryId, int siteId)
        {
            return Ok(_dashboardRepository.GetkitCreatedDataReport(projectId, countryId, siteId));
        }


        [HttpGet]
        [Route("GetSubjectStatusGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetSubjectStatusCountGraph(int projectId, int countryId, int siteId)
        {
            var statusCount = _dashboardRepository.GetSubjectStatusGraph(projectId, countryId, siteId);
            return Ok(statusCount);
        }

        [HttpGet]
        [Route("GetDashboardSubjectList/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardSubjectList(int projectId, int countryId, int siteId)
        {
            var statusCount = _dashboardRepository.GetDashboardSubjectList(projectId, countryId, siteId);
            return Ok(statusCount);
        }
        [HttpGet]
        [Route("GetCTMSProjectStatusChartDashboard/{ProjectId}/{filterType:int}")]
        public IActionResult GetCTMSProjectStatusChartDashboard(int projectId, CtmsStudyTaskFilter filterType)
        {
            var queries = _dashboardRepository.GetCTMSProjectStatusChartDashboard(projectId,  filterType);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetCTMSProjectStatusGrid/{ProjectId}/{filterType:int}")]
        public IActionResult GetCTMSProjectStatusGrid(int projectId, CtmsStudyTaskFilter filterType)
        {
            var queries = _dashboardRepository.GetCTMSProjectStatusGrid(projectId, filterType);
            return Ok(queries);
        }
    }
}