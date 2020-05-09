using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IUserRecentItemRepository _userRecentItemRepository;

        public ProjectDesignController(IProjectDesignRepository projectDesignRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IUserRecentItemRepository userRecentItemRepository, IProjectRightRepository projectRightRepository)
        {
            _projectDesignRepository = projectDesignRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRecentItemRepository = userRecentItemRepository;
            _projectRightRepository = projectRightRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IList<ProjectDesignDto> Get(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectDesignDto>();

            var projectDesigns = _projectDesignRepository.FindByInclude(x => x.IsDeleted == isDeleted
                    && projectList.Any(c => c == x.ProjectId), x => x.Project)
                .Select(x => new ProjectDesignDto
                {
                    Id = x.Id,
                    Period = x.Period,
                    ProjectId = x.ProjectId,
                    ProjectName = x.Project.ProjectName,
                    IsStatic = x.Project.IsStatic ? "Yes" : "No",
                    ProjectNumber = x.Project.ProjectCode,
                    IsDeleted = x.IsDeleted,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy,
                    DeletedBy = x.DeletedBy,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    DeletedDate = x.DeletedDate,
                }).OrderByDescending(x => x.Id).ToList();
            foreach (var b in projectDesigns)
            {
                b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            }

            return projectDesigns;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectDesign = _projectDesignRepository.FindByInclude(x => x.Id == id, x => x.Project)
                .FirstOrDefault();
            var projectDesignDto = _mapper.Map<ProjectDesignDto>(projectDesign);
            //projectDesignDto.Locked = !projectDesignDto.IsUnderTesting && _projectDesignRepository.IsScreeningStarted(projectDesignDto.Id);
            projectDesignDto.Locked = !projectDesignDto.IsUnderTesting;
            _userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            {
                KeyId = projectDesign.Id,
                SubjectName = projectDesign.Project.ProjectCode,
                SubjectName1 = projectDesign.Project.ProjectName,
                ScreenType = UserRecent.ProjectDesign
            });

            return Ok(projectDesignDto);
        }

        [HttpGet("CheckProjectDesign/{projectId}")]
        public IActionResult CheckProjectDesign(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var projectDesign = _projectDesignRepository.FindBy(t => t.ProjectId == projectId && t.DeletedDate == null)
                .FirstOrDefault();

            if (projectDesign == null)
                return Ok(0);
            return Ok(projectDesign.Id);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignDto projectDesignDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectDesignDto.IsActiveVersion = true;
            projectDesignDto.IsUnderTesting = true;
            var projectDesign = _mapper.Map<ProjectDesign>(projectDesignDto);
            _projectDesignRepository.Add(projectDesign);
            if (_uow.Save() <= 0) throw new Exception("Creating Project Design failed on save.");


            return Ok(projectDesign.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignDto projectDesignDto)
        {
            if (projectDesignDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesign = _mapper.Map<ProjectDesign>(projectDesignDto);

            _projectDesignRepository.Update(projectDesign);

            if (_uow.Save() <= 0) throw new Exception("Updating Project Design failed on save.");
            return Ok(projectDesign.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectDesignRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _projectDesignRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _projectDesignRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectDesignRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProjectDesignDetail/{projectId}")]
        public async Task<IActionResult> GetProjectDesignDetail(int projectId)
        {
            var projectDesignWithPeriod = await _projectDesignRepository.GetProjectDesignDetail(projectId);
            return Ok(projectDesignWithPeriod);
        }

        [HttpGet]
        [Route("GetProjectByDesignDropDown")]
        public IActionResult GetProjectByDesignDropDown()
        {
            return Ok(_projectDesignRepository.GetProjectByDesignDropDown());
        }

        [HttpGet("CheckCompleteDesign/{id}")]
        public IActionResult CheckCompleteDesign(int id)
        {
            if (id <= 0) return BadRequest();
            var validateMessage = _projectDesignRepository.CheckCompleteDesign(id);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [HttpPut("UpdateCompleteDesign/{id}")]
        public IActionResult UpdateCompleteDesign(int id)
        {
            var record = _projectDesignRepository.Find(id);

            if (record == null)
                return NotFound();
            record.IsCompleteDesign = true;
            _projectDesignRepository.Update(record);
            _uow.Save();

            return Ok();
        }

        [HttpPut("UpdateUnderTesting/{id}/{IsUnderTesting}")]
        public IActionResult UpdateUnderTesting(int id, bool isUnderTesting)
        {
            var record = _projectDesignRepository.Find(id);

            if (record == null)
                return NotFound();
            record.IsUnderTesting = isUnderTesting;
            _projectDesignRepository.Update(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetIsCompleteDesign/{projectId}")]
        public IActionResult GetIsCompleteDesign(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var projectDesign = _projectDesignRepository.FindBy(t => t.ProjectId == projectId && t.DeletedDate == null)
                .FirstOrDefault();

            return Ok(projectDesign);
        }


        [HttpGet("GetProjectIdfromProjectDesign/{projectDesignId}")]
        public IActionResult GetProjectIdfromProjectDesign(int projectDesignId)
        {
            if (projectDesignId <= 0) return BadRequest();

            var projectDesign = _projectDesignRepository.FindByInclude(x => x.Id == projectDesignId, x => x.Project)
                .FirstOrDefault();
            return Ok(projectDesign);
        }
    }
}