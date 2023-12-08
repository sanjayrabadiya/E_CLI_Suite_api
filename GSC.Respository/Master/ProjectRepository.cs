using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.GeneralConfig;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Shared.Extension;

namespace GSC.Respository.Master
{
    public class ProjectRepository : GenericRespository<Data.Entities.Master.Project>, IProjectRepository
    {
        Dictionary<int, int> templateIdMap = new Dictionary<int, int>();
        Dictionary<int, int> variableIdMap = new Dictionary<int, int>();
        Dictionary<int, int> variableValueIdMap = new Dictionary<int, int>();

        private readonly ICountryRepository _countryRepository;
        private readonly IDesignTrialRepository _designTrialRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectSettingsRepository _projectSettingsRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ISiteTeamRepository _siteTeamRepository;
        private readonly IStudyVersionRepository _studyVersionRepository;

        public ProjectRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            INumberFormatRepository numberFormatRepository,
            IProjectSettingsRepository projectSettingsRepository,
            ICountryRepository countryRepository,
            IDesignTrialRepository designTrialRepository,
            IProjectRightRepository projectRightRepository,
            IMapper mapper,
            ISiteTeamRepository siteTeamRepository,
            IStudyVersionRepository studyVersionRepository)
            : base(context)
        {
            _numberFormatRepository = numberFormatRepository;
            _projectSettingsRepository = projectSettingsRepository;
            _countryRepository = countryRepository;
            _designTrialRepository = designTrialRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _mapper = mapper;
            _context = context;
            _siteTeamRepository = siteTeamRepository;
            _studyVersionRepository = studyVersionRepository;
        }

        public IList<DashboardProject> GetProjectList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<DashboardProject>();

            var projects = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && projectList.Contains(x.Id) && x.ParentProjectId == null)
                .Select(c => new DashboardProject
                {
                    ProjectId = c.Id,
                    CreatedDate = c.CreatedDate.Value
                }).OrderByDescending(x => x.ProjectId).ToList();


            projects.ForEach(item =>
            {
                var temCountries = new List<string>();

                var countries = _context.Project
              .Where(x => x.DeletedDate == null && x.ParentProjectId == item.ProjectId && x.ManageSite != null).Select(r => new
              {
                  Id = (int)r.ManageSite.City.State.CountryId,
                  CountryName = r.ManageSite.City.State.Country.CountryName,
                  CountryCode = r.ManageSite.City.State.Country.CountryCode
              }).Distinct().OrderBy(o => o.CountryCode).ToList();

                var project = _context.Project.Where(x => x.ParentProjectId == null && x.Id == item.ProjectId).
                    ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).FirstOrDefault();
                foreach (var country in countries)
                {
                    temCountries.Add(country.CountryName);
                }

                project.LiveVersion = _studyVersionRepository.All.Where(x => x.ProjectId == item.ProjectId && x.DeletedDate == null && x.VersionStatus == VersionStatus.GoLive).Select(t => t.VersionNumber.ToString()).FirstOrDefault();
                project.AnyLive = _studyVersionRepository.All.Any(x => x.ProjectId == item.ProjectId && x.DeletedDate == null && x.VersionStatus == VersionStatus.GoLive);
                project.TrialVersion = _studyVersionRepository.All.Where(x => x.ProjectId == item.ProjectId && x.DeletedDate == null && x.VersionStatus == VersionStatus.OnTrial).Select(t => t.VersionNumber.ToString()).FirstOrDefault();
                project.ProjectStatusName = _context.ProjectStatus.Any(x => x.ProjectId == item.ProjectId) ? _context.ProjectStatus.Where(x => x.ProjectId == item.ProjectId).FirstOrDefault().Status.GetDescription() : "";
                project.IsCtmsStudy = _context.ProjectSettings.Any(x => x.ProjectId == item.ProjectId) ? _context.ProjectSettings.Where(x => x.ProjectId == item.ProjectId).FirstOrDefault().IsCtms : false;
                item.CountriesName = temCountries.Distinct().ToList();
                item.CountCountry = temCountries.Distinct().Count();
                item.projectCode = project.ProjectCode;
                item.Project = project;


            });


            return projects;
        }







        public void Save(Data.Entities.Master.Project project)
        {
            if (project.ParentProjectId == null)
            {
                var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "project" && x.DeletedDate == null).FirstOrDefault();
                project.ProjectCode = numberFormat.IsManual ? project.ProjectCode : GetProjectCode(project);
            }
            else
            {
                var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "projectchild" && x.DeletedDate == null).FirstOrDefault();
                var projectSettings = _projectSettingsRepository.All.Where(x => x.ProjectId == project.ParentProjectId && x.DeletedDate == null).FirstOrDefault();

                if (projectSettings != null && projectSettings.IsCtms)
                {
                    project.ProjectCode = null;
                }
                else
                {
                    project.ProjectCode = numberFormat.IsManual ? project.ProjectCode : GetProjectSitesCode(project);
                    if (project.IsTestSite)
                        project.ProjectCode = "T-" + project.ProjectCode;
                }
            }

            project.ProjectRight = new List<Data.Entities.ProjectRight.ProjectRight>();
            project.ProjectRight.Add(new Data.Entities.ProjectRight.ProjectRight
            {
                UserId = _jwtTokenAccesser.UserId,
                IsPrimary = true,
                IsTrainingRequired = false,
                IsReviewDone = true,
                RoleId = _jwtTokenAccesser.RoleId
            });
            Add(project);
            foreach (var item in project.ProjectRight)
            {
                _projectRightRepository.Add(item);
            }
        }

        public string Duplicate(Data.Entities.Master.Project objSave)
        {
            //if (objSave.ParentProjectId != null || objSave.ParentProjectId <= 0)
            //{
            //    if (All.Any(x => x.Id != objSave.Id && x.ParentProjectId == objSave.ParentProjectId && x.ProjectCode == objSave.ProjectCode.Trim() && x.DeletedDate == null))
            //        return "Duplicate Site Code : " + objSave.ProjectCode;
            //}

            if (objSave.ParentProjectId == null || objSave.ParentProjectId <= 0)
            {
                if (All.AsNoTracking().Any(x =>
                    x.Id != objSave.Id && x.ProjectName == objSave.ProjectName.Trim() && x.DeletedDate == null && x.ParentProjectId == null))
                    return "Duplicate Study name : " + objSave.ProjectName;

                if (All.Any(x => x.Id != objSave.Id && x.ProjectCode != null && x.ProjectCode == objSave.ProjectCode && x.DeletedDate == null))
                    return "Duplicate Study Code : " + objSave.ProjectCode;
            }

            if (objSave.Id > 0 && objSave.AttendanceLimit != null && !objSave.IsStatic)
            {
                var attendantCount = _context.Attendance.Count(x => x.DeletedDate == null
                                                                   && x.AttendanceType == DataEntryType.Project
                                                                   && x.Status != AttendaceStatus.Suspended
                                                                   && !x.IsStandby
                                                                   && x.ProjectId == objSave.Id && x.PeriodNo == 1);

                if (objSave.AttendanceLimit < attendantCount)
                    return "Can't reduce attendance limit, already taken attendanced";
            }

            if (objSave.Id > 0)
                if (objSave.IsStatic != All.AsNoTracking().Any(x => x.Id == objSave.Id && x.IsStatic) &&
                    _context.ProjectDesign.Any(x => x.ProjectId == objSave.Id && x.DeletedDate == null))
                    return "Can't IsStatic value, already started project design!";
            return "";
        }

        public string CheckAttendanceLimitPost(Data.Entities.Master.Project objSave)
        {
            int? sum = All.AsNoTracking().Where(t => t.ParentProjectId == objSave.ParentProjectId && t.DeletedDate == null && t.IsTestSite == false)
                        .Select(t => t.AttendanceLimit ?? 0).Sum() + objSave.AttendanceLimit;
            int? subSum = All.AsNoTracking().Where(x => x.Id == objSave.ParentProjectId).FirstOrDefault().AttendanceLimit;

            if (subSum < sum)
            {
                return "Subject Limit out of range";
            }
            return "";
        }

        public string CheckAttendanceLimitPut(Data.Entities.Master.Project objSave)
        {
            int? limit = All.AsNoTracking().Where(x => x.Id == objSave.Id).FirstOrDefault().AttendanceLimit;

            int? sum = All.AsNoTracking().Where(t => t.ParentProjectId == objSave.ParentProjectId && t.DeletedDate == null && t.IsTestSite == false)
                        .Select(t => t.AttendanceLimit ?? 0).Sum() - limit + objSave.AttendanceLimit;

            //int? total = sum + objSave.AttendanceLimit;

            int? subSum = All.AsNoTracking().Where(x => x.Id == objSave.ParentProjectId).FirstOrDefault().AttendanceLimit;

            if (subSum < sum)
            {
                return "Subject Limit out of range";
            }
            return "";
        }

        public List<ProjectDropDown> GetParentProjectDropDown()
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ProjectCode != null
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Where(q => q.IsDeleted == false).Distinct().OrderBy(o => o.Value).ToList();
        }


        public List<ProjectDropDown> GetParentProjectDropDownEtmf()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var AlreadyAdded = _context.EtmfProjectWorkPlace.Where(x => x.DeletedDate == null && projectList.Any(c => c == x.ProjectId)).
                   ProjectTo<ETMFWorkplaceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var ProjectList = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null
                    && projectList.Any(c => c == x.Id)
                    )
                  .Select(c => new ProjectDropDown
                  {
                      Id = c.Id,
                      Value = c.ProjectCode,
                      Code = c.ProjectCode,
                      IsStatic = c.IsStatic,
                      ParentProjectId = c.ParentProjectId ?? c.Id,
                      IsDeleted = c.DeletedDate != null
                  }).Distinct().OrderBy(o => o.Value).ToList();

            return ProjectList.Where(x => !AlreadyAdded.Any(c => c.ProjectId == x.Id)).ToList();
        }

        public List<ProjectDropDown> GetParentProjectDropDownStudyReport()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var AlreadyAdded = _context.EtmfProjectWorkPlace.Where(x => x.DeletedDate == null && projectList.Any(c => c == x.ProjectId)).
                     ProjectTo<ETMFWorkplaceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var ProjectList = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null
                    && projectList.Any(c => c == x.Id)
                    )
                  .Select(c => new ProjectDropDown
                  {
                      Id = c.Id,
                      Value = c.ProjectCode,
                      Code = c.ProjectCode,
                      IsStatic = c.IsStatic,
                      ParentProjectId = c.ParentProjectId ?? c.Id,
                      IsDeleted = c.DeletedDate != null
                  }).Distinct().OrderBy(o => o.Value).ToList();

            return ProjectList.Where(x => AlreadyAdded.Any(c => c.ProjectId == x.Id)).ToList();
        }

        public List<ProjectDropDown> GetParentStaticProjectDropDown()
        {
            // var projectList = _projectRightRepository.GetProjectRightIdList();
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null && x.IsStatic == true
                    && x.ProjectCode != null
                    && projectList.Contains(x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    IsSendEmail = c.IsSendEmail,
                    IsSendSMS = c.IsSendSMS,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                    // add where condition for bypass delete study on 07/06/2023 by vipul
                }).Where(q => q.IsDeleted == false).Distinct().OrderBy(o => o.Value).ToList();
        }

        public List<LockUnlockProject> GetParentStaticProject()
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;
            var projects = All.Where(x =>
                  (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                  && x.ParentProjectId == null && x.IsStatic == true
                  && x.ProjectCode != null
                  && projectList.Contains(x.Id))
              .Select(c => new LockUnlockProject
              {
                  ProjectId = c.Id,
                  CreatedDate = c.CreatedDate.Value
              }).Distinct().ToList();

            projects.ForEach(item =>
            {
                var temCountries = new List<string>();

                var countries = _context.Project
               .Where(x => x.DeletedDate == null && x.ParentProjectId == item.ProjectId && x.ManageSite != null).Select(r => new
               {
                   Id = (int)r.ManageSite.City.State.CountryId,
                   CountryName = r.ManageSite.City.State.Country.CountryName,
                   CountryCode = r.ManageSite.City.State.Country.CountryCode
               }).Distinct().OrderBy(o => o.CountryCode).ToList();


                var project = _context.Project.Where(x => x.ParentProjectId == null && x.Id == item.ProjectId).
                    ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).FirstOrDefault();
                foreach (var country in countries)
                {
                    temCountries.Add(country.CountryName);
                }

                item.CountriesName = temCountries.Distinct().ToList();
                item.CountCountry = temCountries.Distinct().Count();
                item.projectCode = project.ProjectCode;
                item.Project = project;
            });

            return projects;
        }



        public IList<ProjectDropDown> GetProjectsForDataEntry()
        {
            var projectIds = _projectRightRepository.GetProjectRightIdList();
            if (!projectIds.Any()) return new List<ProjectDropDown>();

            var projects = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null
                    && projectIds.Any(c => c == x.Id)
                    && x.ParentProjectId == null
                    )
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    IsStatic = c.IsStatic,
                    //Value = c.ProjectCode + " - " + c.ProjectName,
                    Value = c.ProjectCode,
                    ParentProjectId = c.ParentProjectId ?? c.Id
                }).OrderBy(o => o.Value).ToList();

            return projects;
        }

        public IList<ProjectDropDown> GetAllProjectsForDataEntry()
        {
            var projectIds = _projectRightRepository.GetProjectRightIdList();
            if (!projectIds.Any()) return new List<ProjectDropDown>();

            var projects = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null
                    && projectIds.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    IsStatic = c.IsStatic,
                    Value = c.ProjectCode + " - " + c.ProjectName,
                    ParentProjectId = c.ParentProjectId ?? c.Id
                }).OrderBy(o => o.Value).ToList();

            return projects;
        }
      
        public List<ProjectDropDown> GetChildProjectDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null && x.ParentProjectId == parentProjectId
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
                    AttendanceLimit = c.AttendanceLimit ?? 0, //Add for site limt (Tinku Mahato)
                }).OrderBy(o => o.Value).ToList();
        }

        //Add by Mitul On 09-11-2023 GS1-I3112 -> f CTMS On By default Add CTMS Access table.
        public List<ProjectDropDown> GetChildProjectCTMSDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectChildCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null && x.ParentProjectId == parentProjectId
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
                    AttendanceLimit = c.AttendanceLimit ?? 0, //Add for site limt (Tinku Mahato)
                }).OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int ProjectDesignId)
        {
            var ParentProjectId = _context.ProjectDesign.Where(x => x.Id == ProjectDesignId).Select(t => t.ProjectId).FirstOrDefault();

            return GetChildProjectDropDown(ProjectDesignId);
        }

        public List<ProjectDropDown> GetChildProjectRightsDropDown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;


            return All.Where(x =>
                    ((x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null)
                    && projectList.Any(c => c == x.Id) && x.ParentProjectId != null)
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName,
                    CountryId = c.CountryId,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? 0,
                }).OrderBy(o => o.Value).ToList();
        }

        public string CheckChildProjectExists(int id)
        {
            var data = All.Where(x => x.ParentProjectId == id && x.DeletedDate == null).ToList();
            if (data.Count > 0) return "This project have a child project, so you can't delete this project.";

            return "";
        }

        public string CheckParentProjectExists(int id)
        {
            var childData = All.Where(x => x.Id == id).FirstOrDefault();

            if (childData != null && childData.ParentProjectId == null) return "";

            var parentData = All.Where(x => x.Id == childData.ParentProjectId && x.DeletedDate == null)
                .FirstOrDefault();
            if (parentData == null) return "This parent project is not active so you can't active this project.";
            return "";
        }

        public int? GetParentProjectId(int id)
        {
            var childData = All.Where(x => x.Id == id).FirstOrDefault();

            {
                return childData.ParentProjectId;
            }

        }

        public int GetNoOfSite(int id)
        {
            return All.Count(x => x.ParentProjectId == id && x.DeletedDate == null);
        }

        private string GetProjectCode(Data.Entities.Master.Project project)
        {
            if (project.ParentProjectId == null)
            {
                var projectCode = _numberFormatRepository.GenerateNumber("project");
                var country = _countryRepository.Find(project.CountryId).CountryCode;
                //var design = _designTrialRepository.Find(project.DesignTrialId).DesignTrialCode; // commented By Neel
                var design = _designTrialRepository.Find(project.DesignTrialId).DesignTrialName;
                projectCode = projectCode.Replace("DESIGN", design);
                projectCode = projectCode.Replace("COUNTRY", country);

                return projectCode.ToUpper();
            }

            var parent = Find((int)project.ParentProjectId);
            var count = FindBy(x => x.ParentProjectId == project.ParentProjectId && x.DeletedDate == null).Count();
            if (count == 0)
            {
                //Changes by Vipul for get number for project code
                // return parent.ProjectCode + "-A";
                return parent.ProjectCode + "-01";
            }
            //Changes by Vipul for get alphabate using iscii value
            //int number = count + 65;
            //return parent.ProjectCode + "-" + ((char)number).ToString();

            //Changes by Vipul for get number for project code
            var number = count + 1;
            return parent.ProjectCode + "-" + number.ToString().PadLeft(2, '0');
        }

        public string GetProjectSitesCode(Data.Entities.Master.Project project)
        {
            var SiteCount = 0;
            if (!project.IsTestSite)
                SiteCount = All.Where(x => x.ParentProjectId == project.ParentProjectId && x.IsTestSite == false).Count();
            else
                SiteCount = All.Where(x => x.ParentProjectId == project.ParentProjectId && x.IsTestSite == true).Count();
            var projectCode = _numberFormatRepository.GenerateNumberForSite("projectchild", SiteCount);
            var country = _countryRepository.Find(project.CountryId).CountryCode;
            //var design = _designTrialRepository.Find(project.DesignTrialId).DesignTrialCode; // commented by Neel 
            var design = _designTrialRepository.Find(project.DesignTrialId).DesignTrialName;
            projectCode = projectCode.Replace("DESIGN", design);
            projectCode = projectCode.Replace("COUNTRY", country);

            return projectCode.ToUpper();
        }

        public ProjectDetailsDto GetProjectDetails(int projectId)
        {
            var projectDetailsDto = new ProjectDetailsDto();
            var siteDetailsDto = new SiteDetailsDto();
            var workflowDetailsDto = new WorkflowDetailsDto();
            var designDetailsDto = new DesignDetailsDto();
            var userRightDetailsDto = new UserRightDetailsDto();
            var schedulesDetailsDto = new SchedulesDetailsDto();
            var editCheckDetailsDto = new EditCheckDetailsDto();

            siteDetailsDto.NoofSite = GetNoOfSite(projectId);
            siteDetailsDto.NoofCountry = All.Where(x => x.ParentProjectId == projectId && x.DeletedDate == null).GroupBy(x => x.ManageSite.City.State.Country.Id).Select(t => t.Key).Count();


            projectDetailsDto = All.Where(x => x.Id == projectId && x.DeletedDate == null).Select(t => new ProjectDetailsDto
            {
                CountryName = t.Country.CountryName,
                TrialTypeName = t.DesignTrial.TrialType.TrialTypeName,
                RegulatoryTypeName = t.RegulatoryType.RegulatoryTypeName //t.Country.CountryName
            }).FirstOrDefault();

            projectDetailsDto.Sites = All.Where(x => x.ParentProjectId == projectId && x.DeletedDate == null && x.ManageSite != null).Select(t => new BasicSiteDto
            {
                SiteCode = t.ProjectCode,
                SiteName = t.ProjectName,
                SiteCountry = t.ManageSite.City.State.Country.CountryName //t.Country.CountryName,
            }).ToList();

            var project = Find(projectId);
            projectDetailsDto.SendSMS = project.IsSendSMS ? "Yes" : "No";
            projectDetailsDto.SendEmail = project.IsSendEmail ? "Yes" : "No";
            // projectDetailsDto.RandomizationAutomatic = project.IsManualScreeningNo == true ? "No" : "Yes";

            siteDetailsDto.MarkAsCompleted = All.Any(x => x.ParentProjectId == projectId && x.DeletedDate == null);


            var projectDesing = _context.ProjectDesign.Where(x => x.ProjectId == projectId && x.DeletedDate == null).Select(t => new
            {
                projectDeisgnId = t.Id,
                ActiveVersion = t.StudyVersions.Where(r => r.DeletedDate == null && r.VersionStatus == VersionStatus.GoLive).Select(t => t.VersionNumber).FirstOrDefault(),
                TrialVersion = t.StudyVersions.Where(r => r.DeletedDate == null && r.VersionStatus == VersionStatus.OnTrial).Select(t => t.VersionNumber).FirstOrDefault(),
                TotalPeriod = t.ProjectDesignPeriods.Where(c => c.DeletedDate == null).Count(),
                TotalVisit = t.ProjectDesignPeriods.Where(c => c.DeletedDate == null).SelectMany(r => r.VisitList.Where(b => b.DeletedDate == null).Select(y => y.Id)).Count(),
                TotalVisitNoCRF = t.ProjectDesignPeriods.Where(c => c.DeletedDate == null).SelectMany(r => r.VisitList.Where(b => b.DeletedDate == null && b.IsNonCRF).Select(y => y.Id)).Count(),
                TotalTemplate = t.ProjectDesignPeriods.Where(c => c.DeletedDate == null).SelectMany(r => r.VisitList.Where(b => b.DeletedDate == null).
                SelectMany(r => r.Templates.Where(n => n.DeletedDate == null).Select(k => k.Id))).Count()
            }).FirstOrDefault();

            var projectDeisgnId = projectDesing?.projectDeisgnId;

            if (projectDesing != null)
            {
                designDetailsDto.NoofPeriod = projectDesing.TotalPeriod;
                designDetailsDto.NoofVisit = projectDesing.TotalVisit;
                designDetailsDto.NoofTemplate = projectDesing.TotalTemplate;
                designDetailsDto.GoLiveVersion = projectDesing.ActiveVersion;
                designDetailsDto.TrialVersion = projectDesing.TrialVersion;
                designDetailsDto.NoofECrf = projectDesing.TotalVisitNoCRF;
            }

            projectDetailsDto.WorkFlowDetail = _context.ProjectWorkflowIndependent.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDeisgnId
            && x.ProjectWorkflow.DeletedDate == null && x.DeletedDate == null).Select(t => new BasicWorkFlowDetailsDto
            {
                RoleName = t.SecurityRole.RoleName,
                LevelNo = 0
            }).ToList();

            var workFlowDetail = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDeisgnId
            && x.ProjectWorkflow.DeletedDate == null && x.DeletedDate == null).Select(t => new BasicWorkFlowDetailsDto
            {
                RoleName = t.SecurityRole.RoleName,
                LevelNo = t.LevelNo
            }).ToList();

            projectDetailsDto.WorkFlowDetail.AddRange(workFlowDetail);

            var projectWorkflowId = _context.ProjectWorkflow.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).FirstOrDefault()?.Id;
            workflowDetailsDto.Independent = projectWorkflowId == null ? 0 : _context.ProjectWorkflowIndependent.Count(x => x.ProjectWorkflowId == projectWorkflowId && x.DeletedDate == null);
            workflowDetailsDto.NoofLevels = projectWorkflowId == null ? 0 : _context.ProjectWorkflowLevel.Count(x => x.ProjectWorkflowId == projectWorkflowId && x.DeletedDate == null);
            workflowDetailsDto.MarkAsCompleted = _context.ElectronicSignature.Any(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && x.IsCompleteWorkflow == true);

            userRightDetailsDto.NoofUser = _context.ProjectRight.Where(x => x.ProjectId == projectId && x.DeletedDate == null).GroupBy(y => y.UserId).Select(t => t.Key).Count();
            userRightDetailsDto.NoOfDocument = _context.ProjectDocument.Count(x => x.ProjectId == projectId && x.DeletedDate == null);
            userRightDetailsDto.DocumentNotReview = _context.ProjectDocumentReview.Where(x => x.ProjectId == projectId && x.DeletedDate == null && !x.IsReview).GroupBy(y => y.UserId).Select(t => t.Key).Count();


            schedulesDetailsDto.NoofVisit = _context.ProjectSchedule.Where(x => x.ProjectId == projectId && x.DeletedDate == null).GroupBy(y => y.ProjectDesignVisitId).Select(t => t.Key).Count();
            schedulesDetailsDto.NoOfReferenceTemplate = _context.ProjectSchedule.Where(x => x.ProjectId == projectId && x.DeletedDate == null).GroupBy(y => y.ProjectDesignTemplateId).Select(t => t.Key).Count();
            schedulesDetailsDto.NoOfTargetTemplate = _context.ProjectScheduleTemplate.Where(x => x.ProjectSchedule.DeletedDate == null && x.ProjectSchedule.ProjectId == projectId && x.DeletedDate == null).GroupBy(y => y.ProjectDesignTemplateId).Select(t => t.Key).Count();

            editCheckDetailsDto.NoofFormulas = _context.EditCheck.Count(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && x.IsFormula == true);
            editCheckDetailsDto.NoofRules = _context.EditCheck.Count(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && x.IsFormula == false);
            editCheckDetailsDto.NotVerified = _context.EditCheck.Count(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && !x.IsReferenceVerify);
            editCheckDetailsDto.IsAnyRecord = editCheckDetailsDto.NoofFormulas > 0 || editCheckDetailsDto.NoofRules > 0 || editCheckDetailsDto.NotVerified > 0;

            projectDetailsDto.DesignDetails = designDetailsDto;
            projectDetailsDto.UserRightDetails = userRightDetailsDto;
            projectDetailsDto.SchedulesDetails = schedulesDetailsDto;
            projectDetailsDto.EditCheckDetails = editCheckDetailsDto;
            return projectDetailsDto;
        }

        public IList<ProjectGridDto> GetSitesList(int projectId, bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectGridDto>();

            var projects = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ParentProjectId == projectId).
                ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var projectCode = _context.Project.Find(projectId).ProjectCode;
            var projectName = Find(projectId).ProjectName; // Added By Neel
            projects.ForEach(x =>
            {
                x.ParentProjectCode = projectCode;
                x.ProjectName = projectName; // Added By Neel
            });

            //projects.ForEach(x =>
            //{
            //    x.ParentProjectCode = _context.Project.Find(x.ParentProjectId).ProjectCode;
            //    var design = _context.ProjectDesign.Where(t =>
            //        t.ProjectId == (x.ParentProjectId != null ? x.ParentProjectId : x.Id) && t.DeletedDate == null).FirstOrDefault();
            //    if (design != null)
            //    {
            //        x.ProjectDesignId = design.Id;
            //        x.Locked = !design.IsUnderTesting;
            //    }
            //});
            return projects;
        }

        public string GetAutoNumber()
        {
            var projectCode = _numberFormatRepository.GenerateNumber("project");
            var country = "In";
            var design = "007";
            projectCode = projectCode.Replace("DESIGN", design);
            projectCode = projectCode.Replace("COUNTRY", country);

            return projectCode.ToUpper();
        }

        public string GetAutoNumberForSites(int Id)
        {
            var SiteCount = All.Where(x => x.ParentProjectId == Id && x.IsTestSite == false).Count();
            //var SiteCount = All.Where(x => x.ParentProjectId == Id).Count();
            var projectCode = _numberFormatRepository.GenerateNumberForSite("projectchild", SiteCount);
            var country = "In";
            var design = "007";
            projectCode = projectCode.Replace("DESIGN", design);
            projectCode = projectCode.Replace("COUNTRY", country);

            return projectCode.ToUpper();
        }

        public List<ProjectDropDown> GetParentProjectDropDownwithoutRights()
        {

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null)
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).OrderBy(o => o.Value).ToList();
        }

        public void UpdateProject(Data.Entities.Master.Project details)
        {

            var projectdetails = _context.Project.Where(t => t.ProjectCode == details.ProjectCode).FirstOrDefault();
            var user = _context.Users.FirstOrDefault();
            if (projectdetails == null)
            {
                projectdetails = new Data.Entities.Master.Project();
                projectdetails.ProjectCode = details.ProjectCode;
                projectdetails.ProjectName = "";
                projectdetails.ProjectNumber = "";
                //projectdetails.DesignTrialId = 1;
                //projectdetails.CountryId = 1;
                //projectdetails.ClientId = 1;
                //projectdetails.DrugId = 1;
                //  projectdetails.Period = 1;
                //   projectdetails.IsStatic = true;
                projectdetails.FromDate = details.FromDate;
                projectdetails.ToDate = details.ToDate;
                projectdetails.CreatedBy = user.Id;
                projectdetails.CreatedDate = DateTime.Now;
                _context.Project.Add(projectdetails);
                _context.Save();
                Data.Entities.ProjectRight.ProjectRight prights = new Data.Entities.ProjectRight.ProjectRight();
                prights.UserId = user.Id;
                prights.ProjectId = projectdetails.Id;
                prights.RoleId = 1;
                prights.IsPrimary = false;
                prights.IsTrainingRequired = false;
                prights.CreatedBy = user.Id;
                prights.CreatedDate = DateTime.Now;
                prights.IsReviewDone = true;
                _projectRightRepository.Add(prights);
            }
            else
            {
                projectdetails.ProjectCode = details.ProjectCode;
                // projectdetails.ProjectName = "";
                projectdetails.FromDate = details.FromDate;
                projectdetails.ToDate = details.ToDate;
                _context.Project.Update(projectdetails);
            }
            _context.Save();
        }


        public ProjectGridDto GetProjectDetailForDashboard(int ProjectId)
        {
            var projects = All.Where(x => x.ParentProjectId == null && x.Id == ProjectId).
                 ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).FirstOrDefault();

            return projects;
        }

        public List<ProjectDropDown> GetParentProjectDropDownforAE()
        {
            var siteteams = _siteTeamRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null).ToList();
            var childproject = siteteams.Select(x => x.ProjectId).Distinct().ToList();
            var project = All.Where(x => childproject.Contains(x.Id) && x.DeletedDate == null).Select(z => z.ParentProjectId).ToList();
            var data = All.Where(x => project.Contains(x.Id) && x.DeletedDate == null)
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().ToList();
            return data;
        }

        public List<ProjectDropDown> GetChildProjectDropDownforAE(int parentProjectId)
        {
            var siteteams = _siteTeamRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null).ToList();
            var childproject = siteteams.Select(x => x.ProjectId).Distinct().ToList();
            var data = All.Where(x => childproject.Contains(x.Id) && x.ParentProjectId == parentProjectId && x.DeletedDate == null)
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode == null ? c.ManageSite.SiteName : c.ProjectCode + " - " + c.ManageSite.SiteName,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().ToList();
            return data;
        }
        public IList<ProjectDropDown> GetProjectForAttendance(bool isStatic)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var projects = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null
                    && x.IsStatic == isStatic
                    && x.ParentProjectId == null
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode + " - " + c.ProjectName,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    AttendanceLimit = c.AttendanceLimit ?? 0
                }).OrderBy(o => o.Value).ToList();

            return projects;
        }

        public string GetStudyCode(int ProjectId)
        {
            var projectdDetail = All.Where(x => x.Id == ProjectId).Select(i => new { i.ParentProjectId, i.ProjectCode }).FirstOrDefault();
            if (projectdDetail.ParentProjectId != null)
            {
                var projectCode = All.Where(x => x.Id == projectdDetail.ParentProjectId).Select(x => x.ProjectCode).FirstOrDefault();
                return projectCode + "\\" + projectdDetail.ProjectCode;
            }
            return projectdDetail.ProjectCode;
        }

        //Code for clone Study Tinku Mahato (01-04-2022)

        public void CloneStudy(CloneProjectDto cloneProject, Data.Entities.Master.Project project)
        {
            Dictionary<int, int> periodIdMap = new Dictionary<int, int>();
            Dictionary<int, int> visitIdMap = new Dictionary<int, int>();

            var projectDesign = _context.ProjectDesign.FirstOrDefault(q => q.ProjectId == cloneProject.CloneProjectId && q.DeletedDate == null);
            var projectDesignId = projectDesign.Id;

            projectDesign.ProjectId = project.Id;
            projectDesign.Id = 0;
            projectDesign.ModifiedBy = null;
            projectDesign.ModifiedDate = null;
            projectDesign.DeletedBy = null;
            projectDesign.DeletedDate = null;

            _context.ProjectDesign.Add(projectDesign);
            _context.Save();

            var sequence = _context.TemplateVariableSequenceNoSetting.FirstOrDefault(q => q.ProjectDesignId == projectDesignId && q.DeletedDate == null);
            if (sequence != null)
            {
                sequence.Id = 0;
                sequence.ProjectDesignId = projectDesign.Id;
                _context.TemplateVariableSequenceNoSetting.Add(sequence);
            }
            else
            {
                var sequences = new TemplateVariableSequenceNoSetting()
                {
                    Id = 0,
                    ProjectDesignId = projectDesign.Id,
                    IsTemplateSeqNo = true,
                    IsVariableSeqNo = true
                };
                _context.TemplateVariableSequenceNoSetting.Add(sequences);
            }

            var version = new StudyVersion();
            version.ProjectId = project.Id;
            version.ProjectDesignId = projectDesign.Id;
            version.VersionNumber = 1;
            version.VersionStatus = VersionStatus.OnTrial;
            version.IsMinor = false;
            version.CreatedBy = _jwtTokenAccesser.UserId;
            version.CreatedDate = _jwtTokenAccesser.GetClientDate();
            _context.StudyVersion.Add(version);
            _context.Save();

            var designPeriods = _context.ProjectDesignPeriod.Where(q => q.ProjectDesignId == projectDesignId && q.DeletedDate == null).ToList();
            foreach (var period in designPeriods)
            {
                var designPeriodId = period.Id;
                period.ProjectDesignId = projectDesign.Id;
                period.DisplayName = period.DisplayName;
                period.Description = period.Description;
                period.Id = 0;
                period.ModifiedBy = null;
                period.ModifiedDate = null;
                period.DeletedBy = null;
                period.DeletedDate = null;

                _context.ProjectDesignPeriod.Add(period);
                _context.Save();

                periodIdMap.Add(designPeriodId, period.Id);

                var projectVisits = _context.ProjectDesignVisit.Where(q => q.ProjectDesignPeriodId == designPeriodId && q.DeletedDate == null && q.InActiveVersion == null).ToList();

                foreach (var visit in projectVisits)
                {
                    var visitId = visit.Id;
                    visit.Id = 0;
                    visit.ProjectDesignPeriodId = period.Id;
                    visit.StudyVersion = null;

                    if (!cloneProject.ScheduleClone)
                        visit.IsSchedule = false;

                    visit.ModifiedBy = null;
                    visit.ModifiedDate = null;
                    visit.DeletedBy = null;
                    visit.DeletedDate = null;
                    _context.ProjectDesignVisit.Add(visit);
                    _context.Save();

                    visitIdMap.Add(visitId, visit.Id);
                    var visitLanguageList = _context.VisitLanguage.Where(q => q.ProjectDesignVisitId == visitId && q.DeletedDate == null).ToList();
                    visitLanguageList.ForEach(m =>
                    {
                        m.Id = 0;
                        m.ProjectDesignVisitId = visit.Id;
                        visit.ModifiedBy = null;
                        visit.ModifiedDate = null;
                        visit.DeletedBy = null;
                        visit.DeletedDate = null;
                        _context.VisitLanguage.Add(m);
                        _context.Save();
                    });

                    var projectTemplates = _context.ProjectDesignTemplate.Where(q => q.ProjectDesignVisitId == visitId && q.DeletedDate == null && q.InActiveVersion == null && q.ParentId == null).ToList();
                    foreach (var template in projectTemplates)
                    {
                        var cloneTemplates = _context.ProjectDesignTemplate.Where(q => q.ProjectDesignVisitId == visitId && q.DeletedDate == null && q.InActiveVersion == null && q.ParentId == template.Id).ToList();

                        var templateId = SaveCloneTemplate(visit.Id, template, null);

                        foreach (var temp in cloneTemplates)
                        {
                            SaveCloneTemplate(visit.Id, temp, templateId);
                        }
                    }

                }
            }

            //WorkFlow
            if (cloneProject.WorkflowClone)
            {
                var workflows = _context.ProjectWorkflow.Where(q => q.ProjectDesignId == projectDesignId && q.DeletedDate == null).ToList();
                foreach (var workflow in workflows)
                {
                    var workflowId = workflow.Id;
                    workflow.ProjectDesignId = projectDesign.Id;
                    workflow.Id = 0;
                    workflow.ModifiedBy = null;
                    workflow.ModifiedDate = null;

                    _context.ProjectWorkflow.Add(workflow);
                    _context.Save();

                    var workflowIndependents = _context.ProjectWorkflowIndependent.Where(q => q.ProjectWorkflowId == workflowId && q.DeletedDate == null).ToList();

                    foreach (var workflowIndependent in workflowIndependents)
                    {
                        workflowIndependent.Id = 0;
                        workflowIndependent.ProjectWorkflowId = workflow.Id;
                        workflowIndependent.ModifiedBy = null;
                        workflowIndependent.ModifiedDate = null;
                        _context.ProjectWorkflowIndependent.Add(workflowIndependent);
                        _context.Save();
                    }

                    var workflowLevels = _context.ProjectWorkflowLevel.Where(q => q.ProjectWorkflowId == workflowId && q.DeletedDate == null).ToList();

                    foreach (var workflowLevel in workflowLevels)
                    {
                        workflowLevel.Id = 0;
                        workflowLevel.ProjectWorkflowId = workflow.Id;
                        workflowLevel.ModifiedBy = null;
                        workflowLevel.ModifiedDate = null;
                        _context.ProjectWorkflowLevel.Add(workflowLevel);
                        _context.Save();
                    }

                }
            }

            // Editcheck Clone
            if (cloneProject.EditcheckClone)
            {
                var editchecks = _context.EditCheck.Where(q => q.ProjectDesignId == projectDesignId && q.DeletedDate == null).ToList();
                foreach (var editcheck in editchecks)
                {
                    if ((!string.IsNullOrEmpty(editcheck.SourceFormula) && !string.IsNullOrEmpty(editcheck.TargetFormula)) || editcheck.IsOnlyTarget)
                    {
                        var editcheckId = editcheck.Id;
                        editcheck.ProjectDesignId = projectDesign.Id;
                        editcheck.Id = 0;
                        editcheck.ModifiedBy = null;
                        editcheck.ModifiedDate = null;
                        _context.EditCheck.Add(editcheck);
                        _context.Save();

                        var editcheckDetails = _context.EditCheckDetail.Where(q => q.EditCheckId == editcheckId && q.DeletedDate == null
                        && _context.ProjectDesignTemplate.Any(pdt => pdt.Id == q.ProjectDesignTemplateId && pdt.DeletedDate == null && pdt.InActiveVersion == null)
                        && _context.ProjectDesignVariable.Any(pd => pd.Id == q.ProjectDesignVariableId && pd.DeletedDate == null && pd.InActiveVersion==null)).ToList();

                        foreach (var editcheckDetail in editcheckDetails)
                        {
                            editcheckDetail.Id = 0;
                            editcheckDetail.EditCheckId = editcheck.Id;
                            editcheckDetail.ModifiedBy = null;
                            editcheckDetail.ModifiedDate = null;

                            //Discuss
                            int value = 0;
                            if (editcheckDetail.ProjectDesignTemplateId != null)
                                templateIdMap.TryGetValue((int)editcheckDetail.ProjectDesignTemplateId, out value);
                            editcheckDetail.ProjectDesignTemplateId = editcheckDetail.ProjectDesignTemplateId != null ? value : editcheckDetail.ProjectDesignTemplateId;

                            if (editcheckDetail.ProjectDesignVariableId != null)
                                variableIdMap.TryGetValue((int)editcheckDetail.ProjectDesignVariableId, out value);
                            editcheckDetail.ProjectDesignVariableId = editcheckDetail.ProjectDesignVariableId != null ? value : editcheckDetail.ProjectDesignVariableId;

                            if (editcheckDetail.FetchingProjectDesignTemplateId != null)
                                templateIdMap.TryGetValue((int)editcheckDetail.FetchingProjectDesignTemplateId, out value);
                            editcheckDetail.FetchingProjectDesignTemplateId = editcheckDetail.FetchingProjectDesignTemplateId != null ? value : editcheckDetail.FetchingProjectDesignTemplateId;

                            if (editcheckDetail.FetchingProjectDesignTemplateId != null)
                                variableIdMap.TryGetValue((int)editcheckDetail.FetchingProjectDesignVariableId, out value);
                            editcheckDetail.FetchingProjectDesignVariableId = editcheckDetail.FetchingProjectDesignVariableId != null ? value : editcheckDetail.FetchingProjectDesignVariableId;
                           if (editcheckDetail.ProjectDesignVariableId != null)
                                {
                                        CollectionSources collectionSource = new CollectionSources();

                                        if (editcheckDetail.CheckBy == EditCheckRuleBy.ByVariable || editcheckDetail.CheckBy == EditCheckRuleBy.ByVariableRule)
                                            collectionSource = _context.ProjectDesignVariable.Find(editcheckDetail.ProjectDesignVariableId).CollectionSource;
                                        if (editcheckDetail.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                                            collectionSource = _context.ProjectDesignVariable.Where(x => x.DeletedDate == null && x.Annotation == editcheckDetail.VariableAnnotation && x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId).FirstOrDefault().CollectionSource;

                                        if (collectionSource == CollectionSources.ComboBox || collectionSource == CollectionSources.RadioButton || collectionSource == CollectionSources.CheckBox
                                               || collectionSource == CollectionSources.MultiCheckBox)
                                        {
                                            if (editcheckDetail.Operator != Operator.In && editcheckDetail.Operator != Operator.NotIn && collectionSource != CollectionSources.MultiCheckBox)
                                            {
                                                if (editcheckDetail.CollectionValue != null)
                                                    variableValueIdMap.TryGetValue(Convert.ToInt32(editcheckDetail.CollectionValue), out value);
                                                editcheckDetail.CollectionValue = editcheckDetail.CollectionValue != null ? value.ToString() : editcheckDetail.CollectionValue;

                                                if (editcheckDetail.CollectionValue2 != null)
                                                    variableValueIdMap.TryGetValue(Convert.ToInt32(editcheckDetail.CollectionValue2), out value);
                                                editcheckDetail.CollectionValue2 = editcheckDetail.CollectionValue2 != null ? value.ToString() : editcheckDetail.CollectionValue2;
                                            }
                                            else
                                            {
                                                if (editcheckDetail.CollectionValue != null)
                                                {
                                                    var CollectionValues = editcheckDetail.CollectionValue.Split(',').ToArray();

                                                    string values = "";
                                                    int index = 0;
                                                    foreach (var item in CollectionValues)
                                                    {
                                                        variableValueIdMap.TryGetValue(Convert.ToInt32(item), out value);
                                                        if (index == 0)
                                                            values = value.ToString();
                                                        else
                                                            values = values + ',' + value;
                                                        index++;
                                                    }

                                                    editcheckDetail.CollectionValue = values;
                                                }

                                                if (editcheckDetail.CollectionValue2 != null)
                                                {
                                                    var CollectionValues = editcheckDetail.CollectionValue2.Split(',').ToArray();

                                                    string values = "";
                                                    int index = 0;
                                                    foreach (var item in CollectionValues)
                                                    {
                                                        variableValueIdMap.TryGetValue(Convert.ToInt32(item), out value);
                                                        if (index == 0)
                                                            values = value.ToString();
                                                        else
                                                            values = values + ',' + value;
                                                        index++;
                                                    }

                                                    editcheckDetail.CollectionValue2 = values;
                                                }
                                            }
                                        }
                                }
                                _context.EditCheckDetail.Add(editcheckDetail);
                            _context.Save();
                        }
                    }
                }
            }

            // Schedule Clone
            if (cloneProject.ScheduleClone)
            {
                var projectSchedules = _context.ProjectSchedule.Where(q => q.ProjectDesignId == projectDesignId && q.DeletedDate == null
                && _context.ProjectDesignVisit.Any(pdv => pdv.Id == q.ProjectDesignVisitId && pdv.DeletedDate == null && pdv.InActiveVersion == null)
                    && _context.ProjectDesignTemplate.Any(pdt => pdt.Id == q.ProjectDesignTemplateId && pdt.DeletedDate == null && pdt.InActiveVersion == null)
                    && _context.ProjectDesignVariable.Any(pd => pd.Id == q.ProjectDesignVariableId && pd.DeletedDate == null && pd.InActiveVersion == null)).ToList();
                foreach (var projectSchedule in projectSchedules)
                {
                    var projectScheduleId = projectSchedule.Id;
                    projectSchedule.ProjectDesignId = projectDesign.Id;
                    projectSchedule.ProjectId = project.Id;
                    projectSchedule.Id = 0;
                    projectSchedule.ModifiedBy = null;
                    projectSchedule.ModifiedDate = null;

                    //Discuss 
                    int value = 0;

                    periodIdMap.TryGetValue(projectSchedule.ProjectDesignPeriodId, out value);
                    projectSchedule.ProjectDesignPeriodId = value;

                    visitIdMap.TryGetValue(projectSchedule.ProjectDesignVisitId, out value);
                    projectSchedule.ProjectDesignVisitId = value;

                    templateIdMap.TryGetValue(projectSchedule.ProjectDesignTemplateId, out value);
                    projectSchedule.ProjectDesignTemplateId = value;

                    variableIdMap.TryGetValue(projectSchedule.ProjectDesignVariableId, out value);
                    projectSchedule.ProjectDesignVariableId = value;

                    _context.ProjectSchedule.Add(projectSchedule);
                    _context.Save();

                    var projectScheduleTemplates = _context.ProjectScheduleTemplate.Where(q => q.ProjectScheduleId == projectScheduleId && q.DeletedDate == null
                    && _context.ProjectDesignVisit.Any(pdv=>pdv.Id == q.ProjectDesignVisitId && pdv.DeletedDate == null && pdv.InActiveVersion == null)
                    && _context.ProjectDesignTemplate.Any(pdt => pdt.Id == q.ProjectDesignTemplateId && pdt.DeletedDate == null && pdt.InActiveVersion==null)
                    && _context.ProjectDesignVariable.Any(pd => pd.Id == q.ProjectDesignVariableId && pd.DeletedDate == null && pd.InActiveVersion == null)).ToList();

                    foreach (var projectScheduleTemplate in projectScheduleTemplates)
                    {
                        projectScheduleTemplate.Id = 0;
                        projectScheduleTemplate.ProjectScheduleId = projectSchedule.Id;
                        projectScheduleTemplate.ModifiedBy = null;
                        projectScheduleTemplate.ModifiedDate = null;

                        //Discuss 

                        periodIdMap.TryGetValue(projectScheduleTemplate.ProjectDesignPeriodId, out value);
                        projectScheduleTemplate.ProjectDesignPeriodId = value;

                        visitIdMap.TryGetValue(projectScheduleTemplate.ProjectDesignVisitId, out value);
                        projectScheduleTemplate.ProjectDesignVisitId = value;

                        templateIdMap.TryGetValue(projectScheduleTemplate.ProjectDesignTemplateId, out value);
                        projectScheduleTemplate.ProjectDesignTemplateId = value;

                        variableIdMap.TryGetValue(projectScheduleTemplate.ProjectDesignVariableId, out value);
                        projectScheduleTemplate.ProjectDesignVariableId = value;

                        _context.ProjectScheduleTemplate.Add(projectScheduleTemplate);
                        _context.Save();
                    }
                }
            }
        }


        private int SaveCloneTemplate(int visitId, ProjectDesignTemplate template, int? parentId)
        {
            var templateId = template.Id;
            template.Id = 0;
            template.ProjectDesignVisitId = visitId;
            template.ParentId = parentId;
            template.ModifiedBy = null;
            template.ModifiedDate = null;
            template.DeletedBy = null;
            template.DeletedDate = null;
            template.StudyVersion = null;
            _context.ProjectDesignTemplate.Add(template);
            _context.Save();
            templateIdMap.Add(templateId, template.Id);
            var templateLanguage = _context.TemplateLanguage.Where(q => q.ProjectDesignTemplateId == templateId && q.DeletedDate == null).ToList();
            templateLanguage.ForEach(x =>
            {
                x.Id = 0;
                x.ProjectDesignTemplateId = template.Id;
                x.ModifiedBy = null;
                x.ModifiedDate = null;
                x.DeletedBy = null;
                x.DeletedDate = null;
                _context.TemplateLanguage.Add(x);
                _context.Save();
            });

            //var templateWorkflow = _context.WorkflowTemplate.Where(q => q.ProjectDesignTemplateId == templateId && q.DeletedDate == null).ToList();
            //templateWorkflow.ForEach(x =>
            //{
            //    x.Id = 0;
            //    x.ProjectDesignTemplateId = template.Id;
            //    x.ModifiedBy = null;
            //    x.ModifiedDate = null;
            //    x.DeletedBy = null;
            //    x.DeletedDate = null;
            //    _context.WorkflowTemplate.Add(x);
            //    _context.Save();
            //});

            var projectDesignNotes = _context.ProjectDesignTemplateNote.Where(q => q.ProjectDesignTemplateId == templateId && q.DeletedDate == null).ToList();
            projectDesignNotes.ForEach(x =>
            {
                var designNoteId = x.Id;
                x.Id = 0;
                x.ProjectDesignTemplateId = template.Id;
                x.ModifiedBy = null;
                x.ModifiedDate = null;
                x.DeletedBy = null;
                x.DeletedDate = null;
                _context.ProjectDesignTemplateNote.Add(x);
                _context.Save();

                var templateNoteLanguages = _context.TemplateNoteLanguage.Where(q => q.ProjectDesignTemplateNoteId == designNoteId && q.DeletedDate == null).ToList();
                templateNoteLanguages.ForEach(q =>
                {
                    q.Id = 0;
                    q.ProjectDesignTemplateNoteId = x.Id;
                    q.ModifiedBy = null;
                    q.ModifiedDate = null;
                    q.DeletedBy = null;
                    q.DeletedDate = null;
                    _context.TemplateNoteLanguage.Add(q);
                    _context.Save();
                });
            });


            var projectDesignVariables = _context.ProjectDesignVariable.Where(q => q.ProjectDesignTemplateId == templateId && q.DeletedDate == null && q.InActiveVersion == null).ToList();
            projectDesignVariables.ForEach(x =>
            {
                var variableId = x.Id;
                x.Id = 0;
                x.ProjectDesignTemplateId = template.Id;
                x.ModifiedBy = null;
                x.ModifiedDate = null;
                x.DeletedBy = null;
                x.DeletedDate = null;
                x.StudyVersion = null;
                _context.ProjectDesignVariable.Add(x);
                _context.Save();
                variableIdMap.Add(variableId, x.Id);

                var projectDesignVariableValues = _context.ProjectDesignVariableValue.Where(q => q.ProjectDesignVariableId == variableId && q.DeletedDate == null && q.InActiveVersion == null).ToList();
                projectDesignVariableValues.ForEach(s =>
                {
                    var varialbeValueId = s.Id;
                    s.Id = 0;
                    s.ProjectDesignVariableId = x.Id;
                    s.ModifiedBy = null;
                    s.ModifiedDate = null;
                    s.DeletedBy = null;
                    s.DeletedDate = null;
                    s.StudyVersion = null;
                    _context.ProjectDesignVariableValue.Add(s);
                    _context.Save();
                    variableValueIdMap.Add(varialbeValueId, s.Id);
                    var varialbeValueLanguages = _context.VariableValueLanguage.Where(q => q.ProjectDesignVariableValueId == varialbeValueId && q.DeletedDate == null).ToList();
                    varialbeValueLanguages.ForEach(m =>
                    {
                        m.Id = 0;
                        m.ProjectDesignVariableValueId = s.Id;
                        m.ModifiedBy = null;
                        m.ModifiedDate = null;
                        m.DeletedBy = null;
                        m.DeletedDate = null;
                        _context.VariableValueLanguage.Add(m);
                        _context.Save();
                    });
                });

                var projectDesignVariableRemarks = _context.ProjectDesignVariableRemarks.Where(q => q.ProjectDesignVariableId == variableId && q.DeletedDate == null).ToList();
                projectDesignVariableRemarks.ForEach(s =>
                {
                    s.Id = 0;
                    s.ProjectDesignVariableId = x.Id;
                    s.ModifiedBy = null;
                    s.ModifiedDate = null;
                    s.DeletedBy = null;
                    s.DeletedDate = null;
                    _context.ProjectDesignVariableRemarks.Add(s);
                    _context.Save();
                });

                var variableLanguages = _context.VariableLanguage.Where(q => q.ProjectDesignVariableId == variableId && q.DeletedDate == null).ToList();
                variableLanguages.ForEach(s =>
                {
                    s.Id = 0;
                    s.ProjectDesignVariableId = x.Id;
                    s.ModifiedBy = null;
                    s.ModifiedDate = null;
                    s.DeletedBy = null;
                    s.DeletedDate = null;
                    _context.VariableLanguage.Add(s);
                    _context.Save();
                });

                var variableNoteLanguages = _context.VariableNoteLanguage.Where(q => q.ProjectDesignVariableId == variableId && q.DeletedDate == null).ToList();
                variableNoteLanguages.ForEach(s =>
                {
                    s.Id = 0;
                    s.ProjectDesignVariableId = x.Id;
                    s.ModifiedBy = null;
                    s.ModifiedDate = null;
                    s.DeletedBy = null;
                    s.DeletedDate = null;
                    _context.VariableNoteLanguage.Add(s);
                    _context.Save();
                });
            });

            return template.Id;
        }
        //---------------------Code End-------------------------

        public string GetParentProjectCode(int ProjectId)
        {
            return All.Where(x => x.Id == ProjectId).Select(x => x.ProjectCode).FirstOrDefault();
        }

        public List<ProjectDropDown> GetParentProjectDropDownForAddProjectNo()
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ProjectCode != null && !x.IsStatic
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetParentProjectCTMSDropDown()
        {
            var projectList = _projectRightRepository.GetProjectCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;
            var projectsctms = _context.ProjectSettings.Where(x => x.IsCtms == true && x.DeletedDate == null && projectList.Contains(x.ProjectId)).Select(x => x.ProjectId).ToList();
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ProjectCode != null
                    && projectsctms.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }
        public List<ProjectDropDown> GetEditParentProjectCTMSDropDown()
        {
            var projectList = _projectRightRepository.GetProjectCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;
            var projectsctms = _context.ProjectSettings.Where(x => x.IsCtms == true && projectList.Contains(x.ProjectId)).Select(x => x.ProjectId).ToList();
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ProjectCode != null
                    && projectsctms.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id
                }).Distinct().OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetParentProjectCTMSTrueDropDown()
        {
            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;
            var projectsctms = _context.ProjectSettings.Where(x => x.IsCtms == true && x.DeletedDate == null && projectList.Contains(x.ProjectId)).Select(x => x.ProjectId).ToList();
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ProjectCode != null
                    && projectsctms.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }
     
        public List<ProjectDropDown> GetParentStaticProjectDropDownIWRS()
        {

            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var list = _context.RandomizationNumberSettings.Where(x => x.DeletedDate == null && projectList.Contains(x.ProjectId) && x.IsIGT == true).Select(x => x.ProjectId).ToList();
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null
                    && list.Contains(x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    IsSendEmail = c.IsSendEmail,
                    IsSendSMS = c.IsSendSMS,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }
        public List<ProjectDropDown> GetProjectDropDownIWRS()
        {

            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var list = _context.RandomizationNumberSettings.Where(x => x.DeletedDate == null && projectList.Contains(x.ProjectId) && x.IsIWRS == true).Select(x => x.ProjectId).ToList();
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null
                    && list.Contains(x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    IsSendEmail = c.IsSendEmail,
                    IsSendSMS = c.IsSendSMS,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }
        public List<ProjectDropDown> GetProjectDropDownIWRSUnblind()
        {

            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var list = _context.RandomizationNumberSettings.Where(x => x.DeletedDate == null && projectList.Contains(x.ProjectId) && x.IsIWRS == true).Select(x => x.ProjectId).ToList();
            if (list == null && list.Count == 0)
                return null;
            var numbersetting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.IsBlindedStudy == true && list.Contains(x.ProjectId)).Select(x => x.ProjectId).ToList();
            if (numbersetting == null || numbersetting.Count == 0)
                return null;

            var liveVersion = _context.StudyVersion.Where(s => s.DeletedDate == null && numbersetting.Contains(s.ProjectId)).Select(s => s.ProjectId).ToList();

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null
                    && liveVersion.Contains(x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    IsSendEmail = c.IsSendEmail,
                    IsSendSMS = c.IsSendSMS,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }
        public List<ProjectDropDown> GetChildProjectDropDownIWRS(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null && x.ParentProjectId == parentProjectId
                    && x.IsTestSite == false
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
                    AttendanceLimit = c.AttendanceLimit ?? 0, //Add for site limt (Tinku Mahato)
                }).OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetLiveProjectDropDownIWRS()
        {

            var projectList = _projectRightRepository.GetParentProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var list = _context.RandomizationNumberSettings.Where(x => x.DeletedDate == null && projectList.Contains(x.ProjectId) && x.IsIGT == true).Select(x => x.ProjectId).ToList();

            var liveVersion = _context.StudyVersion.Where(s => s.DeletedDate == null && list.Contains(s.ProjectId) && s.VersionStatus == VersionStatus.GoLive).Select(s => s.ProjectId).ToList();

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && x.ProjectCode != null
                    && liveVersion.Contains(x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    IsSendEmail = c.IsSendEmail,
                    IsSendSMS = c.IsSendSMS,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).Distinct().OrderBy(o => o.Value).ToList();
        }

    }
}