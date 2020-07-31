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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Respository.Master
{
    public class ProjectRepository : GenericRespository<Data.Entities.Master.Project, GscContext>, IProjectRepository
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


        public ProjectRepository(IUnitOfWork<GscContext> uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            INumberFormatRepository numberFormatRepository,
            ICountryRepository countryRepository,
            IDesignTrialRepository designTrialRepository,
            IProjectRightRepository projectRightRepository,
            IAttendanceRepository attendanceRepository,
            IVolunteerRepository volunteerRepository)
            : base(uow, jwtTokenAccesser)
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
        }

        public IList<ProjectDto> GetProjectList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectDto>();

            var projects = FindBy(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ParentProjectId == null
                && projectList.Any(c => c == x.Id)
            ).Select(x => new ProjectDto
            {
                Id = x.Id,
                ProjectCode = x.ProjectCode,
                ProjectName = x.ProjectName,
                ProjectNumber = x.ProjectNumber,
                ParentProjectId = x.ParentProjectId,
                DesignTrialId = x.DesignTrialId,
                CountryId = x.CountryId,
                ClientId = x.ClientId,
                DrugId = x.DrugId,
                Period = x.Period,
                RegulatoryType = x.RegulatoryType,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                CompanyId = x.CompanyId,
                ParentProjectName =
                    x.ParentProjectId == null ? "" : Context.Project.Find(x.ParentProjectId).ProjectName,
                DesignTrialName = Context.DesignTrial.Find(x.DesignTrialId).DesignTrialName,
                TrialTypeId = Context.DesignTrial.Find(x.DesignTrialId).TrialTypeId,
                CountryName = Context.Country.Find(x.CountryId).CountryName,
                ClientName = Context.Client.Find(x.ClientId).ClientName,
                DrugName = Context.Drug.Find(x.DrugId).DrugName,
                RegulatoryTypeName = x.RegulatoryType.GetDescription(),
                StateName = x.StateId == null ? "" : Context.State.Find(x.StateId).StateName,
                CityName = x.CityId == null ? "" : Context.City.Find(x.CityId).CityName,
                AreaName = x.CityAreaId == null ? "" : Context.CityArea.Find(x.CityAreaId).AreaName,
                SiteName = x.SiteName,
                PinCode = x.PinCode,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate,
                DeletedDate = x.DeletedDate,
                ModifiedBy = x.ModifiedBy,
                DeletedBy = x.DeletedBy,
                IsDeleted = x.DeletedDate != null,
                AttendanceLimit = x.AttendanceLimit,
                NoofSite = GetNoOfSite(x.Id),
            }).OrderByDescending(x => x.Id).ToList();
            foreach (var b in projects)
            {
                b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            }
            projects.ForEach(x =>
            {
                var design = Context.ProjectDesign.Where(t =>
                    t.ProjectId == (x.ParentProjectId != null ? x.ParentProjectId : x.Id) && t.DeletedDate == null).FirstOrDefault();
                if (design != null)
                {
                    x.ProjectDesignId = design.Id;
                    x.Locked = !design.IsUnderTesting;
                }
            });
            return projects;
        }


        public List<DropDownDto> GetProjectDropDown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                                                                                        && projectList.Any(c =>
                                                                                            c == x.Id))
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.ProjectCode + " - " + c.ProjectName,
                    Code = c.ProjectCode,
                    ExtraData = c.ParentProjectId
                }).OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetSiteByParentProjectIdDropDown(int parentProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null && x.ParentProjectId == null
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode + " - " + c.ProjectName,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id
                }).OrderBy(o => o.Value).ToList();
        }
        public List<ProjectDropDown> GetParentProjectDropDown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.ParentProjectId == null
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    //Value = c.ProjectCode + " - " + c.ProjectName,
                    Value = c.ProjectCode, //change by swati on 31-7-2020 for show only study code
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic,
                    ParentProjectId = c.ParentProjectId ?? c.Id,
                    IsDeleted = c.DeletedDate != null
                }).OrderBy(o => o.Value).ToList();
        }

        public IList<ProjectDropDown> GetProjectForAttendance(bool isStatic)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var projects = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null
                    && x.IsStatic == isStatic
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

        public List<DropDownDto> GetProjectNumberDropDown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                                                                                        && projectList.Any(c =>
                                                                                            c == x.Id))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ProjectNumber, Code = c.ProjectCode })
                .OrderBy(o => o.Value).ToList();
        }

        public void Save(Data.Entities.Master.Project project)
        {
            if (project.ParentProjectId == null)
            {
                var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "pro" && x.DeletedDate == null).FirstOrDefault();
                project.ProjectCode = numberFormat.IsManual ? project.ProjectCode : GetProjectCode(project);
            }
            else
            {
                var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "prochild" && x.DeletedDate == null).FirstOrDefault();
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
        }

        public string Duplicate(Data.Entities.Master.Project objSave)
        {
            if (objSave.ParentProjectId != null || objSave.ParentProjectId <= 0)
            {
                if (All.Any(x => x.Id != objSave.Id && x.ParentProjectId == objSave.ParentProjectId && x.ProjectCode == objSave.ProjectCode && x.DeletedDate == null))
                    return "Duplicate Project Code : " + objSave.ProjectCode;
            }

            if (objSave.ParentProjectId == null || objSave.ParentProjectId <= 0)
            {
                if (All.AsNoTracking().Any(x =>
                    x.Id != objSave.Id && x.ProjectName == objSave.ProjectName && x.DeletedDate == null))
                    return "Duplicate Project name : " + objSave.ProjectName;

                if (All.Any(x => x.Id != objSave.Id && x.ProjectCode == objSave.ProjectCode && x.DeletedDate == null))
                    return "Duplicate Project Code : " + objSave.ProjectCode;
            }

            if (objSave.Id > 0 && objSave.AttendanceLimit != null && !objSave.IsStatic)
            {
                var attendantCount = Context.Attendance.Count(x => x.DeletedDate == null
                                                                   && x.AttendanceType == AttendanceType.Project
                                                                   && x.Status != AttendaceStatus.Suspended
                                                                   && !x.IsStandby
                                                                   && x.ProjectId == objSave.Id && x.PeriodNo == 1);

                if (objSave.AttendanceLimit < attendantCount)
                    return "Can't reduce attendance limit, already taken attendanced";
            }

            if (objSave.Id > 0)
                if (objSave.IsStatic != All.AsNoTracking().Any(x => x.Id == objSave.Id && x.IsStatic) &&
                    Context.ProjectDesign.Any(x => x.ProjectId == objSave.Id && x.DeletedDate == null))
                    return "Can't IsStatic value, already started project design!";
            return "";
        }

        public async Task<ProjectDetailDto> GetProjectDetailWithPeriod(int projectId)
        {
            return await All.Where(x => x.Id == projectId)
                .Include(t => t.ProjectDesigns)
                .ThenInclude(th => th.ProjectDesignPeriods)
                .Where(p => p.DeletedDate == null)
                .Select(c => new ProjectDetailDto
                {
                    ProjectId = c.Id,
                    ProjectName = c.ProjectName,
                    ProjectCode = c.ProjectCode,
                    ProjectNumber = c.ProjectNumber,
                    ProjectDesignPeriod = c.ProjectDesigns
                        .SelectMany(pd => pd.ProjectDesignPeriods)
                        .Select(pp => new ProjectDesignPeriodDto
                        {
                            DisplayName = pp.DisplayName,
                            Description = pp.Description,
                            ProjectDesignId = pp.Id
                        })
                })
                .FirstOrDefaultAsync();
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

        public IList<ProjectDropDown> GetProjectsByLock(bool isLock)
        {
            var projectIds = _projectRightRepository.GetProjectRightIdList();
            if (!projectIds.Any()) return new List<ProjectDropDown>();
            var projects = new List<ProjectDropDown>();

            var screeninglockAudit = Context.ScreeningTemplateLockUnlockAudit.ToList();
            if (isLock)
            {
                projects = All.Where(x =>
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

                var lstproject = All.Where(x =>
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

                try
                {
                    foreach (var item in lstproject)
                    {
                        if (!item.IsStatic)
                        {
                            var attendanceSearch = new ScreeningSearhParamDto();
                            attendanceSearch.ProjectId = item.Id;
                            attendanceSearch.AttendanceType = AttendanceType.Project;
                            attendanceSearch.PeriodNo = 0;
                            var volunteers = _attendanceRepository.GetAttendaceListByLock(attendanceSearch, isLock);
                            if (volunteers.Count == 0)
                            {
                                projects.RemoveAll(x => x.Id == item.Id);
                            }
                        }
                        else
                        {
                            var projectDesignPeriod = Context.ProjectDesign.Where(x => x.ProjectId == item.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
                            var projectDesignPeriodId = projectDesignPeriod != null ? Context.ProjectDesignPeriod.Where(x => x.DeletedDate == null && x.ProjectDesignId == projectDesignPeriod.Id).FirstOrDefault() != null ? Context.ProjectDesignPeriod.Where(x => x.DeletedDate == null && x.ProjectDesignId == projectDesignPeriod.Id).FirstOrDefault().Id : 0 : 0;
                            var volunteers = _volunteerRepository.getVolunteersForDataEntryByPeriodIdLocked(projectDesignPeriodId, item.Id, isLock);
                            if (volunteers == null || volunteers.Count == 0)
                            {
                                projects.RemoveAll(x => x.Id == item.Id);
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                projects = (from project in Context.Project.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && projectIds.Any(c => c == x.Id))
                            join locktemplate in screeninglockAudit.GroupBy(x => new { x.ScreeningEntryId, x.ScreeningTemplateId })
                            .Select(y => new LockUnlockListDto
                            {
                                Id = y.LastOrDefault().Id,
                                screeningEntryId = y.Key.ScreeningEntryId,
                                ProjectId = y.LastOrDefault().ProjectId,
                                ScreeningTemplateId = y.LastOrDefault().ScreeningTemplateId,
                                IsLocked = y.LastOrDefault().IsLocked
                            }).Where(x => x.IsLocked).ToList()
                          on project.Id equals locktemplate.ProjectId
                            group project by project.Id into gcs
                            select new ProjectDropDown
                            {
                                Id = gcs.Key,
                                Value = gcs.FirstOrDefault().ProjectCode + " - " + gcs.FirstOrDefault().ProjectName,
                                IsStatic = gcs.FirstOrDefault().IsStatic,
                                ParentProjectId = gcs.FirstOrDefault().ParentProjectId ?? gcs.FirstOrDefault().Id
                            }).OrderBy(o => o.Value).ToList();
            }
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
                    Value = c.ProjectCode + " - " + c.ProjectName,
                    Code = c.ProjectCode,
                    IsStatic = c.IsStatic
                }).OrderBy(o => o.Value).ToList();
        }

        public List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int ProjectDesignId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var ParentProjectId = Context.ProjectDesign.Find(ProjectDesignId).ProjectId;

            return All.Where(x =>
                    ((x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && x.DeletedDate == null && x.ParentProjectId == ParentProjectId)
                    && projectList.Any(c => c == x.Id))
                .Select(c => new ProjectDropDown
                {
                    Id = c.Id,
                    Value = c.ProjectCode + " - " + c.ProjectName,
                    CountryId = c.CountryId,
                    IsStatic = c.IsStatic
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
            var childData = All.Where(x => x.ParentProjectId == id && x.DeletedDate == null).ToList();

            if (childData.Count == 0)
                return 0;
            return childData.Count();
        }

        private string GetProjectCode(Data.Entities.Master.Project project)
        {
            if (project.ParentProjectId == null)
            {
                var projectCode = _numberFormatRepository.GenerateNumber("pro");
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
            var projectCode = _numberFormatRepository.GenerateNumber("prochild");
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
            var a = GetNoOfSite(projectId);

            siteDetailsDto.NoofSite = GetNoOfSite(projectId);
            siteDetailsDto.NoofCountry = All.Where(x => x.ParentProjectId == projectId && x.DeletedDate == null).ToList().GroupBy(x => x.CountryId).Count();
            siteDetailsDto.MarkAsCompleted = All.Where(x => x.ParentProjectId == projectId && x.DeletedDate == null).Any();

            var projectDeisgnId = Context.ProjectDesign.Where(x => x.ProjectId == projectId && x.DeletedDate == null).FirstOrDefault()?.Id;
            designDetailsDto.NoofPeriod = projectDeisgnId == null ? 0 : Context.ProjectDesignPeriod.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).ToList().Count();
            designDetailsDto.NoofVisit = projectDeisgnId == null ? 0 : GetNoOfVisit(projectDeisgnId);
            designDetailsDto.NoofECrf = projectDeisgnId == null ? 0 : GetNoOfTemplate(projectDeisgnId);
            designDetailsDto.MarkAsCompleted = projectDeisgnId == null ? false : Context.ProjectDesign.Where(x => x.ProjectId == projectId).FirstOrDefault()?.IsCompleteDesign;

            var projectWorkflowId = Context.ProjectWorkflow.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).FirstOrDefault()?.Id;
            workflowDetailsDto.Independent = projectWorkflowId == null ? 0 : Context.ProjectWorkflowIndependent.Where(x => x.ProjectWorkflowId == projectWorkflowId && x.DeletedDate == null).ToList().Count();
            workflowDetailsDto.NoofLevels = projectWorkflowId == null ? 0 : Context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == projectWorkflowId && x.DeletedDate == null).ToList().Count();
            workflowDetailsDto.MarkAsCompleted = Context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).FirstOrDefault()?.IsCompleteWorkflow;

            userRightDetailsDto.NoofUser = Context.ProjectRight.Where(x => x.ProjectId == projectId && x.DeletedDate == null).ToList().GroupBy(y => y.UserId).Count();
            userRightDetailsDto.MarkAsCompleted = Context.ProjectRight.Where(x => x.ProjectId == projectId && x.DeletedDate == null).Any();

            schedulesDetailsDto.NoofVisit = Context.ProjectSchedule.Where(x => x.ProjectId == projectId && x.DeletedDate == null).ToList().GroupBy(y => y.ProjectDesignVisitId).Count();
            schedulesDetailsDto.MarkAsCompleted = Context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).FirstOrDefault()?.IsCompleteSchedule; ;

            editCheckDetailsDto.NoofFormulas = GetNoOfFormulas(projectDeisgnId);
            editCheckDetailsDto.NoofRules = projectDeisgnId == null ? 0 : Context.EditCheck.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).ToList().Count();
            editCheckDetailsDto.MarkAsCompleted = Context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDeisgnId && x.DeletedDate == null).FirstOrDefault()?.IsCompleteEditCheck; ;

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
            var periods = Context.ProjectDesignPeriod.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).ToList();
            var visitCount = 0;

            periods.ForEach(b =>
             {
                 var visit = Context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriodId == b.Id && x.DeletedDate == null).ToList().Count;
                 visitCount += visit;
             });

            return visitCount;
        }

        public int GetNoOfTemplate(int? projectDesignId)
        {
            var periods = Context.ProjectDesignPeriod.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).ToList();
            var temCount = 0;

            periods.ForEach(b =>
            {
                var visit = Context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriodId == b.Id && x.DeletedDate == null).ToList();
                visit.ForEach(v =>
                {
                    var template = Context.ProjectDesignTemplate.Where(x => x.ProjectDesignVisitId == v.Id && x.DeletedDate == null).ToList().Count;
                    temCount += template;
                });
            });

            return temCount;
        }

        public int GetNoOfFormulas(int? projectDesignId)
        {
            var rules = Context.EditCheck.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).ToList();
            var formulasCount = 0;

            rules.ForEach(b =>
            {
                var formula = Context.EditCheckDetail.Where(x => x.EditCheckId == b.Id && x.DeletedDate == null).ToList().Count;
                formulasCount += formula;
            });

            return formulasCount;
        }

        public IList<ProjectDto> GetSitesList(int projectId, bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectDto>();

            var projects = FindBy(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ParentProjectId == projectId
                && projectList.Any(c => c == x.Id)
            ).Select(x => new ProjectDto
            {
                Id = x.Id,
                ProjectCode = x.ProjectCode,
                ProjectName = x.ProjectName,
                ProjectNumber = x.ProjectNumber,
                ParentProjectId = x.ParentProjectId,
                DesignTrialId = x.DesignTrialId,
                CountryId = x.CountryId,
                ClientId = x.ClientId,
                DrugId = x.DrugId,
                Period = x.Period,
                RegulatoryType = x.RegulatoryType,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                CompanyId = x.CompanyId,
                ParentProjectName =
                    x.ParentProjectId == null ? "" : Context.Project.Find(x.ParentProjectId).ProjectName,
                DesignTrialName = Context.DesignTrial.Find(x.DesignTrialId).DesignTrialName,
                TrialTypeId = Context.DesignTrial.Find(x.DesignTrialId).TrialTypeId,
                CountryName = Context.Country.Find(x.CountryId).CountryName,
                ClientName = Context.Client.Find(x.ClientId).ClientName,
                DrugName = Context.Drug.Find(x.DrugId).DrugName,
                RegulatoryTypeName = x.RegulatoryType.GetDescription(),
                StateName = x.StateId == null ? "" : Context.State.Find(x.StateId).StateName,
                CityName = x.CityId == null ? "" : Context.City.Find(x.CityId).CityName,
                AreaName = x.CityAreaId == null ? "" : Context.CityArea.Find(x.CityAreaId).AreaName,
                SiteName = x.SiteName,
                PinCode = x.PinCode,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                ModifiedDate = x.ModifiedDate,
                DeletedDate = x.DeletedDate,
                ModifiedBy = x.ModifiedBy,
                DeletedBy = x.DeletedBy,
                IsDeleted = x.DeletedDate != null,
                AttendanceLimit = x.AttendanceLimit
            }).OrderByDescending(x => x.Id).ToList();
            foreach (var b in projects)
            {
                b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            }
            projects.ForEach(x =>
            {
                var design = Context.ProjectDesign.Where(t =>
                    t.ProjectId == (x.ParentProjectId != null ? x.ParentProjectId : x.Id) && t.DeletedDate == null).FirstOrDefault();
                if (design != null)
                {
                    x.ProjectDesignId = design.Id;
                    x.Locked = !design.IsUnderTesting;
                }
            });
            return projects;
        }

        public string GetAutoNumber()
        {
            var projectCode = _numberFormatRepository.GenerateNumber("pro");
            var country = "In";
            var design = "007";
            projectCode = projectCode.Replace("DESIGN", design);
            projectCode = projectCode.Replace("COUNTRY", country);

            return projectCode.ToUpper();
        }

        public string GetAutoNumberForSites()
        {
            var projectCode = _numberFormatRepository.GenerateNumber("prochild");
            var country = "In";
            var design = "007";
            projectCode = projectCode.Replace("DESIGN", design);
            projectCode = projectCode.Replace("COUNTRY", country);

            return projectCode.ToUpper();
        }
    }
}