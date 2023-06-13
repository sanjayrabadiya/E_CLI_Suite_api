﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.CTMS
{
    public class OverTimeMetricsRepository : GenericRespository<OverTimeMetrics>, IOverTimeMetricsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMetricsRepository _metricsRepository;
        public OverTimeMetricsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, IProjectRepository projectRepository, IProjectRightRepository projectRightRepository, IMetricsRepository MetricsRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectRepository = projectRepository;
            _projectRightRepository = projectRightRepository;
            _metricsRepository = MetricsRepository;
        }

        public List<OverTimeMetricsGridDto> GetTasklist(bool isDeleted, int metricsId, int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && projectIds.Contains(x.ProjectId) && x.PlanMetricsId== metricsId).OrderBy(x => x.Id).
                   ProjectTo<OverTimeMetricsGridDto>(_mapper.ConfigurationProvider).ToList();
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
            else if (countryId == 0 && siteId > 0)
            {
                projectIds = _projectRepository.All.Include(x => x.ManageSite).Where(x => x.ParentProjectId == projectId
                                                          && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                                                          && a.UserId == _jwtTokenAccesser.UserId
                                                          && a.RoleId == _jwtTokenAccesser.RoleId
                                                          && a.DeletedDate == null
                                                          && a.RollbackReason == null)
                                                          && x.Id == siteId
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
        //public string Duplicate(OverTimeMetrics objSave)
        //{
        //    if (All.Any(x => x.Id != objSave.Id && x.PlanMetricsId == objSave.PlanMetricsId && x.TaskName == objSave.TaskName && x.DeletedDate == null))
        //        return "Duplicate Task";
        //    return "";
        //}
        public string PlannedCheck(OverTimeMetrics objSave)
        {
            var planMetrics = _metricsRepository.Find(objSave.PlanMetricsId).Forecast;
            var project = All.Where(x => x.PlanMetricsId == objSave.PlanMetricsId && x.DeletedDate == null).ToList();
            int total = (int)project.Sum(item => item.Planned);
            if (objSave.Id == 0) { 
                total += objSave.Planned;
            }
            else { 
                total -= Find(objSave.Id).Planned;
                total += objSave.Planned;
            }
            if (planMetrics < total)
            return "Planned Subjects are not Added Greater Than " + planMetrics;

            return "";
        }
        public string UpdatePlanning(OverTimeMetrics overTimeMetricsDto)
        {
            if (overTimeMetricsDto.PlanningType != null && overTimeMetricsDto.Planned != null && overTimeMetricsDto.Planned != 0)
            {
                TimeSpan tDay = (TimeSpan)(overTimeMetricsDto.EndDate - overTimeMetricsDto.StartDate);
                decimal tplan = 0;
                if (tDay.Days >= 0 && tDay.Days <= 6 && overTimeMetricsDto.PlanningType == PlanningType.Day)
                {
                    tplan = tDay.Days==0 ? 1 : tDay.Days / 1;
                }
                else if (tDay.Days >= 7 && tDay.Days <= 29 && (overTimeMetricsDto.PlanningType == PlanningType.Week || overTimeMetricsDto.PlanningType == PlanningType.Day))
                {
                    tplan = tDay.Days / (overTimeMetricsDto.PlanningType == PlanningType.Day ? 1 : 7);
                }
                else if (tDay.Days >= 30 && tDay.Days <= 364 && (overTimeMetricsDto.PlanningType == PlanningType.Month || overTimeMetricsDto.PlanningType == PlanningType.Week || overTimeMetricsDto.PlanningType == PlanningType.Day))
                {
                    tplan = tDay.Days / (overTimeMetricsDto.PlanningType == PlanningType.Day ? 1 : overTimeMetricsDto.PlanningType == PlanningType.Week ? 7 : 30);
                }
                else if (tDay.Days >= 365 && (overTimeMetricsDto.PlanningType == PlanningType.Year || overTimeMetricsDto.PlanningType == PlanningType.Month || overTimeMetricsDto.PlanningType == PlanningType.Week || overTimeMetricsDto.PlanningType == PlanningType.Day))
                {
                    tplan = tDay.Days / (overTimeMetricsDto.PlanningType == PlanningType.Day ? 1 : overTimeMetricsDto.PlanningType == PlanningType.Week ? 7 : overTimeMetricsDto.PlanningType == PlanningType.Month ? 30 : 365);
                }
                else
                {
                    return ("please select Planning Type Proper as Between the Date");
                }
                decimal actualPlanned = overTimeMetricsDto.Planned / tplan;
                overTimeMetricsDto.TotalPlannig = tplan.ToString() + "-" + overTimeMetricsDto.PlanningType;
                overTimeMetricsDto.ActualPlannedno = (int?)actualPlanned;
                return "";
            }
            return "";
        }

        public string UpdateActualNo(OverTimeMetrics overTimeMetricsDto)
        {
            if (overTimeMetricsDto.ProjectId != null)
            {
                var metricsType = _metricsRepository.Find(overTimeMetricsDto.PlanMetricsId).MetricsType;
               
               var ProjectSettings = _context.Randomization.Where(x => x.ProjectId== overTimeMetricsDto.ProjectId && x.DeletedDate == null &&
               metricsType == MetricsType.Enrolled ? x.CreatedDate>= overTimeMetricsDto.StartDate && x.CreatedDate <= overTimeMetricsDto.EndDate :
               metricsType == MetricsType.Screened ? x.DateOfScreening >= overTimeMetricsDto.StartDate && x.DateOfScreening <= overTimeMetricsDto.EndDate :
               x.DateOfRandomization >= overTimeMetricsDto.StartDate && x.DateOfRandomization <= overTimeMetricsDto.EndDate).ToList();

                overTimeMetricsDto.Actual = ProjectSettings.Count();
            }
            return "";
        }
        public List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == "act_002" && x.DeletedDate == null).ToList();

            var Activity = _context.Activity.Where(x => CtmsActivity.Select(v => v.Id).Contains(x.CtmsActivityId) && x.DeletedDate == null).ToList();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                               .Where(x => Activity.Select(f => f.Id).Contains(x.ActivityId) && x.ProjectId == parentProjectId
                               && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var CtmsMonitoring = _context.CtmsMonitoringReport
               .Include(i => i.CtmsMonitoring).Where(x => StudyLevelForm.Select(v => v.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) && x.CtmsMonitoring.DeletedDate == null && x.ReportStatus == MonitoringReportStatus.Approved).ToList();

            return _context.Project.Where(x => CtmsMonitoring.Select(v => v.CtmsMonitoring.ProjectId).Contains(x.Id) &&
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName,
                    CountryId = c.ManageSite != null && c.ManageSite.City != null && c.ManageSite.City.State != null ? c.ManageSite.City.State.CountryId : 0,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    IsTestSite = c.IsTestSite,
                    ParentProjectId = c.ParentProjectId ?? 0,
                    AttendanceLimit = c.AttendanceLimit ?? 0, 
                }).OrderBy(o => o.Value).ToList();
        }
    }
}