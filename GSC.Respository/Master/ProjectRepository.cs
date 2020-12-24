using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Respository.Master
{
    public class ProjectRepository : GenericRespository<Data.Entities.Master.Project>, IProjectRepository
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IDesignTrialRepository _designTrialRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ProjectRepository(IGSCContext context,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            INumberFormatRepository numberFormatRepository,
            ICountryRepository countryRepository,
            IDesignTrialRepository designTrialRepository,
            IProjectRightRepository projectRightRepository,
            IAttendanceRepository attendanceRepository,
            IVolunteerRepository volunteerRepository,
            IMapper mapper)
            : base(context)
        {
            _numberFormatRepository = numberFormatRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _designTrialRepository = designTrialRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _attendanceRepository = attendanceRepository;
            _volunteerRepository = volunteerRepository;
            _mapper = mapper;
            _context = context;
        }

        public IList<ProjectGridDto> GetProjectList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectGridDto>();

            var projects = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && projectList.Contains(x.Id) && x.ParentProjectId == null).
                 ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();


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
                project.ProjectCode = numberFormat.IsManual ? project.ProjectCode : GetProjectSitesCode(project);
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
            if (objSave.ParentProjectId != null || objSave.ParentProjectId <= 0)
            {
                if (All.Any(x => x.Id != objSave.Id && x.ParentProjectId == objSave.ParentProjectId && x.ProjectCode == objSave.ProjectCode.Trim() && x.DeletedDate == null))
                    return "Duplicate Site Code : " + objSave.ProjectCode;
            }

            if (objSave.ParentProjectId == null || objSave.ParentProjectId <= 0)
            {
                if (All.AsNoTracking().Any(x =>
                    x.Id != objSave.Id && x.ProjectName == objSave.ProjectName.Trim() && x.DeletedDate == null && x.ParentProjectId == null))
                    return "Duplicate Study name : " + objSave.ProjectName;

                if (All.Any(x => x.Id != objSave.Id && x.ProjectCode == objSave.ProjectCode.Trim() && x.DeletedDate == null))
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
            int? sum = All.AsNoTracking().Where(t => t.ParentProjectId == objSave.ParentProjectId && t.DeletedDate == null)
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

            int? sum = All.AsNoTracking().Where(t => t.ParentProjectId == objSave.ParentProjectId && t.DeletedDate == null)
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
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
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
                }).OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetParentStaticProjectDropDown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null && x.IsStatic == true
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
                }).OrderBy(o => o.Value).ToList();
        }

        public IList<ProjectDropDown> GetProjectsForDataEntry()
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
                    Value = c.ProjectCode,
                    CountryId = c.CountryId,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? 0
                }).OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int ProjectDesignId)
        {
            var ParentProjectId = _context.ProjectDesign.Where(x => x.Id == ProjectDesignId).Select(t => t.ProjectId).FirstOrDefault();

            return GetChildProjectDropDown(ParentProjectId);
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
                    Value = c.ProjectCode,
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
                var design = _designTrialRepository.Find(project.DesignTrialId).DesignTrialCode;
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

        private string GetProjectSitesCode(Data.Entities.Master.Project project)
        {
            var SiteCount = All.Where(x => x.ParentProjectId == project.ParentProjectId).Count();
            var projectCode = _numberFormatRepository.GenerateNumberForSite("projectchild", SiteCount);
            var country = _countryRepository.Find(project.CountryId).CountryCode;
            var design = _designTrialRepository.Find(project.DesignTrialId).DesignTrialCode;
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
            siteDetailsDto.NoofCountry = All.Where(x => x.ParentProjectId == projectId && x.DeletedDate == null).GroupBy(x => x.CountryId).Select(t => t.Key).Count();
            siteDetailsDto.MarkAsCompleted = All.Any(x => x.ParentProjectId == projectId && x.DeletedDate == null);

            var projectDeisgn = _context.ProjectDesign.Where(x => x.ProjectId == projectId && x.DeletedDate == null).FirstOrDefault();
            var projectDeisgnId = projectDeisgn?.Id;

            designDetailsDto.NoofPeriod = projectDeisgnId == null ? 0 : _context.ProjectDesignPeriod.Count(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null);
            designDetailsDto.NoofVisit = projectDeisgnId == null ? 0 : GetNoOfVisit(projectDeisgnId);
            designDetailsDto.NoofECrf = projectDeisgnId == null ? 0 : GetNoOfTemplate(projectDeisgnId);
            designDetailsDto.MarkAsCompleted = projectDeisgn?.IsCompleteDesign;

            var projectWorkflowId = _context.ProjectWorkflow.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).FirstOrDefault()?.Id;
            workflowDetailsDto.Independent = projectWorkflowId == null ? 0 : _context.ProjectWorkflowIndependent.Count(x => x.ProjectWorkflowId == projectWorkflowId && x.DeletedDate == null);
            workflowDetailsDto.NoofLevels = projectWorkflowId == null ? 0 : _context.ProjectWorkflowLevel.Count(x => x.ProjectWorkflowId == projectWorkflowId && x.DeletedDate == null);
            workflowDetailsDto.MarkAsCompleted = _context.ElectronicSignature.Any(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && x.IsCompleteWorkflow == true);

            userRightDetailsDto.NoofUser = _context.ProjectRight.Where(x => x.ProjectId == projectId && x.DeletedDate == null).GroupBy(y => y.UserId).Select(t => t.Key).Count();
            userRightDetailsDto.MarkAsCompleted = _context.ProjectRight.Any(x => x.ProjectId == projectId && x.DeletedDate == null);

            schedulesDetailsDto.NoofVisit = _context.ProjectSchedule.Where(x => x.ProjectId == projectId && x.DeletedDate == null).GroupBy(y => y.ProjectDesignVisitId).Select(t => t.Key).Count();
            schedulesDetailsDto.MarkAsCompleted = _context.ElectronicSignature.Any(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && x.IsCompleteSchedule == true);

            editCheckDetailsDto.NoofFormulas = GetNoOfFormulas(projectDeisgnId);
            editCheckDetailsDto.NoofRules = projectDeisgnId == null ? 0 : _context.EditCheck.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).ToList().Count();
            editCheckDetailsDto.MarkAsCompleted = _context.ElectronicSignature.Any(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null && x.IsCompleteEditCheck == true);

            projectDetailsDto.siteDetails = siteDetailsDto;
            projectDetailsDto.designDetails = designDetailsDto;
            projectDetailsDto.workflowDetails = workflowDetailsDto;
            projectDetailsDto.userRightDetails = userRightDetailsDto;
            projectDetailsDto.schedulesDetails = schedulesDetailsDto;
            projectDetailsDto.editCheckDetails = editCheckDetailsDto;
            return projectDetailsDto;
        }

        public int GetNoOfVisit(int? projectDesignId)
        {
            return _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.Id == projectDesignId
           && x.DeletedDate == null && x.ProjectDesignPeriod.DeletedDate == null).Count();

        }

        public int GetNoOfTemplate(int? projectDesignId)
        {

            return _context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Id == projectDesignId
                && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate == null && x.ProjectDesignVisit.ProjectDesignPeriod.DeletedDate == null).Count();

        }

        public int GetNoOfFormulas(int? projectDesignId)
        {
            var rules = _context.EditCheck.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).ToList();
            var formulasCount = 0;

            rules.ForEach(b =>
            {
                var formula = _context.EditCheckDetail.Where(x => x.EditCheckId == b.Id && x.DeletedDate == null).ToList().Count;
                formulasCount += formula;
            });

            return formulasCount;
        }

        public IList<ProjectGridDto> GetSitesList(int projectId, bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectGridDto>();

            var projects = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ParentProjectId == projectId).
                ProjectTo<ProjectGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var projectCode = _context.Project.Find(projectId).ProjectCode;

            projects.ForEach(x =>
            {
                x.ParentProjectCode = projectCode;

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
            var SiteCount = All.Where(x => x.ParentProjectId == Id).Count();
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

    }
}