using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ProjectController : BaseController
    {
        private readonly IDesignTrialRepository _designTrialRepository;
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUserRecentItemRepository _userRecentItemRepository;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly ICentreUserService _centreUserService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IRandomizationNumberSettingsRepository _randomizationNumberSettingsRepository;
        private readonly IScreeningNumberSettingsRepository _screeningNumberSettingsRepository;
        private readonly IUserAccessRepository _userAccessRepository;
        private readonly IGSCContext _context;

        public ProjectController(IProjectRepository projectRepository,
            IDesignTrialRepository designTrialRepository,
            IProjectDesignRepository projectDesignRepository,
            IUnitOfWork uow, IMapper mapper,
            IUserRecentItemRepository userRecentItemRepository,
            INumberFormatRepository numberFormatRepository,
            ICentreUserService centreUserService,
            IJwtTokenAccesser jwtTokenAccesser,
            IOptions<EnvironmentSetting> environmentSetting,
            IRandomizationNumberSettingsRepository randomizationNumberSettingsRepository,
            IScreeningNumberSettingsRepository screeningNumberSettingsRepository,
            IUserAccessRepository userAccessRepository,
            IGSCContext context
            )
        {
            _projectRepository = projectRepository;
            _designTrialRepository = designTrialRepository;
            _uow = uow;
            _mapper = mapper;
            _userRecentItemRepository = userRecentItemRepository;
            _numberFormatRepository = numberFormatRepository;
            _projectDesignRepository = projectDesignRepository;
            _centreUserService = centreUserService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _environmentSetting = environmentSetting;
            _randomizationNumberSettingsRepository = randomizationNumberSettingsRepository;
            _screeningNumberSettingsRepository = screeningNumberSettingsRepository;
            _userAccessRepository = userAccessRepository;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var projectsDto = _projectRepository.GetProjectList(isDeleted);
            return Ok(projectsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var project = _projectRepository.Find(id);
            var projectDto = _mapper.Map<ProjectDto>(project);
            if (projectDto.DesignTrialId > 0)
                projectDto.TrialTypeId = _designTrialRepository.Find(projectDto.DesignTrialId).TrialTypeId;
            var projectDesign =
                _projectDesignRepository.FindByInclude(t => t.ProjectId == id && t.DeletedDate == null).FirstOrDefault();
            projectDto.ProjectDesignId = projectDesign == null ? (int?)null : projectDesign.Id;
            if (projectDto.ParentProjectId != null)
                projectDto.ProjectName = _projectRepository.Find((int)projectDto.ParentProjectId).ProjectName;

            _userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            {
                KeyId = project.Id,
                SubjectName = project.ProjectName,
                SubjectName1 = project.ProjectName,
                ScreenType = UserRecent.Project
            });

            return Ok(projectDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProjectDto projectDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectDto.Id = 0;
            var project = _mapper.Map<Data.Entities.Master.Project>(projectDto);
            project.IsSendEmail = true;
            project.IsSendSMS = false;

            if (projectDto.ParentProjectId > 0 && !projectDto.IsTestSite)
            {
                if (_projectRepository.All.Where(x => x.ManageSiteId == projectDto.ManageSiteId && x.ParentProjectId == projectDto.ParentProjectId && x.DeletedDate == null && !x.IsTestSite).ToList().Count > 0)
                {
                    ModelState.AddModelError("Message", "This site is already exist, please select other site.");
                    return BadRequest(ModelState);
                }
                var CheckAttendanceLimit = _projectRepository.CheckAttendanceLimitPost(project);
                if (!string.IsNullOrEmpty(CheckAttendanceLimit))
                {
                    ModelState.AddModelError("Message", CheckAttendanceLimit);
                    return BadRequest(ModelState);
                }
            }

            var validate = _projectRepository.Duplicate(project);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectRepository.Save(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Project failed on save.");
                return BadRequest(ModelState);
            }

            //Add by Mitul on 28-11-2023 -> CTMS on Bydeful Add site add on CTMS Access table
            if (projectDto.ParentProjectId != null)
                _userAccessRepository.AddProjectSiteRight((int)projectDto.ParentProjectId, project.Id);

            ScreeningNumberSettings screeningNumberSettings = new ScreeningNumberSettings();
            screeningNumberSettings.Id = 0;
            screeningNumberSettings.ProjectId = project.Id;
            screeningNumberSettings.IsManualScreeningNo = false;
            screeningNumberSettings.IsSiteDependentScreeningNo = false;
            screeningNumberSettings.IsAlphaNumScreeningNo = false;
            screeningNumberSettings.ScreeningLength = 0;
            screeningNumberSettings.ScreeningNoStartsWith = 0;
            screeningNumberSettings.ScreeningNoseries = 0;
            screeningNumberSettings.PrefixScreeningNo = "";
            _screeningNumberSettingsRepository.Add(screeningNumberSettings);

            RandomizationNumberSettings randomizationNumberSettings = new RandomizationNumberSettings();
            randomizationNumberSettings.Id = 0;
            randomizationNumberSettings.ProjectId = project.Id;
            randomizationNumberSettings.IsManualRandomNo = false;
            randomizationNumberSettings.IsSiteDependentRandomNo = false;
            randomizationNumberSettings.IsAlphaNumRandomNo = false;
            randomizationNumberSettings.RandomNoLength = 0;
            randomizationNumberSettings.RandomNoStartsWith = 0;
            randomizationNumberSettings.RandomizationNoseries = 0;
            randomizationNumberSettings.PrefixRandomNo = "";
            _randomizationNumberSettingsRepository.Add(randomizationNumberSettings);

            _uow.Save();

            _userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            {
                KeyId = project.Id,
                SubjectName = project.ProjectName,
                SubjectName1 = project.ProjectName,
                ScreenType = UserRecent.Project
            });

            _projectRepository.AddDefaultRandomizationEntry(project);

            return Ok(project);
        }


        //Code for clone Study Tinku Mahato (01-04-2022)
        [TransactionRequired]
        [HttpPost("{cloneProjectId}")]
        public IActionResult Post([FromRoute] int cloneProjectId, [FromBody] ProjectDto projectDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectDto.Id = 0;
            var project = _mapper.Map<Data.Entities.Master.Project>(projectDto);
            project.IsSendEmail = true;
            project.IsSendSMS = false;

            if (projectDto.ParentProjectId > 0 && !projectDto.IsTestSite)
            {
                if (_projectRepository.All.Where(x => x.ManageSiteId == projectDto.ManageSiteId && x.ParentProjectId == projectDto.ParentProjectId && x.DeletedDate == null && !x.IsTestSite).ToList().Count > 0)
                {
                    ModelState.AddModelError("Message", "This site is already exist, please select other site.");
                    return BadRequest(ModelState);
                }
                var CheckAttendanceLimit = _projectRepository.CheckAttendanceLimitPost(project);
                if (!string.IsNullOrEmpty(CheckAttendanceLimit))
                {
                    ModelState.AddModelError("Message", CheckAttendanceLimit);
                    return BadRequest(ModelState);
                }
            }

            var validate = _projectRepository.Duplicate(project);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectRepository.Save(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Project failed on save.");
                return BadRequest(ModelState);
            }

            ScreeningNumberSettings screeningNumberSettings = new ScreeningNumberSettings();
            screeningNumberSettings.Id = 0;
            screeningNumberSettings.ProjectId = project.Id;
            screeningNumberSettings.IsManualScreeningNo = false;
            screeningNumberSettings.IsSiteDependentScreeningNo = false;
            screeningNumberSettings.IsAlphaNumScreeningNo = false;
            screeningNumberSettings.ScreeningLength = 0;
            screeningNumberSettings.ScreeningNoStartsWith = 0;
            screeningNumberSettings.ScreeningNoseries = 0;
            screeningNumberSettings.PrefixScreeningNo = "";
            _screeningNumberSettingsRepository.Add(screeningNumberSettings);

            RandomizationNumberSettings randomizationNumberSettings = new RandomizationNumberSettings();
            randomizationNumberSettings.Id = 0;
            randomizationNumberSettings.ProjectId = project.Id;
            randomizationNumberSettings.IsManualRandomNo = false;
            randomizationNumberSettings.IsSiteDependentRandomNo = false;
            randomizationNumberSettings.IsAlphaNumRandomNo = false;
            randomizationNumberSettings.RandomNoLength = 0;
            randomizationNumberSettings.RandomNoStartsWith = 0;
            randomizationNumberSettings.RandomizationNoseries = 0;
            randomizationNumberSettings.PrefixRandomNo = "";
            _randomizationNumberSettingsRepository.Add(randomizationNumberSettings);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Project failed on save.");
                return BadRequest(ModelState);
            }

            if (cloneProjectId != 0)
                projectDto.CloneProjectDto.CloneProjectId = cloneProjectId;
            _projectRepository.CloneStudy(projectDto.CloneProjectDto, project);

            _userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            {
                KeyId = project.Id,
                SubjectName = project.ProjectName,
                SubjectName1 = project.ProjectName,
                ScreenType = UserRecent.Project
            });


            return Ok(project);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] ProjectDto projectDto)
        {
            if (projectDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var project = _mapper.Map<Data.Entities.Master.Project>(projectDto);
            project.Id = projectDto.Id;
            var details = _projectRepository.Find(projectDto.Id);

            project.IsSendEmail = details.IsSendEmail;
            project.IsSendSMS = details.IsSendSMS;

            if (projectDto.ParentProjectId > 0 && !projectDto.IsTestSite)
            {
                if (_projectRepository.All.Where(x => x.ManageSiteId == projectDto.ManageSiteId && x.ParentProjectId == projectDto.ParentProjectId && x.Id != projectDto.Id && x.DeletedDate == null && !x.IsTestSite).ToList().Count > 0)
                {
                    ModelState.AddModelError("Message", "This site is already exist, please select other site.");
                    return BadRequest(ModelState);
                }
                var CheckAttendanceLimit = _projectRepository.CheckAttendanceLimitPut(project);
                if (!string.IsNullOrEmpty(CheckAttendanceLimit))
                {
                    ModelState.AddModelError("Message", CheckAttendanceLimit);
                    return BadRequest(ModelState);
                }
            }

            var validate = _projectRepository.Duplicate(project);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectRepository.Update(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Project failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(project.Id);
        }

        [HttpPut]
        [Route("UpdateSMSEmailConfiguration")]
        public IActionResult UpdateSMSEmailConfiguration([FromBody] SMSEMailConfig projectDto)
        {
            if (projectDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var project = _projectRepository.Find(projectDto.Id);
            project.IsSendEmail = projectDto.IsSendEmail;
            project.IsSendSMS = projectDto.IsSendSMS;

            _projectRepository.Update(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating SMS/Email configuration failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(project.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectRepository.Find(id);

            if (record == null)
                return NotFound();

            var checkChild = _projectRepository.CheckChildProjectExists(id);
            if (!string.IsNullOrEmpty(checkChild))
            {
                ModelState.AddModelError("Message", checkChild);
                return BadRequest(ModelState);
            }

            _projectRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _projectRepository.Find(id);

            if (record == null)
                return NotFound();

            var checkParent = _projectRepository.CheckParentProjectExists(id);
            if (!string.IsNullOrEmpty(checkParent))
            {
                ModelState.AddModelError("Message", checkParent);
                return BadRequest(ModelState);
            }

            var validate = _projectRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet]
        [Route("GetParentProjectDropDown")]
        public IActionResult GetParentProjectDropDown()
        {
            return Ok(_projectRepository.GetParentProjectDropDown());
        }

        [HttpGet]
        [Route("GetParentProjectDropDownEtmf")]
        public IActionResult GetParentProjectDropDownEtmf()
        {
            return Ok(_projectRepository.GetParentProjectDropDownEtmf());
        }

        [HttpGet]
        [Route("GetParentStaticProjectDropDown")]
        public IActionResult GetParentStaticProjectDropDown()
        {
            return Ok(_projectRepository.GetParentStaticProjectDropDown());
        }

        [HttpGet]
        [Route("GetChildProjectDropDown/{parentProjectId}")]
        public IActionResult GetChildProjectDropDown(int parentProjectId)
        {
            return Ok(_projectRepository.GetChildProjectDropDown(parentProjectId));
        }


        [HttpGet]
        [Route("GetProjectsForDataEntry")]
        public IActionResult GetProjectsForDataEntry()
        {
            return Ok(_projectRepository.GetProjectsForDataEntry());
        }

        [HttpGet]
        [Route("GetAllProjectsForDataEntry")]
        public IActionResult GetAllProjectsForDataEntry()
        {
            return Ok(_projectRepository.GetAllProjectsForDataEntry());
        }

        [HttpGet]
        [Route("GetChildProjectWithParentProjectDropDown/{parentProjectId}")]
        public IActionResult GetChildProjectWithParentProjectDropDown(int parentProjectId)
        {
            return Ok(_projectRepository.GetChildProjectWithParentProjectDropDown(parentProjectId));
        }

        [HttpGet("CheckProjectIsStatic/{projectId}")]
        public IActionResult CheckProjectIsStatic(int projectId)
        {
            if (projectId <= 0) return BadRequest();
            var projectStatic = _projectRepository
                .FindBy(t => t.Id == projectId && t.DeletedDate == null).FirstOrDefault();

            var projectStaticDto = _mapper.Map<ProjectDto>(projectStatic);

            return Ok(projectStaticDto);
        }

        [HttpGet]
        [Route("GetProjectDetails/{projectId}")]
        public IActionResult GetProjectDetails(int projectId)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_projectRepository.GetProjectDetails(projectId));
        }

        [HttpGet]
        [Route("GetSites/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetSites(int projectId, bool isDeleted)
        {
            var projectsDto = _projectRepository.GetSitesList(projectId, isDeleted);
            return Ok(projectsDto);
        }

        [HttpGet]
        [Route("GetAutoNumber")]
        public IActionResult GetAutoNumber()
        {
            var autoNumber = _projectRepository.GetAutoNumber();
            ModelState.AddModelError("AutoNumber", autoNumber);
            return Ok(ModelState);
        }

        [HttpGet]
        [Route("CheckNumberFormat")]
        public IActionResult CheckNumberFormat()
        {
            var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "project" && x.DeletedDate == null).FirstOrDefault();
            return Ok(numberFormat);
        }

        [HttpGet]
        [Route("CheckSitesNumberFormat")]
        public IActionResult CheckSitesNumberFormat()
        {
            var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "projectchild" && x.DeletedDate == null).FirstOrDefault();
            return Ok(numberFormat);
        }

        [HttpGet]
        [Route("GetAutoNumberForSites/{Id}")]
        public IActionResult GetAutoNumberForSites(int Id)
        {
            var autoNumber = _projectRepository.GetAutoNumberForSites(Id);
            ModelState.AddModelError("AutoNumber", autoNumber);
            return Ok(ModelState);
        }

        [HttpGet]
        [Route("GetChildProjectRightsDropDown")]
        public IActionResult GetChildProjectRightsDropDown()
        {
            return Ok(_projectRepository.GetChildProjectRightsDropDown());
        }

        [HttpGet]
        [Route("GetParentProjectDropDownwithoutRights")]
        public IActionResult GetParentProjectDropDownwithoutRights()
        {
            return Ok(_projectRepository.GetParentProjectDropDown());
        }

        [HttpGet]
        [Route("GetProjectDetailForDashboard/{projectId}")]
        public IActionResult GetProjectDetailForDashboard(int ProjectId)
        {
            var projectsDto = _projectRepository.GetProjectDetailForDashboard(ProjectId);
            return Ok(projectsDto);
        }

        [HttpGet]
        [Route("GetParentProjectDropDownforAE")]
        public IActionResult GetParentProjectDropDownforAE()
        {
            return Ok(_projectRepository.GetParentProjectDropDownforAE());
        }

        [HttpGet]
        [Route("GetChildProjectDropDownforAE/{parentProjectId}")]
        public IActionResult GetChildProjectDropDownforAE(int parentProjectId)
        {
            return Ok(_projectRepository.GetChildProjectDropDownforAE(parentProjectId));
        }
        [HttpGet]
        [Route("validatenoofStudy")]
        public async Task<IActionResult> ValidatenoofStudy()
        {
            bool IsAddmoreStudy = false;

            var noofstudy = await _centreUserService.Getnoofstudy($"{_environmentSetting.Value.CentralApi}Study/Getnoofstudy/{_jwtTokenAccesser.CompanyId}");
            if (noofstudy != null && noofstudy.ValidTo.Value.Date < DateTime.Now.Date)
            {
                IsAddmoreStudy = false;
            }
            else if (noofstudy != null && noofstudy.ValidFrom.Value.Date > DateTime.Now.Date)
            {
                IsAddmoreStudy = false;
            }
            else
            {
                if (noofstudy != null)
                {
                    int studycount = _projectRepository.FindBy(x => x.ParentProjectId == null
                    && x.CreatedDate.Value.Date >= noofstudy.ValidFrom && x.CreatedDate.Value.Date <= noofstudy.ValidTo && x.DeletedDate == null).Count();
                    if (studycount < noofstudy.NoofStudy)
                        IsAddmoreStudy = true;
                }
            }

            return Ok(IsAddmoreStudy);
        }

        [HttpGet]
        [Route("GetParentProjectDropDownStudyReport")]
        public IActionResult GetParentProjectDropDownStudyReport()
        {
            return Ok(_projectRepository.GetParentProjectDropDownStudyReport());
        }

        [HttpGet]
        [Route("GetProjectForAttendance/{isStatic}")]
        public IActionResult GetProjectForAttendance(bool isStatic)
        {
            return Ok(_projectRepository.GetProjectForAttendance(isStatic));
        }

        [HttpPut]
        [Route("UpdateProjectCodeFromCtms/{ProjectId}/{ProjectCode}")]
        public IActionResult UpdateProjectCodeFromCtms(int ProjectId, string ProjectCode)
        {
            ProjectCode = ProjectCode.Replace("%2F", "/"); //GS1-I2746 : Add by mitul on 05/06/2023
            if (ProjectId <= 0) return BadRequest();

            var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "projectchild" && x.DeletedDate == null).First();

            var project = _projectRepository.FindBy(x => x.Id == ProjectId).First();

            project.ProjectCode = numberFormat.IsManual ? ProjectCode : _projectRepository.GetProjectSitesCode(project);

            _projectRepository.Update(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Update Project Code  failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(project.Id);
        }

        [HttpGet]
        [Route("GetSiteCode/{ProjectId}")]
        public IActionResult GetSiteCode(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();

            var Project = _projectRepository.FindBy(x => x.Id == ProjectId).First();

            var ProjectCode = _projectRepository.GetProjectSitesCode(Project);
            Project.ProjectCode = ProjectCode;
            return Ok(Project);
        }      
        [HttpGet]
        [Route("GetParentProjectDropDownForAddProjectNo")]
        public IActionResult GetParentProjectDropDownForAddProjectNo()
        {
            return Ok(_projectRepository.GetParentProjectDropDownForAddProjectNo());
        }
        [HttpGet]
        [Route("GetParentProjectCTMSDropDown")]
        public IActionResult GetParentProjectCTMSDropDown()
        {
            return Ok(_projectRepository.GetParentProjectCTMSDropDown());
        }
        [HttpGet]
        [Route("GetEditParentProjectCTMSDropDown")]
        public IActionResult GetEditParentProjectCTMSDropDown()
        {
            return Ok(_projectRepository.GetEditParentProjectCTMSDropDown());
        }
        [HttpGet]
        [Route("GetChildProjectCTMSDropDown/{parentProjectId}")]
        public IActionResult GetChildProjectCTMSDropDown(int parentProjectId)
        {
            return Ok(_projectRepository.GetChildProjectCTMSDropDown(parentProjectId));
        }
        [HttpGet]
        [Route("GetParentProjectCTMSTrueDropDown")]
        public IActionResult GetParentProjectCTMSTrueDropDown()
        {
            return Ok(_projectRepository.GetParentProjectCTMSTrueDropDown());
        }

        [HttpGet]
        [Route("GetParentStaticProjectDropDownIWRS")]
        public IActionResult GetParentStaticProjectDropDownIWRS()
        {
            return Ok(_projectRepository.GetParentStaticProjectDropDownIWRS());
        }

        [HttpGet]
        [Route("getProjectStatusData/{ProjectId}")]
        public IActionResult getProjectStatusData(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var Project = _context.ProjectStatus.Where(x => x.ProjectId == ProjectId).FirstOrDefault();
            return Ok(Project);
        }

        [HttpPost("UpdateProjectStatus")]
        public IActionResult UpdateProjectStatus([FromBody] ProjectStatusDto projectDto)
        {
            var project = _mapper.Map<Data.Entities.Master.ProjectStatus>(projectDto);
            project.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            project.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            project.Status = projectDto.projectStatusId;
            if (projectDto.Id > 0)
                _context.ProjectStatus.Update(project);
            else
                _context.ProjectStatus.Add(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Update project failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(project);
        }
        [HttpGet]
        [Route("GetProjectDropDownIWRS")]
        public IActionResult GetProjectDropDownIWRS()
        {
            return Ok(_projectRepository.GetProjectDropDownIWRS());
        }

        [HttpGet]
        [Route("GetProjectDropDownIWRSUnblind")]
        public IActionResult GetProjectDropDownIWRSUnblind()
        {
            return Ok(_projectRepository.GetProjectDropDownIWRSUnblind());
        }

        [HttpGet]
        [Route("GetChildProjectDropDownIWRS/{parentProjectId}")]
        public IActionResult GetChildProjectDropDownIWRS(int parentProjectId)
        {
            return Ok(_projectRepository.GetChildProjectDropDownIWRS(parentProjectId));
        }

        [HttpGet]
        [Route("GetLiveProjectDropDownIWRS")]
        public IActionResult GetLiveProjectDropDownIWRS()
        {
            return Ok(_projectRepository.GetLiveProjectDropDownIWRS());
        }

        [HttpPost("UpdateSiteStatus")]
        public IActionResult UpdateSiteStatus([FromBody] SiteStatusDto projectDto)
        {
            var project = _projectRepository.Find(projectDto.Id);
            if (project == null)
            {
                ModelState.AddModelError("Message", "Site not found");
                return BadRequest(ModelState);
            }

            project.Status = projectDto.Status;
            _context.Project.Update(project);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Update project failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(project);
        }

        [HttpGet]
        [Route("CheckSiteStatus/{projectId}")]
        public IActionResult CheckSiteStatus(int projectId)
        {
            return Ok(_projectRepository.CheckSiteStatus(projectId));
        }

        [HttpGet]
        [Route("GetSitesByTemplateId/{templateId}")]
        public IActionResult GetSitesByTemplateId(int templateId)
        {
            var sites = _projectRepository.GetSitesByTemplateId(templateId);
            return Ok(sites);
        }
    }
}