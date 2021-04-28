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
    public class ProjectDesingTemplateRestrictionController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesingTemplateRestrictionRepository _projectDesingTemplateRestrictionRepository;
        public ProjectDesingTemplateRestrictionController(
            IProjectDesingTemplateRestrictionRepository projectDesingTemplateRestrictionRepository,
        IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _projectDesingTemplateRestrictionRepository = projectDesingTemplateRestrictionRepository;
        }

        // Get project design Template permission details
        [HttpGet]
        [Route("GetProjectDesingTemplateRestrictionDetails/{projectDesignTemplateId}")]
        public IActionResult GetProjectDesingTemplateRestrictionDetails(int projectDesignTemplateId)
        {
            return Ok(_projectDesingTemplateRestrictionRepository.GetProjectDesingTemplateRestrictionDetails(projectDesignTemplateId));
        }

        // Save project design Template permission details
        [HttpPost]
        public IActionResult Post([FromBody] List<ProjectDesingTemplateRestriction> ProjectDesingTemplateRestriction)
        {
            if (!ModelState.IsValid || !ProjectDesingTemplateRestriction.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _projectDesingTemplateRestrictionRepository.Save(ProjectDesingTemplateRestriction);

            return Ok();
        }

        // update project design Template permission details
        [HttpPut]
        public IActionResult Put([FromBody] List<ProjectDesingTemplateRestriction> ProjectDesingTemplateRestriction)
        {
            if (!ModelState.IsValid || !ProjectDesingTemplateRestriction.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _projectDesingTemplateRestrictionRepository.updatePermission(ProjectDesingTemplateRestriction);

            return Ok();
        }

    }
}
