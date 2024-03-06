
using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.ProjectRight;


namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ProjectTrainingController : BaseController
    {
        private readonly IProjectRightRepository _ProjectRightRepository;
        
        public ProjectTrainingController(IProjectRightRepository projectRightRepository)
        {
            _ProjectRightRepository = projectRightRepository;
        }

        [HttpGet]
        [Route("GetProjectTrainingReport")]
        public IActionResult GetProjectTrainingReport([FromQuery]ProjectTrainigAccessSearchDto filters)
        {
            if (filters.ProjectId <= 0)
            {
                return BadRequest();
            }

             var auditsDto = _ProjectRightRepository.GetProjectTrainingReportList(filters);
         
            return Ok(auditsDto);
        }
    }
}
