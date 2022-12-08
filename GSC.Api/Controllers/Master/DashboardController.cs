using System.Collections.Generic;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
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
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectDocumentReviewRepository _projectDocumentReviewRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DashboardController(
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IAEReportingRepository aEReportingRepository,
            IProjectRepository projectRepository,
            IProjectDocumentReviewRepository projectDocumentReviewRepository,
            IDashboardRepository dashboardRepository,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository
            )
        {
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _aEReportingRepository = aEReportingRepository;
            _projectRepository = projectRepository;
            _projectDocumentReviewRepository = projectDocumentReviewRepository;
            _dashboardRepository = dashboardRepository;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
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
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectWorkplaceArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_econsentReviewDetailsRepository.GetEconsentMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_aEReportingRepository.GetAEReportingMyTaskList(ProjectId, (int)(SiteId != null ? SiteId : ProjectId)));
            objdashboard.MyTaskList.AddRange(_ctmsMonitoringReportReviewRepository.GetSendTemplateList(ProjectId, SiteId > 0 ? SiteId : 0));
            objdashboard.MyTaskList.AddRange(_ctmsMonitoringReportReviewRepository.GetSendBackTemplateList(ProjectId, SiteId > 0 ? SiteId : 0));
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
        public IActionResult getCTMSMonitoringPlanDashboard(int projectId,int countryId, int siteId)
        {
            var queries = _dashboardRepository.getCTMSMonitoringPlanDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }
        [HttpGet]
        [Route("GetCTMSMonitoringActionPointChartDashboard/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult getCTMSMonitoringActionPointChartDashboard(int projectId, int countryId, int siteId)
        {
            var queries = _dashboardRepository.getCTMSMonitoringActionPointChartDashboard(projectId, countryId, siteId);
            return Ok(queries);
        }
    }
}