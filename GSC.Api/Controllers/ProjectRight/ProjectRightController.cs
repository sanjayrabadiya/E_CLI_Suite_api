﻿using System;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.ProjectRight;
using GSC.Respository.ProjectRight;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.ProjectRight
{
    [Route("api/[controller]")]
    public class ProjectRightController : BaseController
    {
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUnitOfWork _uow;

        public ProjectRightController(IProjectRightRepository projectRightRepository,
            IUnitOfWork uow)
        {
            _projectRightRepository = projectRightRepository;
            _uow = uow;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_projectRightRepository.GetProjectRights());
        }


        [HttpGet("{ProjectId}")]
        public IActionResult Get(int projectId)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_projectRightRepository.GetProjectRightByProjectId(projectId));
        }

        [HttpPost]
        [Route("AccessRight")]
        public IActionResult AccessRight([FromBody] ProjectRightSaveDto projectRightSaveDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectRightRepository.SaveProjectAccessRight(projectRightSaveDto.projectRightDto,
                projectRightSaveDto.projectId);

            _uow.Save();

            return Ok(projectRightSaveDto.projectId);
        }

        [HttpPost]
        [Route("RollbackRight")]
        public IActionResult RollbackRight([FromBody] ProjectRightSaveDto projectRightSaveDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectRightRepository.SaveProjectRollbackRight(projectRightSaveDto.projectRightDto,
                projectRightSaveDto.projectId, projectRightSaveDto.Ids);

            if (_uow.Save() < 0) return Ok(new Exception("Project Revoke rights failed on save."));

            return Ok(projectRightSaveDto.projectId);
        }


        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] int id)
        {
            var projectRight = _projectRightRepository.Find(id);
            _projectRightRepository.Delete(projectRight);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("ProjectReviewDetails/{projectId}")]
        public IActionResult ProjectReviewDetails(int projectId)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_projectRightRepository.GetProjectRightDetailsByProjectId(projectId));
        }

        [HttpGet]
        [Route("ProjectReviewHistory/{projectId}/{userId}/{roleId}")]
        public IActionResult ProjectReviewHistory(int projectId, int userId, int roleId)
        {
            if (projectId <= 0 || userId <= 0) return BadRequest();
            return Ok(_projectRightRepository.GetProjectRightHistory(projectId, userId, roleId));
        }

        [HttpGet]
        [Route("EtmfUserDropDown/{projectId}/{userId}")]
        public IActionResult EtmfUserDropDown(int projectId, int? userId)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_projectRightRepository.EtmfUserDropDown(projectId, userId));
        }
    }
}