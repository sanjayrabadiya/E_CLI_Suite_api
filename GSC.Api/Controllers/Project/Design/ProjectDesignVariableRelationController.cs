using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignVariableRelationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignVariableRelationRepository _projectDesignVariableRelationRepository;
        private readonly IUnitOfWork _uow;

        public ProjectDesignVariableRelationController(IProjectDesignVariableRelationRepository projectDesignVariableRelationRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _projectDesignVariableRelationRepository = projectDesignVariableRelationRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        //added by vipul for get variable relation by variable id
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            return Ok(_projectDesignVariableRelationRepository.GetProjectDesignVariableRelationById(id));
        }

        //added by vipul for add variable relation
        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignVariableRelationDto projectDesignVariableRelationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectDesignVariableRelationDto.Id = 0;
            var ProjectDesignVariableRelation = _mapper.Map<ProjectDesignVariableRelation>(projectDesignVariableRelationDto);
            _projectDesignVariableRelationRepository.Add(ProjectDesignVariableRelation);
            _uow.Save();
            return Ok(ProjectDesignVariableRelation.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignVariableRelationRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectDesignVariableRelationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
    }
}
