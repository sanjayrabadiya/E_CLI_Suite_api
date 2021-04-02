using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
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
        private readonly IDrugRepository _drugRepository;
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;
        private readonly ITrialTypeRepository _trialTypeRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUserRecentItemRepository _userRecentItemRepository;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly ICentreUserService _centreUserService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IRandomizationRepository _randomizationRepository;

        public ProjectController(IProjectRepository projectRepository,
            IDesignTrialRepository designTrialRepository,
            ITrialTypeRepository trialTypeRepository,
            IDrugRepository drugRepository,
            IProjectDesignRepository projectDesignRepository,
            IUnitOfWork uow, IMapper mapper,
            IUserRecentItemRepository userRecentItemRepository,
            INumberFormatRepository numberFormatRepository,
            ICentreUserService centreUserService,
            IJwtTokenAccesser jwtTokenAccesser,
            IOptions<EnvironmentSetting> environmentSetting,
            IRandomizationRepository randomizationRepository)
        {
            _projectRepository = projectRepository;
            _designTrialRepository = designTrialRepository;
            _trialTypeRepository = trialTypeRepository;
            _drugRepository = drugRepository;
            _uow = uow;
            _mapper = mapper;
            _userRecentItemRepository = userRecentItemRepository;
            _numberFormatRepository = numberFormatRepository;
            _projectDesignRepository = projectDesignRepository;
            _centreUserService = centreUserService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _environmentSetting = environmentSetting;
            _randomizationRepository = randomizationRepository;
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
                SubjectName = project.ProjectCode,
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
                if (_projectRepository.All.Where(x => x.ManageSiteId == projectDto.ManageSiteId && x.ParentProjectId == projectDto.ParentProjectId && x.DeletedDate == null && x.IsTestSite == false).ToList().Count > 0)
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
            if (_uow.Save() <= 0) throw new Exception("Creating Project failed on save.");

            _userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            {
                KeyId = project.Id,
                SubjectName = project.ProjectCode,
                SubjectName1 = project.ProjectName,
                ScreenType = UserRecent.Project
            });


            return Ok(project);
        }

        [HttpPut("UpdateRandomizationNumberFormat")]
        public IActionResult UpdateRandomizationNumberFormat([FromBody] RandomizationAndScreeningNumberFormatDto projectDto)
        {
            if (projectDto.Id <= 0) return BadRequest();

            var sites = _projectRepository.All.Where(x => x.ParentProjectId == projectDto.Id).Select(y => y.Id).ToList();
            var randomizations = _randomizationRepository.All.Where(x => sites.Contains(x.ProjectId) && x.RandomizationNumber != null && x.RandomizationNumber != "").ToList();

            if (randomizations != null && randomizations.Count > 0)
            {
                ModelState.AddModelError("Message", "You can't change format, Randomization entry is started in subject management");
                return BadRequest(ModelState);
            }

            if (projectDto.IsManualRandomNo == false)
            {
                if (projectDto.RandomNoStartsWith == null)
                {
                    ModelState.AddModelError("Message", "Please add valid Starts with number");
                    return BadRequest(ModelState);
                }
            }

            var project = _projectRepository.Find(projectDto.Id);
            project.RandomNoLength = projectDto.RandomNoLength;
            project.IsManualRandomNo = projectDto.IsManualRandomNo;
            project.IsAlphaNumRandomNo = projectDto.IsAlphaNumRandomNo;
            project.RandomNoStartsWith = projectDto.RandomNoStartsWith;
            project.IsSiteDependentRandomNo = projectDto.IsSiteDependentRandomNo;
            //project.ScreeningLength = projectDto.ScreeningLength;
            //project.IsManualScreeningNo = projectDto.IsManualScreeningNo;
            //project.IsAlphaNumScreeningNo = projectDto.IsAlphaNumScreeningNo;
            //project.ScreeningNoStartsWith = projectDto.ScreeningNoStartsWith;
            //project.IsSiteDependentScreeningNo = projectDto.IsSiteDependentScreeningNo;

            if (project.IsManualRandomNo == false)
            {
                if (project.IsSiteDependentRandomNo == true)
                {
                    var projects = _projectRepository.FindBy(x => x.ParentProjectId == project.Id).ToList();
                    for (int i = 0; i < projects.Count; i++)
                    {
                        projects[i].RandomizationNoseries = (int)project.RandomNoStartsWith;
                        _projectRepository.Update(projects[i]);
                    }
                }
                else
                {
                    project.RandomizationNoseries = (int)project.RandomNoStartsWith;
                }
            }
            _projectRepository.Update(project);
            if (_uow.Save() <= 0) throw new Exception("Updating Project failed on save.");
            return Ok(project.Id);
        }

        [HttpPut("UpdateScreeningNumberFormat")]
        public IActionResult UpdateScreeningNumberFormat([FromBody] RandomizationAndScreeningNumberFormatDto projectDto)
        {
            if (projectDto.Id <= 0) return BadRequest();

            var sites = _projectRepository.All.Where(x => x.ParentProjectId == projectDto.Id).Select(y => y.Id).ToList();
            var randomizations = _randomizationRepository.All.Where(x => sites.Contains(x.ProjectId) && x.ScreeningNumber != null && x.ScreeningNumber != "").ToList();

            if (randomizations != null && randomizations.Count > 0)
            {
                ModelState.AddModelError("Message", "You can't change format, Screening entry is started in subject management");
                return BadRequest(ModelState);
            }

            if (projectDto.IsManualScreeningNo == false)
            {
                if (projectDto.ScreeningNoStartsWith == null)
                {
                    ModelState.AddModelError("Message", "Please add valid Starts with number");
                    return BadRequest(ModelState);
                }
            }

            var project = _projectRepository.Find(projectDto.Id);
            //project.RandomNoLength = projectDto.RandomNoLength;
            //project.IsManualRandomNo = projectDto.IsManualRandomNo;
            //project.IsAlphaNumRandomNo = projectDto.IsAlphaNumRandomNo;
            //project.RandomNoStartsWith = projectDto.RandomNoStartsWith;
            //project.IsSiteDependentRandomNo = projectDto.IsSiteDependentRandomNo;
            project.ScreeningLength = projectDto.ScreeningLength;
            project.IsManualScreeningNo = projectDto.IsManualScreeningNo;
            project.IsAlphaNumScreeningNo = projectDto.IsAlphaNumScreeningNo;
            project.ScreeningNoStartsWith = projectDto.ScreeningNoStartsWith;
            project.IsSiteDependentScreeningNo = projectDto.IsSiteDependentScreeningNo;

            if (project.IsManualScreeningNo == false)
            {
                if (project.IsSiteDependentScreeningNo == true)
                {
                    var projects = _projectRepository.FindBy(x => x.ParentProjectId == project.Id).ToList();
                    for (int i = 0; i < projects.Count; i++)
                    {
                        projects[i].ScreeningNoseries = (int)project.ScreeningNoStartsWith;
                        _projectRepository.Update(projects[i]);
                    }
                }
                else
                {
                    project.ScreeningNoseries = (int)project.ScreeningNoStartsWith;
                }
            }
            _projectRepository.Update(project);
            if (_uow.Save() <= 0) throw new Exception("Updating Project failed on save.");
            return Ok(project.Id);
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
            project.ScreeningNoseries = details.ScreeningNoseries;
            project.RandomizationNoseries = details.RandomizationNoseries;
            project.ScreeningNoStartsWith = details.ScreeningNoStartsWith;
            project.RandomNoStartsWith = details.RandomNoStartsWith;
            project.ScreeningLength = details.ScreeningLength;
            project.RandomNoLength = details.RandomNoLength;
            project.IsManualScreeningNo = details.IsManualScreeningNo;
            project.IsManualRandomNo = details.IsManualRandomNo;
            project.IsSiteDependentScreeningNo = details.IsSiteDependentScreeningNo;
            project.IsSiteDependentRandomNo = details.IsSiteDependentRandomNo;
            project.IsAlphaNumScreeningNo = details.IsAlphaNumScreeningNo;
            project.IsAlphaNumRandomNo = details.IsAlphaNumRandomNo;
            project.IsSendEmail = details.IsSendEmail;
            project.IsSendSMS = details.IsSendSMS;

            if (projectDto.ParentProjectId > 0 && !projectDto.IsTestSite)
            {
                if (_projectRepository.All.Where(x => x.ManageSiteId == projectDto.ManageSiteId && x.ParentProjectId == projectDto.ParentProjectId && x.Id != projectDto.Id && x.DeletedDate == null && x.IsTestSite == false).ToList().Count > 0)
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
            if (_uow.Save() <= 0) throw new Exception("Updating Project failed on save.");
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
            if (_uow.Save() <= 0) throw new Exception("Updating SMS/Email configuration failed on save.");
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
            bool IsAddmoreStudy = true;
            int noofstudy = 0;
            if (!_environmentSetting.Value.IsPremise)
            {
                //noofstudy = await _centreUserService.Getnoofstudy($"{_environmentSetting.Value.CentralApi}Company/Getnoofstudy/{_jwtTokenAccesser.CompanyId}");
                //int studycount = _projectRepository.FindBy(x => x.ParentProjectId == null && x.DeletedDate == null).Count();
                //if (studycount < noofstudy)
                    IsAddmoreStudy = true;
            }
            return Ok(IsAddmoreStudy);
        }

        [HttpGet]
        [Route("GetStudyCount/{CompanyID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudyCount(int CompanyID)
        {
            await _centreUserService.SentConnectionString(CompanyID, $"{_environmentSetting.Value.CentralApi}Company/GetConnectionDetails/{CompanyID}");
            int studycount = _projectRepository.FindBy(x => x.ParentProjectId == null && x.DeletedDate == null).Count();
            return Ok(studycount);
        }

    }
}