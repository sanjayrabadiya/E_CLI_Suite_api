using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
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
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;


        public DashboardRepository(IJwtTokenAccesser jwtTokenAccesser, IScreeningVisitRepository screeningVisitRepository,
            IRandomizationRepository randomizationRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IProjectDesignRepository projectDesignRepository,
            IProjectRepository projectRepository,
            IProjectRightRepository projectRightRepository,
            IProjectDocumentReviewRepository projectDocumentReviewRepository,
            IGSCContext context)
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
            r.ScreeningTemplateValue.ProjectDesignVariable.DeletedDate == null && r.ScreeningTemplateValue.DeletedDate == null);

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
            result.Screened = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.Screening).Count();
            result.ConsentInProgress = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.ConsentInProcess).Count();
            result.ConsentCompleted = randomizations.Where(x => x.PatientStatusId == ScreeningPatientStatus.ConsentCompleted).Count();
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
                                                : x.Id == 3 ? v.PatientStatusId == ScreeningPatientStatus.ConsentCompleted
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
                    EntrollCount = 0
                }).ToList();

                var result1 = _context.Randomization.Where(x => asd.Select(z => z.ProjectId).Contains(x.ProjectId) && x.DeletedDate == null).GroupBy(x => x.ProjectId).Select(x => x.Key).Count();
                var result3 = new CtmsMonitoringStatusChartDto
                {
                    ActivityName = "Enrolled",
                    EntrollCount = result1

                };
                result.Add(result3);
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
                        }
                        if (item.ActivityName == "Enrolled")
                        {
                            if (item.EntrollCount > 0)
                            {
                                CtmsMonitoringStatusPIChartDto obj = new CtmsMonitoringStatusPIChartDto();
                                obj.Text = item.ActivityName + " " + item.EntrollCount;
                                obj.Lable = item.ActivityName;
                                obj.Count = item.EntrollCount;
                                obj.Status = "Enrolled";
                                list.Add(obj);
                            }

                        }

                    }
                }
                return list;
            }
            return null;
        }

        public List<CtmsMonitoringPlanDashoardDto> getCTMSMonitoringPlanDashboard(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();


            var list = _context.CtmsMonitoring
                .Include(x => x.Project)
                .ThenInclude(x => x.ManageSite)
                .Where(x => projectIds.Contains(x.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(x.StudyLevelFormId)
                            && x.DeletedDate == null)
                .Select(b => new CtmsMonitoringPlanDashoardDto
                {
                    Id = b.Id,
                    Activity = b.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                    ScheduleStartDate = b.ScheduleStartDate,
                    ActualStartDate = b.ActualStartDate,
                    Site = b.Project.ProjectCode,
                    Country = b.Project.ManageSite.City.State.Country.CountryName

                }).ToList();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    item.Status = GetMonitoringStatus(item);
                }
            }



            return list;
        }

        public string GetMonitoringStatus(CtmsMonitoringPlanDashoardDto obj)
        {
            var data = _context.CtmsMonitoringReportReview.Include(x => x.CtmsMonitoringReport).Where(x => x.CtmsMonitoringReport.CtmsMonitoringId == obj.Id).FirstOrDefault();
            if (obj.ScheduleStartDate != null && data == null)
            {
                return "In Progress";
            }
            if (obj.ScheduleStartDate != null && data != null && !data.IsApproved)
            {
                return "In signature/pending review";
            }
            if (obj.ScheduleStartDate != null && data != null && data.IsApproved)
            {
                return "Final";
            }
            return "";
        }

        public dynamic getCTMSMonitoringActionPointChartDashboard(int projectId, int countryId, int siteId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_001" || x.ActivityCode == "act_002" || x.ActivityCode == "act_003" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == projectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();


            var asd = _context.CtmsActionPoint.Include(x => x.CtmsMonitoring).Where(x => projectIds.Contains(x.CtmsMonitoring.ProjectId) && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId)
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

        public dynamic GetDashboardAesBySeverityGraph(int projectId, int countryId, int siteId)
        {
            //var template = _context.ProjectDesignTemplate.Where(x => x.TemplateCode == "AE001" && x.DeletedDate == null).FirstOrDefault();
            //var Variable = _context.ProjectDesignVariable.Include(x => x.ProjectDesignTemplate).Include(x => x.Values).Where(x => x.ProjectDesignTemplate.TemplateCode == "AE001" && x.DeletedDate == null).ToList();

            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            string[] types = { "Major", "Mild", "Moderate", "Severe" };
            string[] ser = { "likely", "possible unlikely", "conditional unspecified" };

            //  type: "Major", likely: 5, unlikely: 7, unspecified: 8

            //var screeningTempateValue = _screeningTemplateValueRepository.All
            //    .Include(x => x.ProjectDesignVariable)
            //    .ThenInclude(x => x.Values)
            //     .Include(x => x.ProjectDesignVariable)
            //    .ThenInclude(x => x.ProjectDesignTemplate)
            //    .ThenInclude(x => x.ProjectDesignVisit)
            //    .Include(x => x.ScreeningTemplate)
            //    .ThenInclude(x => x.ScreeningVisit)
            //    .ThenInclude(x => x.ScreeningEntry).
            //    Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
            //    && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
            //    && (x.ProjectDesignVariable.VariableCode == "V003" || x.ProjectDesignVariable.VariableCode == "001")
            //    && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).ToList().GroupBy(x => x.ScreeningTemplateId).ToList();

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
                && (x.ProjectDesignVariable.VariableCode == "V003")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new
                {
                    r.ScreeningTemplateId,
                    r.ProjectDesignVariableId,
                    r.ProjectDesignVariable.VariableName,
                    r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value)).Select(g => g.ValueName).FirstOrDefault(),
                    against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value)).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<AeChart>();


            foreach (string t in types)
            {
                var r = new AeChart();
                r.Type = t;
                r.Likely = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[0]).ToList().Count();
                r.Unlikely = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[1]).ToList().Count();
                r.Unspecified = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[2]).ToList().Count();
                result.Add(r);
            }


            //var finalResult = tenoResult.GroupBy(x => new { x.Value, x.against }).Select(a => new
            //{
            //    //data = tenoResult.Where(p => p.ProjectDesignVariableId == a.Key.ProjectDesignVariableId).GroupBy(v => v.Value).
            //    //   Select(d => new
            //    //   {
            //    count = a.Count(),
            //    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(a.Key.Value)).Select(g => g.ValueName).FirstOrDefault(),
            //    Against = a.Key.against
            //    //}).ToList()


            //});

            return result;
            //var result = new AeReportDto();
            //var VariableValue = Variable.Find(x => x.VariableCode == "V003").Values.Select(s => new { Id = s.Id, Value = s.ValueName }).ToList();
            //var VValue = Variable.Find(x => x.VariableCode == "001").Values.Select(s => new { Id = s.Id, Value = s.ValueName }).ToList();

            //dynamic newobj = new ExpandoObject();

            ////dynamic obj for property
            //VValue.ForEach(x =>
            //{
            //    AddProperty(newobj, "Variable", x.Value);

            //    VariableValue.ForEach(s =>
            //    {
            //        AddProperty(newobj, s.Value, s.Id);
            //    });

            //});

            ////for get proprty name
            //List<String> list = new List<String>();

            ////for get property(collectionValue Id)
            //List<String> valueID = new List<String>();

            //// set property name and id
            //foreach (KeyValuePair<string, object> kvp in ((IDictionary<string, object>)newobj))
            //{
            //    //string PropertyWithValue = kvp.Key + ": " + kvp.Value.ToString();
            //    list.Add(kvp.Key);
            //    valueID.Add(kvp.Value.ToString());
            //}

            //dynamic returnobj = new ExpandoObject();
            //VValue.ForEach(x =>
            //{
            //    AddProperty(returnobj, "Variable", x.Value);

            //    for (int i = 1; list.Count() > i; i++)
            //    {
            //        int j = 0;

            //        screeningTempateValue.ForEach(z =>
            //        {
            //            var count = z.Where(y => Convert.ToInt32(y.Value) == Convert.ToInt32(valueID[i]) || Convert.ToInt32(y.Value) == x.Id).Count();
            //            if (count > 1)
            //                j++;
            //        });
            //        AddProperty(returnobj, list[i], j);
            //    }
            //    var ret = returnobj;


            //});
            //// R.Add()




            //return screeningTempateValue;
        }

        public dynamic GetDashboardAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            string[] types = { "Major", "Mild", "Moderate", "Severe" };
            string[] ser = { "resolved without sequlae", "resolved with sequlae", "ongoing", "ongoing at the time of death", "death Complete Study Exit CRF with date of death", "other specify" };

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
                && (x.ProjectDesignVariable.VariableCode == "V004")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new
                {
                    r.ScreeningTemplateId,
                    r.ProjectDesignVariableId,
                    r.ProjectDesignVariable.VariableName,
                    r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value)).Select(g => g.ValueName).FirstOrDefault(),
                    against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "001")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value)).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<AeCChart>();


            foreach (string t in types)
            {
                var r = new AeCChart();
                r.Type = t;
                r.ResolvedWithoutSequlae = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[0].ToLower()).ToList().Count();
                r.ResolvedWithSequlae = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[1].ToLower()).ToList().Count();
                r.Ongoing = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[2].ToLower()).ToList().Count();
                r.OngoingAtTheTimeOfDeath = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[3].ToLower()).ToList().Count();
                r.Death = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[4].ToLower()).ToList().Count();
                r.Other = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[5].ToLower()).ToList().Count();
                result.Add(r);
            }

            return result;
            
        }

        public dynamic GetDashboardSAesBySeverityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            string[] types = { "Death", "Life Threatening", "Requires impatient hospitalization or prolongation of existing hospitalization", "Result in persistent or significant disability or incapacity", "Birth defect", "Other medically important event" };
            string[] ser = { "likely", "possible unlikely", "conditional unspecified" };

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
                && (x.ProjectDesignVariable.VariableCode == "V003")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new
                {
                    r.ScreeningTemplateId,
                    r.ProjectDesignVariableId,
                    r.ProjectDesignVariable.VariableName,
                    r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value)).Select(g => g.ValueName).FirstOrDefault(),
                    against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "V002")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value)).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<AeChart>();


            foreach (string t in types)
            {
                var r = new AeChart();
                r.Type = t;
                r.Likely = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[0]).ToList().Count();
                r.Unlikely = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[1]).ToList().Count();
                r.Unspecified = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[2]).ToList().Count();
                result.Add(r);
            }

            return result;
        }

        public dynamic GetDashboardSAesBySeverityandCausalityGraph(int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            string[] types = { "Death", "Life Threatening", "Requires impatient hospitalization or prolongation of existing hospitalization", "Result in persistent or significant disability or incapacity", "Birth defect", "Other medically important event" };
            string[] ser = { "Resolved without sequlae", "Resolved with sequlae", "Ongoing", "Ongoing at the time of death", "Death Complete Study Exit CRF with date of death", "Other specify" };

            var tenoResult = _screeningTemplateValueRepository.All.
                Where(x => projectIds.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId)
                && x.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode == "AE001"
                && (x.ProjectDesignVariable.VariableCode == "V004")
                && (siteId == 0 ? (!x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).Select(r => new
                {
                    r.ScreeningTemplateId,
                    r.ProjectDesignVariableId,
                    r.ProjectDesignVariable.VariableName,
                    r.Value,
                    VariableValue = _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(r.Value)).Select(g => g.ValueName).FirstOrDefault(),
                    against = _screeningTemplateValueRepository.All.Where(x => x.ScreeningTemplateId == r.ScreeningTemplateId && x.ProjectDesignVariable.VariableCode == "V002")
                    .Select(x => _context.ProjectDesignVariableValue.Where(m => m.Id == Convert.ToInt32(x.Value)).Select(g => g.ValueName).FirstOrDefault()).FirstOrDefault()
                }).ToList();

            var result = new List<AeCChart>();


            foreach (string t in types)
            {
                var r = new AeCChart();
                r.Type = t;
                r.ResolvedWithoutSequlae = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[0].ToLower()).ToList().Count();
                r.ResolvedWithSequlae = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[1].ToLower()).ToList().Count();
                r.Ongoing = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[2].ToLower()).ToList().Count();
                r.OngoingAtTheTimeOfDeath = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[3].ToLower()).ToList().Count();
                r.Death = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[4].ToLower()).ToList().Count();
                r.Other = tenoResult.Where(e => e.against == t && e.VariableValue.ToLower() == ser[5].ToLower()).ToList().Count();
                result.Add(r);
            }

            return result;

        }
    }
}
