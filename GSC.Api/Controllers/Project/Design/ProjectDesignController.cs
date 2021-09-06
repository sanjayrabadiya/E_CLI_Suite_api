using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Common;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUserRecentItemRepository _userRecentItemRepository;
        private readonly IStudyVersionRepository _studyVersionRepository;
        private readonly IGSCContext _context;

        public ProjectDesignController(IProjectDesignRepository projectDesignRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
            IStudyVersionRepository studyVersionRepository,
            IUserRecentItemRepository userRecentItemRepository)
        {
            _projectDesignRepository = projectDesignRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _studyVersionRepository = studyVersionRepository;
            _userRecentItemRepository = userRecentItemRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectDesign = _projectDesignRepository.FindByInclude(x => x.Id == id, x => x.Project)
                .FirstOrDefault();
            var projectDesignDto = _mapper.Map<ProjectDesignDto>(projectDesign);
            projectDesignDto.Locked = !_studyVersionRepository.IsOnTrialByProjectDesing(id);
            projectDesignDto.LiveVersion = _studyVersionRepository.All.Where(x => x.ProjectDesignId == id && x.DeletedDate == null && x.VersionStatus == VersionStatus.GoLive).Select(t => t.VersionNumber.ToString()).FirstOrDefault();
            projectDesignDto.AnyLive = _studyVersionRepository.All.Any(x => x.ProjectDesignId == id && x.DeletedDate == null && x.VersionStatus == VersionStatus.GoLive);
            projectDesignDto.TrialVersion = _studyVersionRepository.All.Where(x => x.ProjectDesignId == id && x.DeletedDate == null && x.VersionStatus == VersionStatus.OnTrial).Select(t => t.VersionNumber.ToString()).FirstOrDefault();
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
            var projectDesign = _mapper.Map<ProjectDesign>(projectDesignDto);

            var studyVersion = new StudyVersion();
            studyVersion.VersionNumber = 1;
            studyVersion.VersionStatus = VersionStatus.OnTrial;
            studyVersion.ProjectDesign = projectDesign;
            studyVersion.ProjectId = projectDesign.ProjectId;
            studyVersion.IsMinor = false;
            _studyVersionRepository.Add(studyVersion);
            _projectDesignRepository.Add(projectDesign);

            _uow.Save();
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

        [HttpGet]
        [Route("GetProjectByDesignDropDown")]
        public IActionResult GetProjectByDesignDropDown()
        {
            return Ok(_projectDesignRepository.GetProjectByDesignDropDown());
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


        [HttpPut("updateElectricSignature/{projectDesignId}/{moduleName}/{isComplete}")]
        public IActionResult updateElectricSignature(int projectDesignId, string moduleName, bool isComplete)
        {
            var record = _projectDesignRepository.IsCompleteExist(projectDesignId, moduleName, isComplete);
            return Ok();
        }

        [HttpGet("IsWorkFlowOrEditCheck/{projectDesignId}")]
        public IActionResult IsWorkFlowOrEditCheck(int projectDesignId)
        {
            var result = _projectDesignRepository.IsWorkFlowOrEditCheck(projectDesignId);
            return Ok(result);
        }


        [HttpGet("CheckPeriodWithProjectPeriod/{projectDesignId}/{projectId}")]
        public IActionResult CheckPeriodWithProjectPeriod(int projectDesignId, int projectId)
        {
            var result = _projectDesignRepository.CheckPeriodWithProjectPeriod(projectDesignId, projectId);
            return Ok(result);
        }
    }
}