using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Authorization;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ProjectAccessController : BaseController
    {
        private readonly IProjectRightRepository _ProjectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ProjectAccessController(IProjectRightRepository projectRightRepository,
           
            IJwtTokenAccesser jwtTokenAccesser,
            IUnitOfWork uow, IMapper mapper)
        {
            _ProjectRightRepository = projectRightRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetProjectAccessReport")]
        public IActionResult GetProjectAccessReport([FromQuery]ProjectTrainigAccessSearchDto filters)
        {
            if (filters.ProjectId <= 0)
            {
                return BadRequest();
            }
            var auditsDto = _ProjectRightRepository.GetProjectAccessReportList(filters);
            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("ProjectRoleDetails/{projectId}")]
        public IActionResult ProjectRoleDetails(int ProjectId)
        {
            if (ProjectId <= 0)
            {
                return BadRequest();
            }
            return Ok(_ProjectRightRepository.GetRoles(ProjectId));
        }

        [HttpGet]
        [Route("ProjectUserDetails/{projectId}")]
        public IActionResult ProjectUserDetails(int ProjectId)
        {
            if (ProjectId <= 0)
            {
                return BadRequest();
            }
            return Ok(_ProjectRightRepository.GetUsers(ProjectId));
        }

    }
}
