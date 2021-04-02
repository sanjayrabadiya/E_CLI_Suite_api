using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Design;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatePermissionController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ITemplatePermissionRepository _templatePermissionRepository;
        public TemplatePermissionController(
            ITemplatePermissionRepository templatePermissionRepository,
        IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _templatePermissionRepository = templatePermissionRepository;
        }

        // Get project design Template permission details
        [HttpGet]
        [Route("GetTemplatePermissionDetails/{projectDesignTemplateId}")]
        public IActionResult GetTemplatePermissionDetails(int projectDesignTemplateId)
        {
            return Ok(_templatePermissionRepository.GetTemplatePermissionDetails(projectDesignTemplateId));
        }

        // Save project design Template permission details
        [HttpPost]
        public IActionResult Post([FromBody] List<TemplatePermission> TemplatePermission)
        {
            if (!ModelState.IsValid || !TemplatePermission.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _templatePermissionRepository.Save(TemplatePermission);

            return Ok();
        }

        // update project design Template permission details
        [HttpPut]
        public IActionResult Put([FromBody] List<TemplatePermission> TemplatePermission)
        {
            if (!ModelState.IsValid || !TemplatePermission.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _templatePermissionRepository.updatePermission(TemplatePermission);

            return Ok();
        }

    }
}
