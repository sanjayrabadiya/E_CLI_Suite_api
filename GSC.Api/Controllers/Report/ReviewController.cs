using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ReviewController : BaseController
    {
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateReviewRepository _screeningTemplateReviewRepository;

        public ReviewController(IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningTemplateReviewRepository screeningTemplateReviewRepository)
        {
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningTemplateReviewRepository = screeningTemplateReviewRepository;
        }

        [HttpGet]
        [Route("GetReviewReport")]
        public IActionResult GetReviewReport([FromQuery] ReviewSearchDto filters)
        {
            if (filters.ProjectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateRepository.GetReviewReportList(filters);

            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("GetReviewLevel/{projectId}")]
        public IActionResult GetReviewLevel(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateReviewRepository.GetReviewLevel(projectId);

            return Ok(auditsDto);
        }
    }
}