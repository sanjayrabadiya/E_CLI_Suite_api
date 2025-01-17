﻿using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Workflow;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Workflow
{
    [Route("api/[controller]")]
    public class ProjectWorkflowController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectWorkflowIndependentRepository _projectWorkflowIndependentRepository;
        private readonly IProjectWorkflowLevelRepository _projectWorkflowLevelRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IStudyVersionRepository _studyVersionRepository;

        public ProjectWorkflowController(IProjectWorkflowRepository projectWorkflowRepository,
            IProjectWorkflowIndependentRepository projectWorkflowIndependentRepository,
            IProjectWorkflowLevelRepository projectWorkflowLevelRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IStudyVersionRepository studyVersionRepository)
        {
            _projectWorkflowRepository = projectWorkflowRepository;
            _projectWorkflowIndependentRepository = projectWorkflowIndependentRepository;
            _projectWorkflowLevelRepository = projectWorkflowLevelRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _studyVersionRepository = studyVersionRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectWorkflow = _projectWorkflowRepository.FindByInclude(t => t.Id == id,
                t => t.Levels, t => t.Independents).FirstOrDefault();

            if (projectWorkflow != null && projectWorkflow.Independents != null)
                projectWorkflow.Independents = projectWorkflow.Independents.Where(x => x.DeletedDate == null).ToList();

            if (projectWorkflow != null && projectWorkflow.Levels != null)
                projectWorkflow.Levels = projectWorkflow.Levels.Where(x => x.DeletedDate == null).ToList();

            var projectWorkflowDto = _mapper.Map<ProjectWorkflowDto>(projectWorkflow);

            if (projectWorkflow != null) projectWorkflowDto.IsLock = !_studyVersionRepository.IsOnTrialByProjectDesing(projectWorkflowDto.ProjectDesignId);

            return Ok(projectWorkflowDto);
        }

        [HttpGet("CheckProjectWorkflow/{projectDesignId}")]
        public IActionResult CheckProjectWorkflow(int projectDesignId)
        {
            if (projectDesignId <= 0) return BadRequest();
            var projectWorkflow = _projectWorkflowRepository
                .FindBy(t => t.ProjectDesignId == projectDesignId && t.DeletedDate == null).FirstOrDefault();

            if (projectWorkflow == null)
                return Ok(0);
            return Ok(projectWorkflow.Id);
        }

        [HttpGet("checkElectronicsSignature/{projectDesignId}")]
        public IActionResult checkElectronicsSignature(int projectDesignId)
        {
            if (projectDesignId <= 0) return BadRequest();
            var projectDesign = _projectWorkflowRepository.IsElectronicsSignatureComplete(projectDesignId);
            return Ok(projectDesign);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProjectWorkflowDto projectWorkflowDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!projectWorkflowDto.IsIndependent)
                projectWorkflowDto.Independents = null;

            var projectWorkflow = _mapper.Map<ProjectWorkflow>(projectWorkflowDto);
            _projectWorkflowRepository.Add(projectWorkflow);
            foreach (var item in projectWorkflow.Independents)
            {
                _projectWorkflowIndependentRepository.Add(item);
            }
            foreach (var item in projectWorkflow.Levels)
            {
                _projectWorkflowLevelRepository.Add(item);
            }
            _uow.Save();
            return Ok(projectWorkflow.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectWorkflowDto projectWorkflowDto)
        {
            if (projectWorkflowDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!projectWorkflowDto.IsIndependent)
                projectWorkflowDto.Independents = null;

            var projectWorkflow = _mapper.Map<ProjectWorkflow>(projectWorkflowDto);
            UpdateIndependents(projectWorkflow);
            UpdateLevels(projectWorkflow);
            _projectWorkflowRepository.Update(projectWorkflow);

            _uow.Save();
            return Ok(projectWorkflow.Id);
        }

        private void UpdateIndependents(ProjectWorkflow projectWorkflow)
        {
            var data = _projectWorkflowIndependentRepository.FindBy(x =>
                x.ProjectWorkflowId == projectWorkflow.Id).ToList();
            var deleteIndependents = data.Where(t => projectWorkflow.Independents.Where(a => a.Id == t.Id).ToList().Count <= 0).ToList();
            var addIndependents = projectWorkflow.Independents.Where(x => x.Id == 0).ToList();
            var updateValue = projectWorkflow.Independents.Where(x => x.Id != 0).ToList();
            foreach (var item in updateValue)
            {
                _projectWorkflowIndependentRepository.Update(item);
            }
            foreach (var item in deleteIndependents)
            {
                item.DeletedBy = _jwtTokenAccesser.UserId;
                item.DeletedDate = _jwtTokenAccesser.GetClientDate();
                _projectWorkflowIndependentRepository.Update(item);
            }
            foreach (var item in addIndependents)
            {
                _projectWorkflowIndependentRepository.Add(item);
            }
        }

        private void UpdateLevels(ProjectWorkflow projectWorkflow)
        {
            var data = _projectWorkflowLevelRepository.FindBy(x => x.ProjectWorkflowId == projectWorkflow.Id).ToList();
            var deleteLevels = data.Where(t => projectWorkflow.Levels.Where(a => a.Id == t.Id).ToList().Count <= 0).ToList();
            var addLevels = projectWorkflow.Levels.Where(x => x.Id == 0).ToList();
            var updateValue = projectWorkflow.Levels.Where(x => x.Id != 0).ToList();
            foreach (var item in updateValue)
            {
                _projectWorkflowLevelRepository.Update(item);
            }
            foreach (var level in deleteLevels)
            {
                level.DeletedBy = _jwtTokenAccesser.UserId;
                level.DeletedDate = _jwtTokenAccesser.GetClientDate();
                _projectWorkflowLevelRepository.Update(level);
            }
            foreach (var level in addLevels)
            {
                _projectWorkflowLevelRepository.Add(level);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectWorkflowRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesign)
                .FirstOrDefault();

            if (record == null)
                return NotFound();

            if (!_studyVersionRepository.IsOnTrialByProjectDesing(record.ProjectDesign.Id))
            {
                ModelState.AddModelError("Message", "Can not delete worklow!");
                return BadRequest(ModelState);
            }

            _projectWorkflowRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _projectWorkflowRepository.Find(id);

            if (record == null)
                return NotFound();
            _projectWorkflowRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}