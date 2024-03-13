using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.ProjectRight;


namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ProjectAccessController : BaseController
    {
        private readonly IProjectRightRepository _ProjectRightRepository;
        
        public ProjectAccessController(IProjectRightRepository projectRightRepository)
        {
            _ProjectRightRepository = projectRightRepository;          
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
