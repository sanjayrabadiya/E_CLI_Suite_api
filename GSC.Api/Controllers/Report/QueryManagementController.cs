using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class QueryManagementController : BaseController
    {
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;

        // private readonly IQueryManagementRepository _queryManagementRepository;

        public QueryManagementController( //IQueryManagementRepository queryManagementRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository)
        {
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            //_queryManagementRepository = queryManagementRepository;
        }

        [HttpGet]
        [Route("GetQueryEntries")]
        public IActionResult GetQueryEntries([FromQuery] QuerySearchDto filters)
        {
            if (filters.ProjectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateValueQueryRepository.GetQueryEntries(filters);

            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("GetGenerateQueryBy/{projectId}")]
        public IActionResult GetGenerateQueryBy(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateValueQueryRepository.GetGenerateQueryBy(projectId);

            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("GetDataEntryBy/{projectId}")]
        public IActionResult GetDataEntryBy(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateValueQueryRepository.GetDataEntryBy(projectId);

            return Ok(auditsDto);
        }
    }
}