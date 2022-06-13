using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.AdverseEvent;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.Etmf;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly IManageMonitoringReportReviewRepository _manageMonitoringReportReviewRepository;
        private readonly IAEReportingRepository _aEReportingRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IProjectDocumentReviewRepository _projectDocumentReviewRepository;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public DashboardController(
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IManageMonitoringReportReviewRepository manageMonitoringReportReviewRepository,
            IAEReportingRepository aEReportingRepository,
            IProjectRepository projectRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IRandomizationRepository randomizationRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDocumentReviewRepository projectDocumentReviewRepository,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser
            )
        {
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _manageMonitoringReportReviewRepository = manageMonitoringReportReviewRepository;
            _aEReportingRepository = aEReportingRepository;
            _projectRepository=projectRepository;
            _screeningVisitRepository=screeningVisitRepository;
            _randomizationRepository=randomizationRepository;
            _screeningTemplateRepository=screeningTemplateRepository;
            _screeningTemplateValueRepository=screeningTemplateValueRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _screeningEntryRepository=screeningEntryRepository;
            _projectDocumentReviewRepository= projectDocumentReviewRepository;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
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
            objdashboard.manageMonitoringReportSendData = _manageMonitoringReportReviewRepository.GetSendTemplateList((int)(SiteId != null ? SiteId : ProjectId));
            objdashboard.manageMonitoringReportSendBackData = _manageMonitoringReportReviewRepository.GetSendBackTemplateList((int)(SiteId != null ? SiteId : ProjectId));
            return Ok(objdashboard);
        }


        [HttpGet]
        [Route("GetDashboardMyTaskList/{ProjectId}/{SiteId:int?}")]
        public IActionResult GetDashboardMyTaskList(int ProjectId, int? SiteId)
        {
            DashboardMyTaskDto objdashboard = new DashboardMyTaskDto()
            {
                MyTaskList=new List<DashboardDto>()
            };
            objdashboard.MyTaskList.AddRange(_projectArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectWorkplaceArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectWorkplaceArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_projectSubSecArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId));
            objdashboard.MyTaskList.AddRange(_econsentReviewDetailsRepository.GetEconsentMyTaskList(ProjectId));
            objdashboard.MyTaskList.AddRange(_aEReportingRepository.GetAEReportingMyTaskList(ProjectId, (int)(SiteId != null ? SiteId : ProjectId)));
            objdashboard.MyTaskList.AddRange(_manageMonitoringReportReviewRepository.GetSendTemplateList((int)(SiteId != null ? SiteId : ProjectId)));
            objdashboard.MyTaskList.AddRange(_manageMonitoringReportReviewRepository.GetSendBackTemplateList((int)(SiteId != null ? SiteId : ProjectId)));
            return Ok(objdashboard);
        }

        [HttpGet]
        [Route("GetDashboardVisitGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardVisitGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var screeningVisits = _screeningVisitRepository.All
                .Include(x => x.ProjectDesignVisit).Include(i => i.ScreeningEntry).Where(x => projectIds.Contains(x.ScreeningEntry.ProjectId) && (!x.ScreeningEntry.Project.IsTestSite) && x.DeletedDate == null).ToList()
                .GroupBy(g => g.ProjectDesignVisitId).Select(s => new
                {
                    Name = _context.ProjectDesignVisit.FirstOrDefault(m => m.Id==s.Key).DisplayName,
                    NotStarted = s.Where(q => q.Status==ScreeningVisitStatus.NotStarted).Count(),
                    Missed = s.Where(q => q.Status==ScreeningVisitStatus.Missed).Count(),
                    OnHold = s.Where(q => q.Status==ScreeningVisitStatus.OnHold).Count(),
                    Completed = s.Where(q => q.Status==ScreeningVisitStatus.Completed).Count(),
                });

            return Ok(screeningVisits);
        }

        [HttpGet]
        [Route("ScreenedToRandomizedGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult ScreenedToRandomizedGraph(int projectId, int countryId, int siteId)
        {
            var projectList = GetProjectIds(projectId, countryId, siteId);

            var DaysDiffList = new List<DashboardDaysScreenedToRandomized>();

            foreach (var project in projectList)
            {
                var randomizes = _randomizationRepository.All.Where(q => q.ProjectId==project.Id && q.DeletedDate==null && q.DateOfRandomization!=null && q.DateOfScreening!=null).ToList()
                    .Select(s => new
                    {
                        DayDiff = s.DateOfRandomization.Value.Subtract(s.DateOfScreening.Value).Days,
                        ProjectId = s.ProjectId
                    }).ToList().GroupBy(g => g.ProjectId)
                    .Select(m => new DashboardDaysScreenedToRandomized
                    {
                        AvgDayDiff = m.Sum(s => s.DayDiff)/m.Count(),
                        SiteName=project.ProjectCode,
                        SiteId = m.Key
                    });

                DaysDiffList.AddRange(randomizes);
            }
            return Ok(DaysDiffList);
        }

        [HttpGet]
        [Route("GetRandomizedProgressGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetRandomizedProgressGraph(int projectId, int countryId, int siteId)
        {

            var projectList = GetProjectIds(projectId, countryId, siteId);

            var progresses = new List<RandomizedProgress>();
            foreach (var project in projectList)
            {
                var randomizesSubject = _randomizationRepository.All.Where(q => q.ProjectId==project.Id && q.DeletedDate==null && q.DateOfRandomization!=null).Count();
                var totalSubject = _context.Randomization.Where(q => q.ProjectId==project.Id && q.DeletedDate==null).Count();
                if (totalSubject>0)
                {
                    var percentage = randomizesSubject*100/totalSubject;

                    progresses.Add(new RandomizedProgress()
                    {
                        Progress=percentage,
                        SiteId=project.Id,
                        SiteName=project.ProjectCode
                    });
                }
                else
                {
                    progresses.Add(new RandomizedProgress()
                    {
                        Progress=0,
                        SiteId=project.Id,
                        SiteName=project.ProjectCode
                    });
                }
            }

            return Ok(progresses);
        }

        [HttpGet]
        [Route("GetDashboardQueryGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardQueryGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var queries = _screeningTemplateValueRepository.All.Where(r => projectIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
            (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) &&
            r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null).
                 GroupBy(c => new
                 {
                     c.QueryStatus
                 }).Select(t => new DashboardQueryStatusDto
                 {
                     Status = t.Key.QueryStatus,
                     DisplayName = t.Key.QueryStatus.GetDescription(),
                     Total = t.Count()
                 }).ToList().OrderBy(x => x.Status).ToList();

            var closeQueries = _screeningTemplateValueQueryRepository.All.Count(r => projectIds.Contains(r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
            r.QueryStatus == QueryStatus.Closed &&
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null && r.ScreeningTemplateValue.DeletedDate == null);

            queries.Where(x => x.DisplayName == QueryStatus.Closed.GetDescription()).OrderBy(x => x.Status).ToList().ForEach(x => x.Total = closeQueries);


            if (!queries.Any(x => x.DisplayName == QueryStatus.Closed.GetDescription()))
                queries.Add(new DashboardQueryStatusDto
                {
                    DisplayName = QueryStatus.Closed.GetDescription(),
                    Total = closeQueries
                });

            var list = new List<string>() { "Open", "Resolved", "Answered", "Closed" };

            return Ok(queries.Where(q => list.Contains(q.DisplayName)));
        }

        [HttpGet]
        [Route("GetDashboardLabelGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardLabelGraph(int projectId, int countryId, int siteId)
        {
            var projectList = GetProjectIds(projectId, countryId, siteId);

            var labelGraphs = new List<LabelGraph>();

            foreach (var project in projectList)
            {
                var randomizeCount = _randomizationRepository.All.Where(x => x.DateOfRandomization!=null && x.DeletedDate==null && x.ProjectId==project.Id).Count();
                var screeningCount = _randomizationRepository.All.Where(x => x.DateOfScreening!=null && x.DeletedDate==null && x.ProjectId==project.Id).Count();

                labelGraphs.Add(new LabelGraph()
                {
                    RandomizedCount=randomizeCount,
                    ScreeningCount =screeningCount,
                    TargetCount=project.AttendanceLimit.Value,
                    SiteId=project.Id,
                    SiteName=project.ProjectCode
                });
            }
            return Ok(labelGraphs);
        }

        [HttpGet]
        [Route("GetDashboardFormsGraph/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetDashboardFormsGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var projectDesign = _context.ProjectDesign.FirstOrDefault(x => x.ProjectId==projectId && x.DeletedDate==null);

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesign.Id);

            var screnningTemplates = _screeningTemplateRepository.All.Include(x => x.ScreeningVisit)
                .Include(x => x.ScreeningVisit.ScreeningEntry).Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId) && (!x.ScreeningVisit.ScreeningEntry.Project.IsTestSite) && x.DeletedDate == null && x.ScreeningVisit.DeletedDate==null);

            var formGrpah = new List<FormsGraphModel>();

            foreach (var item in workflowlevel.WorkFlowText)
            {
                var level = new FormsGraphModel()
                {
                    StatusName=item.RoleName,
                    RecordCount=screnningTemplates.Where(s => s.ReviewLevel==item.LevelNo).Count()
                };

                formGrpah.Add(level);
            }

            formGrpah.Add(
                new FormsGraphModel()
                {
                    StatusName="Submitted",
                    RecordCount=screnningTemplates.Where(s => s.Status== ScreeningTemplateStatus.Submitted).Count()
                });
            formGrpah.Add(
                new FormsGraphModel()
                {
                    StatusName="In Progress",
                    RecordCount=screnningTemplates.Where(s => s.Status== ScreeningTemplateStatus.InProcess).Count()
                });
            formGrpah.Add(
                new FormsGraphModel()
                {
                    StatusName="Not Started",
                    RecordCount=screnningTemplates.Where(s => s.Status== ScreeningTemplateStatus.Pending).Count()
                });
            return Ok(formGrpah);
        }


        private List<Data.Entities.Master.Project> GetProjectIds(int projectId, int countryId, int siteId)
        {
            var projectIds = new List<Data.Entities.Master.Project>();

            if (countryId==0 && siteId==0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                           && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                           && a.UserId == _jwtTokenAccesser.UserId
                                                           && a.RoleId == _jwtTokenAccesser.RoleId
                                                           && a.DeletedDate == null
                                                           && a.RollbackReason == null)
                                                           && x.DeletedDate == null).ToList();
            }
            else if (countryId>0 && siteId==0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                          && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                          && a.UserId == _jwtTokenAccesser.UserId
                                                          && a.RoleId == _jwtTokenAccesser.RoleId
                                                          && a.DeletedDate == null
                                                          && a.RollbackReason == null)
                                                          && x.ManageSite.City.State.CountryId==countryId
                                                          && x.DeletedDate == null).ToList();
            }
            else
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                         && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                         && a.UserId == _jwtTokenAccesser.UserId
                                                         && a.RoleId == _jwtTokenAccesser.RoleId
                                                         && a.DeletedDate == null
                                                         && a.RollbackReason == null)
                                                         && x.ManageSite.City.State.CountryId==countryId
                                                         && x.Id==siteId
                                                         && x.DeletedDate == null).ToList();
            }

            return projectIds;
        }

        #endregion

        #region Dashboard Query Management Code

        [HttpGet]
        [Route("GetQueryManagementTotalQueryStatus/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetQueryManagementTotalQueryStatus(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var queries = _screeningTemplateValueRepository.All.Where(r => projectIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
            (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) &&
            r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null).
                 GroupBy(c => new
                 {
                     c.QueryStatus
                 }).Select(t => new DashboardQueryStatusDto
                 {
                     Status = t.Key.QueryStatus,
                     DisplayName = t.Key.QueryStatus.GetDescription(),
                     Total = t.Count()
                 }).ToList().OrderBy(x => x.Status).ToList();

            var closeQueries = _screeningTemplateValueQueryRepository.All.Count(r => projectIds.Contains(r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
            r.QueryStatus == QueryStatus.Closed &&
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null && r.ScreeningTemplateValue.DeletedDate == null);

            queries.Where(x => x.DisplayName == QueryStatus.Closed.GetDescription()).OrderBy(x => x.Status).ToList().ForEach(x => x.Total = closeQueries);


            if (!queries.Any(x => x.DisplayName == QueryStatus.Closed.GetDescription()))
                queries.Add(new DashboardQueryStatusDto
                {
                    DisplayName = QueryStatus.Closed.GetDescription(),
                    Total = closeQueries
                });

            return Ok(queries.Where(x => !string.IsNullOrEmpty(x.DisplayName)));
        }

        [HttpGet]
        [Route("GetQueryManagementVisitWiseQuery/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetQueryManagementVisitWiseQuery(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var queries = _screeningTemplateValueRepository.All.Where(r => projectIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
                (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) && r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null).
                Select(s => new
                {
                    VisitName = s.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DisplayName,
                    VisitId = s.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId,
                    Query = s
                }).AsEnumerable().GroupBy(g => g.VisitId).Select(s => new
                {
                    VisitName = s.FirstOrDefault(q => q.VisitId==s.Key).VisitName,
                    VisitId = s.Key,
                    Open = s.Where(q => q.Query.QueryStatus== QueryStatus.Open).Count(),
                    Answered = s.Where(q => q.Query.QueryStatus== QueryStatus.Answered).Count(),
                    Resolved = s.Where(q => q.Query.QueryStatus== QueryStatus.Resolved).Count(),
                    Reopened = s.Where(q => q.Query.QueryStatus== QueryStatus.Reopened).Count(),
                    Closed = s.Where(q => q.Query.QueryStatus== QueryStatus.Closed).Count(),
                    SelfCorrection = s.Where(q => q.Query.QueryStatus== QueryStatus.SelfCorrection).Count(),
                    Acknowledge = s.Where(q => q.Query.QueryStatus== QueryStatus.Acknowledge).Count(),
                });


            return Ok(queries);
        }

        [HttpGet]
        [Route("GetQueryManagementRoleWiseQuery/{ProjectId}/{countryId}/{siteId}")]
        public IActionResult GetQueryManagementRoleWiseQuery(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var result = _screeningTemplateValueQueryRepository.All.Where(x => projectIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) && (!x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite)
              && (x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.SelfCorrection
              || x.QueryStatus == QueryStatus.Acknowledge) && x.UserRole != null).GroupBy(
              t => new { t.UserRole, t.QueryStatus }).Select(g => new DashboardQueryStatusDto
              {
                  DisplayName = g.Key.UserRole,
                  QueryStatus = g.Key.QueryStatus.GetDescription(),
                  Total = g.Count()
              }).ToList();

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
    }
}