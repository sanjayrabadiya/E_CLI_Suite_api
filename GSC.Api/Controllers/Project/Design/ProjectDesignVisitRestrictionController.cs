using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Design;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectDesignVisitRestrictionController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesignVisitRestrictionRepository _projectDesignVisitRestrictionRepository;
        public ProjectDesignVisitRestrictionController(
            IProjectDesignVisitRestrictionRepository projectDesignVisitRestrictionRepository,
        IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            _projectDesignVisitRestrictionRepository = projectDesignVisitRestrictionRepository;
        }

        // Get project design Visit permission details
        [HttpGet]
        [Route("GetProjectDesignVisitRestrictionDetails/{projectDesignVisitId}")]
        public IActionResult GetProjectDesignVisitRestrictionDetails(int projectDesignVisitId)
        {
            return Ok(_projectDesignVisitRestrictionRepository.GetProjectDesignVisitRestrictionDetails(projectDesignVisitId));
        }

        // Save project design Visit permission details
        [HttpPost]
        public IActionResult Post([FromBody] List<ProjectDesignVisitRestriction> ProjectDesignVisitRestriction)
        {
            if (!ModelState.IsValid || !ProjectDesignVisitRestriction.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _projectDesignVisitRestrictionRepository.Save(ProjectDesignVisitRestriction);

            return Ok();
        }

        // update project design Visit permission details
        [HttpPut]
        public IActionResult Put([FromBody] List<ProjectDesignVisitRestriction> ProjectDesignVisitRestriction)
        {
            if (!ModelState.IsValid || !ProjectDesignVisitRestriction.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _projectDesignVisitRestrictionRepository.updatePermission(ProjectDesignVisitRestriction);

            return Ok();
        }
    }
}
