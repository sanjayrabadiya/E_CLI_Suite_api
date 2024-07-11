using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Project.Generalconfig;
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
        private readonly IUnitOfWork _uow;

        public OverTimeMetricsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IUnitOfWork uow,
            IMapper mapper, IProjectRepository projectRepository,
            IProjectRightRepository projectRightRepository, IMetricsRepository metricsRepository)
            : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _projectRightRepository = projectRightRepository;
            _metricsRepository = metricsRepository;
        }

        public List<OverTimeMetrics> UpdateAllActualNo(bool isDeleted, int metricsId, int projectId, int countryId, int siteId)
        {
            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();
            var overTimeMetrics = All
                .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && projectIds.Contains(x.ProjectId) && x.PlanMetricsId == metricsId)
                .ToList();

            foreach (var task in overTimeMetrics)
            {
                var metricsType = _metricsRepository.Find(task.PlanMetricsId).MetricsType;
                var projectSettings = GetProjectSettings(metricsType, task.StartDate, task.EndDate, task.ProjectId);
                task.Actual = projectSettings.Count;
                Update(task);
                _uow.Save();
            }
            return overTimeMetrics;
        }

        public List<OverTimeMetricsGridDto> GetTasklist(bool isDeleted, int metricsId, int projectId, int countryId, int siteId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<OverTimeMetricsGridDto>();

            var projectIds = GetProjectIds(projectId, countryId, siteId).Select(s => s.Id).ToList();

            return All
                .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && projectIds.Contains(x.ProjectId) && x.PlanMetricsId == metricsId && projectList.Contains(x.ProjectId))
                .OrderBy(x => x.Id)
                .ProjectTo<OverTimeMetricsGridDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        private List<Data.Entities.Master.Project> GetProjectIds(int projectId, int countryId, int siteId)
        {
            var query = _projectRepository.All.Include(x => x.ManageSite).AsQueryable();

            if (countryId > 0)
                query = query.Where(x => x.ManageSite.City.State.CountryId == countryId);
            if (siteId > 0)
                query = query.Where(x => x.Id == siteId);

            return query
                .Where(x => x.ParentProjectId == projectId
                    && _projectRightRepository.All.Any(a => a.ProjectId == x.Id
                        && a.UserId == _jwtTokenAccesser.UserId
                        && a.RoleId == _jwtTokenAccesser.RoleId
                        && a.DeletedDate == null
                    && a.RollbackReason == null)
                && x.DeletedDate == null)
                .ToList();
        }

        private List<Randomization> GetProjectSettings(MetricsType metricsType, DateTime? startDate, DateTime? endDate, int projectId)
        {
            return _context.Randomization.Where(x => x.ProjectId == projectId && x.DeletedDate == null
                && (metricsType == MetricsType.Enrolled ? x.CreatedDate >= startDate && x.CreatedDate <= endDate
                : metricsType == MetricsType.Screened ? x.DateOfScreening >= startDate && x.DateOfScreening <= endDate
                : x.DateOfRandomization >= startDate && x.DateOfRandomization <= endDate)).ToList();
        }

        public string PlannedCheck(OverTimeMetrics objSave)
        {
            var planMetrics = _metricsRepository.Find(objSave.PlanMetricsId).Forecast;
            var totalPlanned = All.Where(x => x.PlanMetricsId == objSave.PlanMetricsId && x.DeletedDate == null && x.If_Active == true)
                                  .Sum(item => item.Planned) - (objSave.Id == 0 ? 0 : Find(objSave.Id).Planned) + objSave.Planned;

            return totalPlanned > planMetrics ? $"Planned Subjects are not Added Greater Than {planMetrics}" : string.Empty;
        }

        public string UpdatePlanning(OverTimeMetrics overTimeMetricsDto)
        {
            if (overTimeMetricsDto.Planned == 0) return string.Empty;

            var tDay = (TimeSpan)(overTimeMetricsDto.EndDate - overTimeMetricsDto.StartDate);
            decimal tplan = CalculatePlanningDays(tDay.Days, overTimeMetricsDto.PlanningType);

            if (tplan == 0)
                return "Please select Planning Type Proper as Between the Date";

            decimal actualPlanned = overTimeMetricsDto.Planned / tplan;
            overTimeMetricsDto.TotalPlannig = $"{tplan}-{overTimeMetricsDto.PlanningType}";
            overTimeMetricsDto.ActualPlannedno = (int?)actualPlanned;

            return string.Empty;
        }

        private decimal CalculatePlanningDays(int totalDays, PlanningType planningType)
        {
            if (totalDays < 0) return 0;

            return planningType switch
            {
                PlanningType.Day => totalDays == 0 ? 1 : totalDays,
                PlanningType.Week => (decimal)totalDays / 7,
                PlanningType.Month => (decimal)totalDays / 30,
                PlanningType.Year => (decimal)totalDays / 365,
                _ => 0,
            };
        }

        public List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectDropDown>();

            var appScreenId = _context.AppScreen.FirstOrDefault(x => x.ScreenCode == "mnu_ctms")?.Id;
            var activityIds = _context.CtmsActivity.Where(x => x.ActivityCode == "act_002" && x.DeletedDate == null).Select(x => x.Id).ToList();
            var studyLevelFormIds = _context.StudyLevelForm.Include(x => x.Activity)
                .Where(x => activityIds.Contains(x.ActivityId) && x.ProjectId == parentProjectId && x.AppScreenId == appScreenId && x.DeletedDate == null)
                .Select(x => x.Id)
                .ToList();
            var projectIds = _context.CtmsMonitoringReport
                .Include(i => i.CtmsMonitoring)
                .Where(x => studyLevelFormIds.Contains(x.CtmsMonitoring.StudyLevelFormId) && x.CtmsMonitoring.DeletedDate == null && x.ReportStatus == MonitoringReportStatus.Approved)
                .Select(x => x.CtmsMonitoring.ProjectId)
                .ToList();

            return _context.Project.Where(x => projectIds.Contains(x.Id)
                                               && (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                                               && x.DeletedDate == null
                                               && projectList.Contains(x.Id))
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
