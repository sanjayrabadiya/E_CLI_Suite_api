﻿using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.CTMS;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Respository.Screening;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Master
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IProjectDocumentReviewRepository _projectDocumentReviewRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly ICtmsActivityRepository _CtmsActivityRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMetricsRepository _metricsRepository;


        public DashboardRepository(IJwtTokenAccesser jwtTokenAccesser, IScreeningVisitRepository screeningVisitRepository,
            IRandomizationRepository randomizationRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
             ICtmsActivityRepository ctmsActivityRepository,
            IProjectDesignRepository projectDesignRepository,
            IProjectRepository projectRepository,
            IProjectRightRepository projectRightRepository,
            IProjectDocumentReviewRepository projectDocumentReviewRepository,
            IGSCContext context,
            IMetricsRepository metricsRepository)
        {
            _screeningVisitRepository = screeningVisitRepository;
            _randomizationRepository = randomizationRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _projectDocumentReviewRepository = projectDocumentReviewRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _projectDesignRepository = projectDesignRepository;
            _projectRepository = projectRepository;
            _projectRightRepository = projectRightRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _metricsRepository = metricsRepository;
        }


        public dynamic GetDashboardPatientStatus(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            int total = 0;
            if (countryId == 0 && siteId == 0)
            {
                var project = _projectRepository.All.Where(x => projectIds.Contains(x.Id) && (siteId == 0 ? (!x.IsTestSite) : true)).ToList();
                total = (int)project.Sum(item => item.AttendanceLimit);


            }
            else if (countryId > 0 && siteId == 0)
            {
                var project = _projectRepository.All.Include(x => x.ManageSite).Where(x => projectIds.Contains(x.Id) && (siteId == 0 ? (!x.IsTestSite) : true)
                                                          && x.ManageSite.City.State.CountryId == countryId
                                                          && x.DeletedDate == null).ToList();
                total = (int)project.Sum(item => item.AttendanceLimit);
            }
            else
            {
                var project = _projectRepository.All.Include(x => x.ManageSite).Where(x => projectIds.Contains(x.Id)
                                                         && x.ManageSite.City.State.CountryId == countryId
                                                         && x.Id == siteId
                                                         && x.DeletedDate == null).ToList();
                total = (int)project.Sum(item => item.AttendanceLimit);
            }

            var patientStatus = _randomizationRepository.All
                .Include(x => x.Project).Where(x => projectIds.Contains(x.Project.Id) && (siteId == 0 ? (!x.Project.IsTestSite) : true) && x.DeletedDate == null).ToList()
                .GroupBy(g => g.Project.ParentProjectId).Select(s => new
                {
                    parent = s.Key,
                    EnrolledTotal = total,
                    PreScreeened = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.PreScreening || (int)q.PatientStatusId > 1).Count(),
                    Screened = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.Screening || (int)q.PatientStatusId > 2).Count(),
                    Ontrial = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.OnTrial).Count(),
                    Randomized = s.Where(q => q.RandomizationNumber != null).Count(),
                    ScreeningFailure = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.ScreeningFailure).Count(),
                    Withdrawal = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.Withdrawal).Count(),
                });

            return patientStatus;
        }



        public dynamic GetDashboardVisitGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var screeningVisits = _screeningVisitRepository.All
                .Include(x => x.ProjectDesignVisit).Include(i => i.ScreeningEntry).Where(x => projectIds.Contains(x.ScreeningEntry.ProjectId) && (siteId == 0 ? (!x.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).ToList()
                .GroupBy(g => g.ProjectDesignVisitId).Select(s => new
                {
                    Name = _projectDesignVisitRepository.All.FirstOrDefault(m => m.Id == s.Key).DisplayName,
                    NotStarted = s.Where(q => q.Status == ScreeningVisitStatus.NotStarted).Count(),
                    Missed = s.Where(q => q.Status == ScreeningVisitStatus.Missed).Count(),
                    OnHold = s.Where(q => q.Status == ScreeningVisitStatus.OnHold).Count(),
                    Completed = s.Where(q => q.Status == ScreeningVisitStatus.Completed).Count(),
                });

            return screeningVisits;
        }
        public dynamic GetDashboardMonitoringReportGrid(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var list = _context.CtmsMonitoringReport
               .Include(z => z.CtmsMonitoring)
               .ThenInclude(i => i.Project)
               .Where(z => projectIds.Contains(z.CtmsMonitoring.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(z.CtmsMonitoring.StudyLevelFormId)
                && (siteId == 0 ? (!z.CtmsMonitoring.Project.IsTestSite) : true) && z.DeletedDate == null)
               .Select(b => new CtmsMonitoringPlanDashoardDto
               {
                   Id = b.Id,
                   Site = b.CtmsMonitoring.Project.ProjectCode,
                   Activity = b.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                   Country = b.CtmsMonitoring.Project.ManageSite.City.State.Country.CountryName,
                   Status = b.ReportStatus.ToString(),
               }).ToList();

            return list;
        }

        public dynamic GetDashboardMonitoringReportGraph(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();
            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();
            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var list = _context.CtmsMonitoringReport
                .Include(z => z.CtmsMonitoring)
                .ThenInclude(i => i.StudyLevelForm)
                .ThenInclude(i => i.Activity)
                .ThenInclude(i => i.CtmsActivity)
                .Where(z => projectIds.Contains(z.CtmsMonitoring.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(z.CtmsMonitoring.StudyLevelFormId)
                 && (siteId == 0 ? (!z.CtmsMonitoring.Project.IsTestSite) : true) && z.DeletedDate == null).ToList()
                .GroupBy(x => new { x.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName, x.ReportStatus })
                .Select(s => new
                {
                    name = s.Key.ActivityName,
                    NotInitiated = s.Where(q => q.ReportStatus == MonitoringReportStatus.OnGoing).Count(),
                    SendForReview = s.Where(q => q.ReportStatus == MonitoringReportStatus.UnderReview).Count(),
                    QueryGenerated = s.Where(q => q.ReportStatus == MonitoringReportStatus.ReviewInProgress).Count(),
                    FormApproved = s.Where(q => q.ReportStatus == MonitoringReportStatus.Approved).Count(),
                });
            return list;
        }

        public dynamic ScreenedToRandomizedGraph(int projectId, int countryId, int siteId)
        {
            var projectList = GetProjectIds(projectId, countryId, siteId);
            var DaysDiffList = new List<DashboardDaysScreenedToRandomized>();

            foreach (var project in projectList)
            {
                var randomizes = _randomizationRepository.All.Where(q => q.ProjectId == project.Id && (siteId == 0 ? (!q.Project.IsTestSite) : true) && q.DeletedDate == null && q.DateOfRandomization != null && q.DateOfScreening != null).ToList()
                    .Select(s => new
                    {
                        DayDiff = s.DateOfRandomization.Value.Subtract(s.DateOfScreening.Value).Days,
                        ProjectId = s.ProjectId
                    }).ToList().GroupBy(g => g.ProjectId)
                    .Select(m => new DashboardDaysScreenedToRandomized
                    {
                        AvgDayDiff = (m.Sum(s => s.DayDiff)),
                        SiteName = project.ProjectCode,
                        SiteId = m.Key
                    });

                DaysDiffList.AddRange(randomizes);
            }

            DaysDiffList.ForEach(t =>
            {
                t.AvgDayDiff = decimal.Round((t.AvgDayDiff / _randomizationRepository.All.Where(q => q.ProjectId == t.SiteId && q.DeletedDate == null && q.DateOfScreening != null).Count()), 2, MidpointRounding.AwayFromZero);
            });
            return DaysDiffList;
        }
        public dynamic GetRandomizedProgressGraph(int projectId, int countryId, int siteId)
        {

            var projectList = GetProjectIds(projectId, countryId, siteId);

            var progresses = new List<RandomizedProgress>();
            foreach (var project in projectList)
            {
                var randomizesSubject = _randomizationRepository.All.Where(q => q.ProjectId == project.Id && q.DeletedDate == null && q.DateOfRandomization != null).Count();
                var totalSubject = _randomizationRepository.All.Where(q => q.ProjectId == project.Id && q.DeletedDate == null).Count();
                if (totalSubject > 0)
                {
                    var percentage = randomizesSubject * 100 / totalSubject;

                    progresses.Add(new RandomizedProgress()
                    {
                        Progress = percentage,
                        SiteId = project.Id,
                        SiteName = project.ProjectCode
                    });
                }
                else
                {
                    progresses.Add(new RandomizedProgress()
                    {
                        Progress = 0,
                        SiteId = project.Id,
                        SiteName = project.ProjectCode
                    });
                }
            }

            return progresses;
        }

        public dynamic GetDashboardQueryGraph(int projectId, int countryId, int siteId)
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


            if (closeQueries > 0)
            {
                if (!queries.Any(x => x.DisplayName == QueryStatus.Closed.GetDescription()))
                    queries.Add(new DashboardQueryStatusDto
                    {
                        DisplayName = QueryStatus.Closed.GetDescription(),
                        Total = closeQueries
                    });
            }

            var list = new List<string>() { "Open", "Resolved", "Answered", "Closed" };

            return queries.Where(q => list.Contains(q.DisplayName));
        }


        public dynamic GetDashboardLabelGraph(int projectId, int countryId, int siteId)
        {
            var projectList = GetProjectIds(projectId, countryId, siteId);

            var labelGraphs = new List<LabelGraph>();

            foreach (var project in projectList)
            {
                var randomizeCount = _randomizationRepository.All.Where(x => x.DateOfRandomization != null && x.DeletedDate == null && x.ProjectId == project.Id && (siteId == 0 ? (!x.Project.IsTestSite) : true)).Count();
                var screeningCount = _randomizationRepository.All.Where(x => x.DateOfScreening != null && x.DeletedDate == null && x.ProjectId == project.Id && (siteId == 0 ? (!x.Project.IsTestSite) : true)).Count();

                var isTestSite = _projectRepository.Find(project.Id).IsTestSite;

                if ((!isTestSite && siteId == 0) || ((isTestSite && siteId != 0)))
                {
                    labelGraphs.Add(new LabelGraph()
                    {
                        RandomizedCount = randomizeCount,
                        ScreeningCount = screeningCount,
                        TargetCount = project.AttendanceLimit.Value,
                        SiteId = project.Id,
                        SiteName = project.ProjectCode
                    });
                }

                else if (!isTestSite && siteId != 0)
                {
                    labelGraphs.Add(new LabelGraph()
                    {
                        RandomizedCount = randomizeCount,
                        ScreeningCount = screeningCount,
                        TargetCount = project.AttendanceLimit.Value,
                        SiteId = project.Id,
                        SiteName = project.ProjectCode
                    });
                }
            }
            return labelGraphs;
        }
        public dynamic GetDashboardFormsGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var projectDesign = _projectDesignRepository.All.FirstOrDefault(x => x.ProjectId == projectId && x.DeletedDate == null);

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesign.Id);

            var screnningTemplates = _screeningTemplateRepository.All.Include(x => x.ScreeningVisit)
                .Include(x => x.ScreeningVisit.ScreeningEntry).Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId) && (siteId == 0 ? (!x.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null && x.ScreeningVisit.DeletedDate == null);

            var formGrpah = new List<FormsGraphModel>();

            foreach (var item in workflowlevel.WorkFlowText)
            {
                var level = new FormsGraphModel()
                {
                    StatusName = item.RoleName,
                    RecordCount = screnningTemplates.Where(s => s.ReviewLevel == item.LevelNo).Count()
                };

                formGrpah.Add(level);
            }

            formGrpah.Add(
                new FormsGraphModel()
                {
                    StatusName = "Submitted",
                    RecordCount = screnningTemplates.Where(s => s.Status == ScreeningTemplateStatus.Submitted).Count()
                });
            formGrpah.Add(
                new FormsGraphModel()
                {
                    StatusName = "In Progress",
                    RecordCount = screnningTemplates.Where(s => s.Status == ScreeningTemplateStatus.InProcess).Count()
                });
            formGrpah.Add(
                new FormsGraphModel()
                {
                    StatusName = "Not Started",
                    RecordCount = screnningTemplates.Where(s => s.Status == ScreeningTemplateStatus.Pending).Count()
                });

            return formGrpah;
        }


        private List<Data.Entities.Master.Project> GetProjectIds(int projectId, int countryId, int siteId)
        {
            var projectIds = new List<Data.Entities.Master.Project>();

            if (countryId == 0 && siteId == 0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                           && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                           && a.UserId == _jwtTokenAccesser.UserId
                                                           && a.RoleId == _jwtTokenAccesser.RoleId
                                                           && a.DeletedDate == null
                                                           && a.RollbackReason == null)
                                                           && x.DeletedDate == null).ToList();
            }
            else if (countryId > 0 && siteId == 0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                          && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                          && a.UserId == _jwtTokenAccesser.UserId
                                                          && a.RoleId == _jwtTokenAccesser.RoleId
                                                          && a.DeletedDate == null
                                                          && a.RollbackReason == null)
                                                          && x.ManageSite.City.State.CountryId == countryId
                                                          && x.DeletedDate == null).ToList();
            }
            else
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                         && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                         && a.UserId == _jwtTokenAccesser.UserId
                                                         && a.RoleId == _jwtTokenAccesser.RoleId
                                                         && a.DeletedDate == null
                                                         && a.RollbackReason == null)
                                                         && x.ManageSite.City.State.CountryId == countryId
                                                         && x.Id == siteId
                                                         && x.DeletedDate == null).ToList();
            }

            return projectIds;
        }



        public dynamic GetQueryManagementTotalQueryStatus(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var queries = _screeningTemplateValueRepository.All.Where(r => projectIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
           && (siteId == 0 ? (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) &&
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
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null && r.ScreeningTemplateValue.DeletedDate == null
             && (siteId == 0 ? (!r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true));

            queries.Where(x => x.DisplayName == QueryStatus.Closed.GetDescription()).OrderBy(x => x.Status).ToList().ForEach(x => x.Total = closeQueries);

            if (!queries.Any(x => x.DisplayName == QueryStatus.Closed.GetDescription()))
                queries.Add(new DashboardQueryStatusDto
                {
                    DisplayName = QueryStatus.Closed.GetDescription(),
                    Total = closeQueries
                });

            return queries.Where(x => !string.IsNullOrEmpty(x.DisplayName));
        }

        public dynamic GetQueryManagementVisitWiseQuery(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var queries = _screeningTemplateValueRepository.All.Where(r => projectIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
                (siteId == 0 ? (!r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && r.ProjectDesignVariable.DeletedDate == null && r.DeletedDate == null).
                Select(s => new
                {
                    VisitName = s.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DisplayName,
                    VisitId = s.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId,
                    Query = s
                }).AsEnumerable().GroupBy(g => g.VisitId).Select(s => new
                {
                    VisitName = s.FirstOrDefault(q => q.VisitId == s.Key).VisitName,
                    VisitId = s.Key,
                    Open = s.Where(q => q.Query.QueryStatus == QueryStatus.Open).Count(),
                    Answered = s.Where(q => q.Query.QueryStatus == QueryStatus.Answered).Count(),
                    Resolved = s.Where(q => q.Query.QueryStatus == QueryStatus.Resolved).Count(),
                    Reopened = s.Where(q => q.Query.QueryStatus == QueryStatus.Reopened).Count(),
                    Closed = s.Where(q => q.Query.QueryStatus == QueryStatus.Closed).Count(),
                    SelfCorrection = s.Where(q => q.Query.QueryStatus == QueryStatus.SelfCorrection).Count(),
                    Acknowledge = s.Where(q => q.Query.QueryStatus == QueryStatus.Acknowledge).Count(),
                });


            return queries;
        }

        public dynamic GetQueryManagementRoleWiseQuery(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var result = _screeningTemplateValueQueryRepository.All.Where(x => projectIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
            (siteId == 0 ? (!x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true)
              && (x.QueryStatus == QueryStatus.Open || x.QueryStatus == QueryStatus.SelfCorrection
              || x.QueryStatus == QueryStatus.Acknowledge) && x.UserRole != null).GroupBy(
              t => new { t.UserRole, t.QueryStatus }).Select(g => new DashboardQueryStatusDto
              {
                  DisplayName = g.Key.UserRole,
                  QueryStatus = g.Key.QueryStatus.GetDescription(),
                  Total = g.Count()
              }).ToList();

            return result;
        }

        public dynamic getVisitStatuschart(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var list = _context.CtmsMonitoringReportReview
                .Include(x => x.CtmsMonitoringReport)
                .ThenInclude(x => x.CtmsMonitoring)
                .ThenInclude(x => x.Project)
                .ThenInclude(x => x.ManageSite)
                .Where(x => projectIds.Contains(x.CtmsMonitoringReport.CtmsMonitoring.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoringReport.CtmsMonitoring.StudyLevelFormId)
                            && (siteId == 0 ? (!x.CtmsMonitoringReport.CtmsMonitoring.Project.IsTestSite) : true)
                            && x.DeletedDate == null)
                .Select(b => new CtmsMonitoringPlanDashoardDto
                {
                    Id = b.Id,
                    ScheduleStartDate = b.CtmsMonitoringReport.CtmsMonitoring.ScheduleStartDate,
                    SchedulEndtDate = b.CtmsMonitoringReport.CtmsMonitoring.ScheduleEndDate,
                    ActualStartDate = b.CtmsMonitoringReport.CtmsMonitoring.ActualStartDate,
                    ActualEndDate = b.CtmsMonitoringReport.CtmsMonitoring.ActualEndDate,
                }).ToList();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    item.visitStatus = GetVisitgStatus(item);
                }
            }
            var groupedCustomerList = list
            .GroupBy(u => u.visitStatus)
            .Select(b => new CtmsMonitoringPlanDashoardDto
            {
                DisplayName = b.Key,
                Total = b.Count()
            }).ToList();
            return groupedCustomerList;
        }

        public List<DashboardQueryGraphFinalDto> GetNewDashboardQueryGraphData(int projectId, int countryId, int siteId)
        {
            try
            {
                var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
                List<DashboardQueryGraphDto> list = new List<DashboardQueryGraphDto>();
                List<DashboardQueryGraphFinalDto> finallist = new List<DashboardQueryGraphFinalDto>();
                var query = _screeningTemplateValueQueryRepository.All.Where(x => projectIds.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) &&
            (siteId == 0 ? (!x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true)
                && (x.QueryStatus == Helper.QueryStatus.Open || x.QueryStatus == Helper.QueryStatus.Closed)
               ).OrderBy(x => x.CreatedDate).ToList();
                if (query != null && query.Count > 0)
                {
                    var opendata = query.Where(x => x.QueryStatus == Helper.QueryStatus.Open).ToList();

                    foreach (var item in opendata)
                    {
                        var slist = new List<ScreeningTemplateValueQuery>();
                        var data1 = FindClosedData(item, slist);
                        if (data1 != null && data1.Count > 0)
                        {
                            var finaldata = data1.Where(x => x != null && x.QueryStatus == QueryStatus.Closed).FirstOrDefault();
                            if (finaldata != null)
                            {
                                DashboardQueryGraphDto obj = new DashboardQueryGraphDto();
                                obj.ScreeningTemplateValueId = finaldata.ScreeningTemplateValueId;
                                double week = ((DateTime)finaldata.CreatedDate - (DateTime)item.CreatedDate).TotalDays / 7;
                                obj.Lable = Convert.ToInt32(week + 1) + " Week";
                                obj.Id = item.Id;
                                obj.week = Convert.ToInt32(week + 1);
                                list.Add(obj);
                            }
                        }
                    }
                    if (list != null && list.Count > 0)
                    {
                        finallist = list.OrderBy(x => x.week).GroupBy(x => x.Lable).Select(x => new DashboardQueryGraphFinalDto
                        {
                            Lable = x.Key,
                            Count = x.ToList().Count
                        }).ToList();
                    }

                }

                return finallist;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public List<ScreeningTemplateValueQuery> FindClosedData(ScreeningTemplateValueQuery obj, List<ScreeningTemplateValueQuery> list)
        {
            list.Add(obj);
            var data = _screeningTemplateValueQueryRepository.All.Where(x => x.QueryParentId == obj.Id).FirstOrDefault();
            if (data != null && data.QueryStatus != QueryStatus.Closed)
            {
                FindClosedData(data, list);
            }
            else
            {
                list.Add(data);
            }

            return list.ToList();
        }

        public DashboardInformConsentStatusDto GetDashboardInformConsentCount(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var randomizations = _randomizationRepository.All.Include(c => c.EconsentReviewDetails).Where(x => x.DeletedDate == null && projectIds.Contains(x.ProjectId)).ToList();

            var result = new DashboardInformConsentStatusDto();

            result.TotalRandomization = randomizations.Count();
            result.PreScreening = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.PreScreening || (int)x.PatientStatusId > 1).Count();
            result.Screened = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.Screening).Count();
            result.ConsentInProgress = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.ConsentInProcess).Count();
            result.ConsentCompleted = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.ConsentCompleted || ((int)x.PatientStatusId > 4 && (int)x.PatientStatusId < 10 && (int)x.PatientStatusId != 5)).Count();
            result.ReConsent = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess).Count();
            result.ConsentWithdraw = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.Withdrawal).Count();

            return result;

        }

        public List<DashboardInformConsentStatusDto> GetDashboardInformConsentChart(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var randomizations = _randomizationRepository.All.Include(c => c.EconsentReviewDetails).Where(x => x.DeletedDate == null && projectIds.Contains(x.ProjectId)).ToList();

            var statusList = Enum.GetValues(typeof(InformConsentChart))
               .Cast<InformConsentChart>().Select(e => new DropDownEnum
               {
                   Id = Convert.ToInt16(e),
                   Value = e.GetDescription()
               }).OrderBy(o => o.Id).ToList();

            var result = new List<DashboardInformConsentStatusDto>();
            statusList.ForEach(x =>
            {
                var obj = new DashboardInformConsentStatusDto();
                obj.DisplayName = x.Value;
                obj.Total = randomizations.Where(v => (x.Id == 1 ? v.PatientStatusId == ScreeningPatientStatus.Screening
                                                : x.Id == 2 ? v.PatientStatusId == ScreeningPatientStatus.ConsentInProcess
                                                : x.Id == 3 ? (v.PatientStatusId == ScreeningPatientStatus.ConsentCompleted || ((int)v.PatientStatusId > 4 && (int)v.PatientStatusId < 10 && (int)v.PatientStatusId != 5))
                                                : x.Id == 4 ? v.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess
                                                : v.PatientStatusId == ScreeningPatientStatus.Withdrawal
                                                )).Count();
                result.Add(obj);
            });

            return result;

        }

        public dynamic GetCTMSMonitoringChart(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();


            var asd = _context.CtmsMonitoring.Where(x => projectIds.Contains(x.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(x.StudyLevelFormId)
                       && (siteId == 0 ? (!x.Project.IsTestSite) : true)
                        && x.DeletedDate == null)
                .Select(b => new
                {
                    ProjectId = b.ProjectId,
                    ProjectName = b.Project.ProjectCode,
                    ActivityName = b.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                    Status = _context.CtmsMonitoringStatus.Where(v => v.CtmsMonitoringId == b.Id).OrderByDescending(x => x.Id).FirstOrDefault().Status
                }).ToList();


            if (asd.Count > 0)
            {
                var result = asd.GroupBy(g => g.ActivityName).Select(n => new CtmsMonitoringStatusChartDto
                {
                    ActivityName = n.Key,
                    ACount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.Approved).Count(),
                    RCount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.Rejected).Count(),
                    EntrollCount = 0
                }).ToList();

                var result1 = _context.Randomization.Where(x => asd.Select(z => z.ProjectId).Contains(x.ProjectId) && x.DeletedDate == null).GroupBy(x => x.ProjectId).Select(x => x.Key).Count();
                var result3 = new CtmsMonitoringStatusChartDto
                {
                    ActivityName = "Enrolled",
                    EntrollCount = result1

                };
                result.Add(result3);


                return result;
            }
            return null;
        }

        public dynamic GetCTMSMonitoringPIChart(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();


            var asd = _context.CtmsMonitoring.Where(x => projectIds.Contains(x.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(x.StudyLevelFormId)
                        && (siteId == 0 ? (!x.Project.IsTestSite) : true)
                        && x.DeletedDate == null)
                .Select(b => new
                {
                    ProjectId = b.ProjectId,
                    ProjectName = b.Project.ProjectCode,
                    ActivityName = b.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                    Status = _context.CtmsMonitoringStatus.Where(v => v.CtmsMonitoringId == b.Id).OrderByDescending(x => x.Id).FirstOrDefault().Status
                }).ToList();


            if (asd.Count > 0)
            {
                var result = asd.GroupBy(g => g.ActivityName).Select(n => new CtmsMonitoringStatusChartDto
                {
                    ActivityName = n.Key,
                    ACount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.Approved).Count(),
                    RCount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.Rejected).Count(),
                    TerminatedCount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.Terminated).Count(),
                    OnHoldCount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.OnHold).Count(),
                    CloseOutCount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.CloseOut).Count(),
                    ActiveCount = n.ToList().Where(x => x.Status == MonitoringSiteStatus.Active).Count(),
                    EntrollCount = 0
                }).ToList();

                //Remove by mitul on 20-04-2023 (Deshbord ->site monitoring->Site Status->Enrolled
                //var result1 = _context.Randomization.Where(x => asd.Select(z => z.ProjectId).Contains(x.ProjectId) && x.DeletedDate == null).GroupBy(x => x.ProjectId).Select(x => x.Key).Count();
                //var result3 = new CtmsMonitoringStatusChartDto
                //{
                //    ActivityName = "Enrolled",
                //    EntrollCount = result1

                //};
                //result.Add(result3);
                List<CtmsMonitoringStatusPIChartDto> list = new List<CtmsMonitoringStatusPIChartDto>();
                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {

                        if (item.ActivityName == "Feasibility" || item.ActivityName == "Site Selection" || item.ActivityName == "Site Initiation")
                        {
                            if (item.ACount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " Approved " + item.ACount;
                                obj.Lable = item.ActivityName + " Approved ";
                                obj.Count = item.ACount;
                                obj.Status = "Approved";
                                list.Add(obj);
                            }
                            if (item.RCount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " Rejected " + item.RCount;
                                obj.Lable = item.ActivityName + " Rejected ";
                                obj.Count = item.RCount;
                                obj.Status = "Rejected";
                                list.Add(obj);
                            }
                            if (item.TerminatedCount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " Terminated " + item.TerminatedCount;
                                obj.Lable = item.ActivityName + " Terminated ";
                                obj.Count = item.TerminatedCount;
                                obj.Status = "Terminated";
                                list.Add(obj);
                            }
                            if (item.OnHoldCount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " OnHold " + item.OnHoldCount;
                                obj.Lable = item.ActivityName + " OnHold ";
                                obj.Count = item.OnHoldCount;
                                obj.Status = "OnHold";
                                list.Add(obj);
                            }
                            if (item.CloseOutCount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " Close Out " + item.CloseOutCount;
                                obj.Lable = item.ActivityName + " Close Out ";
                                obj.Count = item.CloseOutCount;
                                obj.Status = "CloseOut";
                                list.Add(obj);
                            }
                            if (item.ActiveCount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " Active " + item.ActiveCount;
                                obj.Lable = item.ActivityName + " Active ";
                                obj.Count = item.ActiveCount;
                                obj.Status = "Active";
                                list.Add(obj);
                            }
                        }
                        //Remove by mitul on 20-04-2023 (Deshbord ->site monitoring->Site Status->Enrolled
                        //if (item.ActivityName == "Enrolled")
                        //{
                        //    if (item.EntrollCount > 0)
                        //    {
                        //        CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                        //        obj.Text = item.ActivityName + " " + item.EntrollCount;
                        //        obj.Lable = item.ActivityName;
                        //        obj.Count = item.EntrollCount;
                        //        obj.Status = "Enrolled";
                        //        list.Add(obj);
                        //    }

                        //}

                    }
                }
                return list;
            }
            return null;
        }

        public List<CtmsMonitoringPlanDashoardDto> getCTMSMonitoringPlanDashboard(int projectId, int countryId, int siteId)
        {
            var today = DateTime.Now;
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            //Changes made by Mitul
            var list = _context.CtmsMonitoring
            .Include(i => i.Project)
            .ThenInclude(i => i.ManageSite)
            .Where(z => projectIds.Contains(z.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(z.StudyLevelFormId)
            && (siteId == 0 ? (!z.Project.IsTestSite) : true)
            && z.DeletedDate == null && z.ScheduleStartDate != null && z.ScheduleEndDate != null)
            .Select
            (b => new CtmsMonitoringPlanDashoardDto
            {
                Id = b.Id,
                Activity = b.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                ScheduleStartDate = b.ScheduleStartDate,
                SchedulEndtDate = b.ScheduleEndDate,
                ActualStartDate = b.ActualStartDate,
                ActualEndDate = b.ActualEndDate,
                Site = b.Project.ProjectCode == null ? b.Project.ManageSite.SiteName : b.Project.ProjectCode,
                Country = b.Project.ManageSite.City.State.Country.CountryName,
                sendForReviewDate = b.CreatedDate
            }).ToList();
            int Total = 0, Count = 0;
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    var monitoringReport = _context.CtmsMonitoringReport.Where(x => x.CtmsMonitoringId == item.Id).FirstOrDefault();
                    if (monitoringReport != null)
                        item.Status = monitoringReport.ReportStatus.ToString();
                    //item.Status = GetMonitoringStatus(item);
                    item.visitStatus = GetVisitgStatus(item);
                    if (item.Status == "UnderReview")
                    {
                        TimeSpan v = (TimeSpan)(today - item.sendForReviewDate);
                        Total = Total + v.Days;
                        Count++;
                    }
                }
                list[0].AvgReviewDay = Total != 0 ? (Total / Count) : 0;
            }

            return list;
        }

        //Add by mitul on 24-04-2024
        public List<CtmsActionPointGridDto> getCTMSOpenActionDashboard(int projectId, int countryId, int siteId)
        {
            var today = DateTime.Now;
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var asd = _context.CtmsActionPoint.Include(x => x.CtmsMonitoring).ThenInclude(x => x.Project).Where(x => projectIds.Contains(x.CtmsMonitoring.ProjectId)
             && (siteId == 0 ? (!x.CtmsMonitoring.Project.IsTestSite) : true)
             && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) && x.DeletedDate == null)
                .Select(b => new CtmsActionPointGridDto
                {
                    Id = b.Id,
                    Site = b.CtmsMonitoring.Project.ProjectCode,
                    Activity = b.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                    StatusName = b.Status.ToString(),
                    QueryDescription = b.QueryDescription,
                    QueryBy = b.CreatedByUser.UserName,
                    QueryDate = Convert.ToDateTime(b.CreatedDate),
                    CloseBy = b.CloseUser.UserName,
                    CloseDate = b.CloseDate
                }).ToList();
            int Total = 0, Count = 0;
            if (asd.Count > 0)
            {
                foreach (var item in asd)
                {
                    if (item.StatusName == "Open")
                    {
                        TimeSpan v = (TimeSpan)(today - item.QueryDate);
                        Total = Total + v.Days;
                        Count++;
                    }
                }
                asd[0].AvgOpenQueries = Total != 0 ? (Total / Count) : 0;
            }
            return asd;
        }

        //Add by mitul on 24-04-2024
        public List<CtmsMonitoringReportVariableValueQueryDto> getCTMSQueriesDashboard(int projectId, int countryId, int siteId)
        {
            var today = DateTime.Now;
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();


            var asd = _context.CtmsMonitoringReportVariableValueQuery.Include(x => x.CtmsMonitoringReportVariableValue).ThenInclude(x => x.CtmsMonitoringReport).ThenInclude(x => x.CtmsMonitoring).ThenInclude(x => x.Project).Where(x => projectIds.Contains(x.CtmsMonitoringReportVariableValue.CtmsMonitoringReport.CtmsMonitoring.ProjectId)
            && (siteId == 0 ? (!x.CtmsMonitoringReportVariableValue.CtmsMonitoringReport.CtmsMonitoring.Project.IsTestSite) : true)
            && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoringReportVariableValue.CtmsMonitoringReport.CtmsMonitoring.StudyLevelFormId))
              //&& x.QueryParentId != null)
              .Select(b => new CtmsMonitoringReportVariableValueQueryDto
              {
                  Id = b.Id,
                  Site = b.CtmsMonitoringReportVariableValue.CtmsMonitoringReport.CtmsMonitoring.Project.ProjectCode,
                  Activity = b.CtmsMonitoringReportVariableValue.CtmsMonitoringReport.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                  CreatedDate = b.CreatedDate,
                  ReasonName = b.Reason.ReasonName,
                  ReasonOth = b.ReasonOth,
                  StatusName = b.QueryStatus.GetDescription(),
                  Value = b.Value,
                  OldValue = b.OldValue,
                  CreatedByName = b.UserName + "(" + b.UserRole + ")",
                  QueryBy = b.CtmsMonitoringReportVariableValue.CreatedByUser.UserName,
                  Note = string.IsNullOrEmpty(b.Note) ? b.ReasonOth : b.Note,
              }).ToList();

            int Total = 0, Count = 0;
            if (asd.Count > 0)
            {
                foreach (var item in asd)
                {
                    if (item.StatusName == "Open")
                    {
                        TimeSpan v = (TimeSpan)(today - item.CreatedDate);
                        Total = Total + v.Days;
                        Count++;
                    }
                }
                asd[0].AvgOpenQueries = Total != 0 ? (Total / Count) : 0;
            }


            return asd;
        }
        //public string GetMonitoringStatus(CtmsMonitoringPlanDashoardDto obj)
        //{
        //    var data = _context.CtmsMonitoringReportReview.Include(x => x.CtmsMonitoringReport).Where(x => x.CtmsMonitoringReport.CtmsMonitoringId == obj.Id).FirstOrDefault();
        //    if (obj.ScheduleStartDate != null && data == null)
        //    {
        //        return "In Progress";
        //    }
        //    if (obj.ScheduleStartDate != null && data != null && !data.IsApproved)
        //    {
        //        return "In signature/pending review";
        //    }
        //    if (obj.ScheduleStartDate != null && data != null && data.IsApproved)
        //    {
        //        return "Final";
        //    }
        //    return "";
        //}

        public string GetVisitgStatus(CtmsMonitoringPlanDashoardDto obj)
        {
            #region visit status Add by mitul
            var today = DateTime.Now;
            if (obj.ScheduleStartDate == obj.ActualStartDate)
                return "On Track";
            else if (obj.ScheduleStartDate == obj.ActualStartDate && obj.SchedulEndtDate == obj.ActualEndDate)
                return "Completed";
            else if (obj.ScheduleStartDate > today && obj.ActualStartDate == null)
                return "Upcoming";
            else if (obj.ScheduleStartDate < obj.ActualStartDate)
                return "Overdue";
            else if (obj.ScheduleStartDate < today && obj.ActualStartDate == null)
                return "Unnoticed";
            else if (obj.ScheduleStartDate > obj.ActualStartDate)
                return "Prearranged";
            return "";
            #endregion
        }

        public dynamic getCTMSMonitoringActionPointChartDashboard(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" || x.ActivityCode == "act_004" || x.ActivityCode == "act_005" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();


            var asd = _context.CtmsActionPoint.Include(x => x.CtmsMonitoring).ThenInclude(x => x.Project).Where(x => projectIds.Contains(x.CtmsMonitoring.ProjectId)
              && (siteId == 0 ? (!x.CtmsMonitoring.Project.IsTestSite) : true)
              && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId)
                            && x.DeletedDate == null)
                .Select(b => new
                {
                    Id = b.CtmsMonitoringId,
                    Activity = b.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                    Status = b.Status
                }).ToList();

            if (asd.Count > 0)
            {
                var result = asd
                .GroupBy(c => new { Activity = c.Activity, status = c.Status })
                .Select(g => new
                {
                    Activity = g.Key.Activity,
                    Status = g.Key.status + " action of " + g.Key.Activity,
                    Count = g.ToList().Count
                }).ToList();

                return result;
            }
            return null;
        }


        //Ae Detail

        public dynamic GetDashboardAEDetail(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var CountAE = _screeningTemplateRepository.All.Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId) && (x.ProjectDesignTemplate.TemplateCode == "AE001") && x.Status > ScreeningTemplateStatus.Pending).Count();
            var Count7AE = _screeningTemplateRepository.All.Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId) && (x.ProjectDesignTemplate.TemplateCode == "AE001" && x.Status > ScreeningTemplateStatus.Pending && (x.CreatedDate <= DateTime.Today.AddDays(-7) || x.CreatedDate >= DateTime.Today.AddDays(-7)))).Count();
            var CountSAE = _screeningTemplateRepository.All.Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId) && (x.ProjectDesignTemplate.TemplateCode == "SAE001") && x.Status > ScreeningTemplateStatus.Pending).Count();
            var Count7SAE = _screeningTemplateRepository.All.Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId) && (x.ProjectDesignTemplate.TemplateCode == "SAE001" && x.Status > ScreeningTemplateStatus.Pending && (x.CreatedDate <= DateTime.Today.AddDays(-7) || x.CreatedDate >= DateTime.Today.AddDays(-7)))).Count();

            return new { CountAE, Count7AE, CountSAE, Count7SAE };
        }

        public dynamic GetDashboardAesBySeverityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var ser = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == "V003" && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
            && x.ProjectDesignVariable.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null
            && x.DeletedDate == null
            ).ToList();


            //      var ser = _context.VariableValue.Where(x => x.Variable.VariableCode == "V003").ToList();
            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
                && (x.ProjectDesignVariable.VariableCode == "V003")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new DynamicAeChartData
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    Value = r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault(),
                    Against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<DynamicAeChart>();

            foreach (var t in ser)
            {
                var r = new DynamicAeChart();
                r.SeriesName = t.ValueName;
                r.Data = GetDetails(tenoResult, t.ValueName, "001");
                result.Add(r);
            }

            return result;
        }


        public dynamic GetDashboardAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            //var ser = _context.VariableValue.Where(x => x.Variable.VariableCode == "V004").ToList();
            var ser = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == "V004" && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignVariable.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null
            && x.DeletedDate == null).ToList();

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
                && (x.ProjectDesignVariable.VariableCode == "V004")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new DynamicAeChartData
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    Value = r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault(),
                    Against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<DynamicAeChart>();


            foreach (var t in ser)
            {
                var r = new DynamicAeChart();
                r.SeriesName = t.ValueName;
                r.Data = GetDetails(tenoResult, t.ValueName, "001");
                result.Add(r);
            }

            return result;

        }

        //  public List<DynamicAeChartDetails> GetDetails(List<DynamicAeChartData> data, string ser)
        //{
        //    var types = _context.VariableValue.Where(x => x.Variable.VariableCode == "001").Select(x => x.ValueName).ToList();
        //    var r = new List<DynamicAeChartDetails>();

        //    foreach (var item in types)
        //    {
        //        var result = new DynamicAeChartDetails();
        //        result.X = item;
        //        result.Y = data.Where(e => e.Against == item && e.VariableValue == ser).ToList().Count();
        //        r.Add(result);
        //    }
        //    return r;
        //}


        public dynamic GetDashboardSAesBySeverityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var ser = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == "SAE003" && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "SAE001"
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignVariable.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null
            && x.DeletedDate == null).ToList();

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "SAE001"
                && (x.ProjectDesignVariable.VariableCode == "SAE003")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new DynamicAeChartData
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    Value = r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault(),
                    Against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "SAE001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<DynamicAeChart>();


            foreach (var t in ser)
            {
                var r = new DynamicAeChart();
                r.SeriesName = t.ValueName;
                r.Data = GetDetails(tenoResult, t.ValueName, "SAE001");
                result.Add(r);
            }

            return result;
        }


        public dynamic GetDashboardSAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var ser = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == "SAE002" && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "SAE001"
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
             && x.ProjectDesignVariable.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.DeletedDate == null
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DeletedDate == null
            && x.DeletedDate == null).ToList();

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "SAE001"
                && (x.ProjectDesignVariable.VariableCode == "SAE002")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new DynamicAeChartData
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    Value = r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault(),
                    Against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "SAE001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value) && m.DeletedDate == null).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<DynamicAeChart>();

            foreach (var t in ser)
            {
                var r = new DynamicAeChart();
                r.SeriesName = t.ValueName;
                r.Data = GetDetails(tenoResult, t.ValueName, "SAE001");
                result.Add(r);
            }
            return result;
        }


        //public List<DynamicAeChartDetails> GetAECDetails(List<DynamicAeChartData> data, string ser)
        //{
        //    var types = _context.VariableValue.Where(x => x.Variable.VariableCode == "SAE001").Select(x => x.ValueName).ToList();
        //    var r = new List<DynamicAeChartDetails>();

        //    foreach (var item in types)
        //    {
        //        var result = new DynamicAeChartDetails();
        //        result.X = item;
        //        result.Y = data.Where(e => e.Against == item && e.VariableValue == ser).ToList().Count();
        //        r.Add(result);
        //    }
        //    return r;
        //}

        public dynamic GetDashboardPatientEngagementGraph(int projectId, int countryId, int siteId, int FilterFlag)
        {
            // 0 all expired and due
            // 1 due
            // 2 expired only
            // 3 upcoming only

            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var details = _screeningTemplateRepository.All.
                Where(x => projectIds.Contains(x.ScreeningVisit.ScreeningEntry.ProjectId)
                && (x.Status == ScreeningTemplateStatus.Pending || x.Status == ScreeningTemplateStatus.InProcess)
                //  && x.ScreeningTemplateReview.OrderBy(r => r.Id).LastOrDefault().Status >= ScreeningTemplateStatus.Submitted
                && x.ScheduleDate != null &&
                (x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId != ScreeningPatientStatus.ScreeningFailure || x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId != ScreeningPatientStatus.Withdrawal)
               && (FilterFlag == 0 ? (((DateTime)x.ScheduleDate).Date == _jwtTokenAccesser.GetClientDate().Date || ((DateTime)x.ScheduleDate).Date < _jwtTokenAccesser.GetClientDate().Date)
               : FilterFlag == 1 ? ((DateTime)x.ScheduleDate).Date == _jwtTokenAccesser.GetClientDate().Date
               : FilterFlag == 2 ? (((DateTime)x.ScheduleDate).Date < _jwtTokenAccesser.GetClientDate().Date && ((DateTime)x.ScheduleDate).Date >= _jwtTokenAccesser.GetClientDate().Date.AddMonths(-1))
               : (((DateTime)x.ScheduleDate).Date > _jwtTokenAccesser.GetClientDate().Date && ((DateTime)x.ScheduleDate).Date <= _jwtTokenAccesser.GetClientDate().Date.AddMonths(1))
               )
               ).Select(r => new
               {
                   r.ScreeningVisit.ScreeningEntry.Randomization.Initial,
                   r.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                   r.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber,
                   r.ScreeningVisit.ProjectDesignVisit.DisplayName,
                   r.ProjectDesignTemplate.TemplateName,
                   r.ScheduleDate,
                   flag = ((DateTime)r.ScheduleDate).Date == _jwtTokenAccesser.GetClientDate().Date == true ? true : false
               }).ToList();

            return details;

        }

        public dynamic GetDashboardByCriticalGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var ser = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == "Cd001" && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "DV001"
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId).ToList();


            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "DV001"
                && (x.ProjectDesignVariable.VariableCode == "Cd001")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new DynamicAeChartData
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    Value = r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value)).Select(g => g.ValueName).FirstOrDefault(),
                    Against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "Dev001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value)).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<DynamicAeChart>();

            foreach (var t in ser)
            {
                var r = new DynamicAeChart();
                r.SeriesName = t.ValueName;
                r.Data = GetDetails(tenoResult, t.ValueName, "Dev001");
                result.Add(r);
            }

            return result;
        }

        public dynamic GetDashboardByDiscontinuationGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var ser = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == "Disc001" && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "Disc001"
            && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId).ToList();

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "Disc001"
                && (x.ProjectDesignVariable.VariableCode == "Disc001")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new DynamicAeChartData
                {
                    ScreeningTemplateId = r.ScreeningTemplateId,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    Value = r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value)).Select(g => g.ValueName).FirstOrDefault(),
                    Against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "DiscR001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value)).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<DynamicAeChart>();

            foreach (var t in ser)
            {
                var r = new DynamicAeChart();
                r.SeriesName = t.ValueName;
                r.Data = GetDetails(tenoResult, t.ValueName, "DiscR001");
                result.Add(r);
            }

            return result;
        }

        public List<DynamicAeChartDetails> GetDetails(List<DynamicAeChartData> data, string ser, string vCode)
        {
            var types = _context.ProjectDesignVariableValue.Where(x => x.ProjectDesignVariable.VariableCode == vCode && x.DeletedDate == null).Select(x => x.ValueName).ToList();
            var r = new List<DynamicAeChartDetails>();

            foreach (var item in types)
            {
                var result = new DynamicAeChartDetails();
                result.X = item;
                result.Y = data.Where(e => e.Against == item && e.VariableValue == ser).ToList().Count();
                r.Add(result);
            }
            return r;
        }
        #region //Added Graphs Of Subject Recruitment in Site Monitoring By Sachin On 19/06/2023
        public dynamic GetEnrolledGraph(int projectId, int countryId, int siteId)
        {
            int entotals = 0;
            if (countryId != 0 && siteId != 0)
            {
                var EnStudyproject = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
                var endata = _context.PlanMetrics.Where(x => x.ProjectId == projectId && (x.MetricsType == MetricsType.Enrolled) && x.Forecast != null && x.DeletedDate == null).FirstOrDefault();

                var ensresults = _context.OverTimeMetrics.Where(x => EnStudyproject.Contains(x.ProjectId) && x.PlanMetricsId == endata.Id && x.DeletedDate == null).ToList();

                entotals = (int)ensresults.Sum(c => c.Planned);

                var ensgraph = new List<DashboardEnrolledGraph>();

                var r = new DashboardEnrolledGraph();
                r.DisplayName = "Planned";
                r.Total = entotals * 100 / endata.Forecast;
                ensgraph.Add(r);

                var s = new DashboardEnrolledGraph();
                s.DisplayName = "Forecast";
                s.Total = 100 - r.Total;
                ensgraph.Add(s);

                return ensgraph;
            }
            else
            {
                int EnSiteproject = projectId;

                var enresult = _context.PlanMetrics.Where(x => x.ProjectId == EnSiteproject
                     && (x.MetricsType == MetricsType.Enrolled) && x.Forecast != null && x.DeletedDate == null).FirstOrDefault();


                var enresults = _context.OverTimeMetrics.Where(x => x.PlanMetricsId == enresult.Id
                    && x.DeletedDate == null).ToList();

                entotals = (int)enresults.Sum(c => c.Planned);

                var engraph = new List<DashboardEnrolledGraph>();

                var r = new DashboardEnrolledGraph();
                r.DisplayName = "Planned";
                r.Total = entotals * 100 / enresult.Forecast;
                engraph.Add(r);

                var s = new DashboardEnrolledGraph();
                s.DisplayName = "Forecast";
                s.Total = 100 - r.Total;
                engraph.Add(s);
                                
                return engraph;
            }
        }
        public dynamic GetScreenedGraph(int projectId, int countryId, int siteId)
        {
            int sctotals = 0;
            if (countryId != 0 && siteId != 0)
            {
                var ScStudyproject = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
                var scdata = _context.PlanMetrics.Where(x => x.ProjectId == projectId && (x.MetricsType == MetricsType.Screened) && x.Forecast != null && x.DeletedDate == null).FirstOrDefault();

                var scsresults = _context.OverTimeMetrics.Where(x => ScStudyproject.Contains(x.ProjectId) && x.PlanMetricsId == scdata.Id && x.DeletedDate == null).ToList();

                sctotals = (int)scsresults.Sum(c => c.Planned);

                var scsgraph = new List<DashboardEnrolledGraph>();

                var r = new DashboardEnrolledGraph();
                r.DisplayName = "Planned";
                r.Total = sctotals * 100 / scdata.Forecast;
                scsgraph.Add(r);

                var s = new DashboardEnrolledGraph();
                s.DisplayName = "Forecast";
                s.Total = 100 - r.Total;
                scsgraph.Add(s);

                return scsgraph;
            }
            else
            {
                int ScSiteproject = projectId;

                var scresult = _context.PlanMetrics.Where(x => x.ProjectId == ScSiteproject
                     && (x.MetricsType == MetricsType.Screened) && x.Forecast != null && x.DeletedDate == null).FirstOrDefault();


                var scresults = _context.OverTimeMetrics.Where(x => x.PlanMetricsId == scresult.Id
                    && x.DeletedDate == null).ToList();

                sctotals = (int)scresults.Sum(c => c.Planned);

                var scgraph = new List<DashboardScreenedGraph>();

                var r = new DashboardScreenedGraph();
                r.DisplayName = "Planned";
                r.Total = sctotals * 100 / scresult.Forecast;
                scgraph.Add(r);

                var s = new DashboardScreenedGraph();
                s.DisplayName = "Forecast";
                s.Total = 100 - r.Total;
                scgraph.Add(s);

                return scgraph;
            }
        }
        public dynamic GetRandomizedGraph(int projectId, int countryId, int siteId)
        {
            int ratotals = 0;
            if (countryId != 0 && siteId != 0)
            {
                var RaStudyproject = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
                var radata = _context.PlanMetrics.Where(x => x.ProjectId == projectId && (x.MetricsType == MetricsType.Randomized) && x.Forecast != null && x.DeletedDate == null).FirstOrDefault();

                var rasresults = _context.OverTimeMetrics.Where(x => RaStudyproject.Contains(x.ProjectId) && x.PlanMetricsId == radata.Id && x.DeletedDate == null).ToList();

                ratotals = (int)rasresults.Sum(c => c.Planned);

                var rasgraph = new List<DashboardEnrolledGraph>();

                var r = new DashboardEnrolledGraph();
                r.DisplayName = "Planned";
                r.Total = ratotals * 100 / radata.Forecast;
                rasgraph.Add(r);

                var s = new DashboardEnrolledGraph();
                s.DisplayName = "Forecast";
                s.Total = 100 - r.Total;
                rasgraph.Add(s);

                return rasgraph;
            }
            else
            {
                int RaSiteproject = projectId;

                var raresult = _context.PlanMetrics.Where(x => x.ProjectId == RaSiteproject
                     && (x.MetricsType == MetricsType.Randomized) && x.Forecast != null && x.DeletedDate == null).FirstOrDefault();


                var raresults = _context.OverTimeMetrics.Where(x => x.PlanMetricsId == raresult.Id
                    && x.DeletedDate == null).ToList();

                ratotals = (int)raresults.Sum(c => c.Planned);

                var ragraph = new List<DashboardRandomizedGraph>();

                var r = new DashboardRandomizedGraph();
                r.DisplayName = "Planned";
                r.Total = ratotals * 100 / raresult.Forecast;
                ragraph.Add(r);

                var s = new DashboardRandomizedGraph();
                s.DisplayName = "Forecast";
                s.Total = 100 - r.Total;
                ragraph.Add(s);

                return ragraph;
            }
        }
        #endregion

    }
}
