using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ProjectDatabaseController : BaseController
    {
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;

        public ProjectDatabaseController(IScreeningTemplateValueRepository screeningTemplateValueRepository)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
        }

        [HttpPost]
        [Route("GetProjectDatabaseEntries")]
        public IActionResult GetProjectDatabaseEntries([FromBody] ProjectDatabaseSearchDto filters)
        {
            if (filters.ProjectId.Length <= 0) return BadRequest();

            _screeningTemplateValueRepository.GetProjectDatabaseEntries(filters);
            return Ok();
        }
    }
}