﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

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

        public ProjectController(IProjectRepository projectRepository,
            IDesignTrialRepository designTrialRepository,
            ITrialTypeRepository trialTypeRepository,
            IDrugRepository drugRepository,
            IProjectDesignRepository projectDesignRepository,
            IUnitOfWork uow, IMapper mapper,
            IUserRecentItemRepository userRecentItemRepository,
            INumberFormatRepository numberFormatRepository)
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

            projectDto.TrialTypeId = _designTrialRepository.Find(projectDto.DesignTrialId).TrialTypeId;
            var projectDesign =
                _projectDesignRepository.FindByInclude(t => t.ProjectId == id && t.DeletedDate == null).FirstOrDefault();
            projectDto.ProjectDesignId = projectDesign == null ? (int?)null : projectDesign.Id;

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

            if (projectDto.ParentProjectId > 0)
            {
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

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] ProjectDto projectDto)
        {
            if (projectDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var project = _mapper.Map<Data.Entities.Master.Project>(projectDto);
            project.Id = projectDto.Id;

            if (projectDto.ParentProjectId > 0)
            {
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
            var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "pro" && x.DeletedDate == null).FirstOrDefault();
            return Ok(numberFormat);
        }

        [HttpGet]
        [Route("CheckSitesNumberFormat")]
        public IActionResult CheckSitesNumberFormat()
        {
            var numberFormat = _numberFormatRepository.FindBy(x => x.KeyName == "prochild" && x.DeletedDate == null).FirstOrDefault();
            return Ok(numberFormat);
        }

        [HttpGet]
        [Route("GetAutoNumberForSites")]
        public IActionResult GetAutoNumberForSites()
        {
            var autoNumber = _projectRepository.GetAutoNumberForSites();
            ModelState.AddModelError("AutoNumber", autoNumber);
            return Ok(ModelState);
        }

        [HttpGet]
        [Route("GetChildProjectRightsDropDown")]
        public IActionResult GetChildProjectRightsDropDown()
        {
            return Ok(_projectRepository.GetChildProjectRightsDropDown());
        }
    }
}