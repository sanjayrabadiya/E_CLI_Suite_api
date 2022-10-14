﻿using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
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
            IProjectDocumentReviewRepository projectDocumentReviewRepository)
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
                .Include(x => x.Project).Where(x => projectIds.Contains(x.Project.Id) && (siteId == 0 ? (!x.ScreeningEntry.Project.IsTestSite) : true) && x.DeletedDate == null).ToList()
                .GroupBy(g => g.Project.ParentProjectId).Select(s => new
                {
                    parent = s.Key,
                    EnrolledTotal = total,
                    PreScreeened = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.PreScreening).Count(),
                    Screened = s.Where(q => q.PatientStatusId == ScreeningPatientStatus.Screening).Count(),
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

            DaysDiffList.ForEach(t => {
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
                        var data = query.Where(x => x.QueryParentId == item.Id).FirstOrDefault();
                        if (data != null && data.QueryStatus == QueryStatus.Closed)
                        {
                            DashboardQueryGraphDto obj = new DashboardQueryGraphDto();
                            obj.ScreeningTemplateValueId = data.ScreeningTemplateValueId;
                            double week = ((DateTime)data.CreatedDate - (DateTime)item.CreatedDate).TotalDays / 7;
                            obj.Lable = Convert.ToInt32(week + 1) + " Week";
                            obj.Id = item.Id;
                            obj.week = Convert.ToInt32(week + 1);
                            list.Add(obj);
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

    }
}
