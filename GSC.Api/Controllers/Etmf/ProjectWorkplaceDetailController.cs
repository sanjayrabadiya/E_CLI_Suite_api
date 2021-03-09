using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceDetailController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectWorkplaceDetailRepository _projectWorkplaceDetailRepository;

        public ProjectWorkplaceDetailController(IUnitOfWork uow,
            IMapper mapper,
            IProjectWorkplaceDetailRepository projectWorkplaceDetailRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _projectWorkplaceDetailRepository = projectWorkplaceDetailRepository;
        }


        [HttpGet]
        [Route("GetCountryByWorkplace/{id}")]
        public IActionResult GetCountryByWorkplace(int id)
        {
            return Ok(_projectWorkplaceDetailRepository.GetCountryByWorkplace(id));
        }

        [HttpGet]
        [Route("GetSiteByWorkplace/{id}")]
        public IActionResult GetSiteByWorkplace(int id)
        {
            return Ok(_projectWorkplaceDetailRepository.GetSiteByWorkplace(id));
        }

        [HttpGet("GetByUserId/{UserId}/{ProjectId}")]
        public IActionResult GetByUserId(int UserId, int ProjectId)
        {
            if (UserId <= 0) return BadRequest();

            var validate = _projectWorkplaceDetailRepository
                .FindByInclude(t => t.DeletedDate == null && t.ProjectWorkplace.ProjectId == ProjectId);
            if (validate.Count() == 0)
            {
                ModelState.AddModelError("Message", "Worksplace Not Created.");
                return BadRequest(ModelState);
            }
            var permissionDtos = _projectWorkplaceDetailRepository.GetByUserId(UserId, ProjectId);

            return Ok(permissionDtos);
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<EtmfUserPermission> etmfUserPermission)
        {
            if (!ModelState.IsValid || !etmfUserPermission.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _projectWorkplaceDetailRepository.Save(etmfUserPermission);

            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] List<EtmfUserPermissionDto> etmfUserPermissionDto)
        {
            if (!ModelState.IsValid || !etmfUserPermissionDto.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _projectWorkplaceDetailRepository.updatePermission(etmfUserPermissionDto);
            return Ok();
        }

        [HttpGet]
        [Route("GetEtmfPermissionData/{ProjectId}")]
        public IActionResult GetEtmfPermissionData(int ProjectId)
        {
            return Ok(_projectWorkplaceDetailRepository.GetEtmfPermissionData(ProjectId));
        }

        [HttpPut]
        [Route("RollbackRight/{ProjectId}/{UserIds}")]
        public IActionResult RollbackRight(int ProjectId, int[] UserIds)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectWorkplaceDetailRepository.SaveProjectRollbackRight(ProjectId, UserIds);

            if (_uow.Save() < 0) throw new Exception("Project Revoke rights failed on save.");

            return Ok();
        }

        [HttpGet]
        [Route("EtmfRightHistoryDetails/{projectId}/{userId}")]
        public IActionResult EtmfRightHistoryDetails(int projectId, int userId)
        {
            if (projectId <= 0 || userId <= 0) return BadRequest();
            return Ok(_projectWorkplaceDetailRepository.GetEtmfRightHistoryDetails(projectId, userId));
        }

        [HttpGet]
        [Route("GetSitesForEtmf/{ProjectId}")]
        public IActionResult GetSitesForEtmf(int ProjectId)
        {
            return Ok(_projectWorkplaceDetailRepository.GetSitesForEtmf(ProjectId));
        }
    }
}