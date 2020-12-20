using GSC.Api.Controllers.Common;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateValueAuditController : BaseController
    {
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;

        public ScreeningTemplateValueAuditController(
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository)
        {
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
        }

        [HttpGet("{screeningTemplateValueId}")]
        public IActionResult Get(int screeningTemplateValueId)
        {
            if (screeningTemplateValueId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateValueAuditRepository.GetAudits(screeningTemplateValueId);

            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("GetAuditHistory/{id}")]
        public IActionResult GetAuditHistory(int id)
        {
            var auditHistory = _screeningTemplateValueAuditRepository.GetAuditHistoryByScreeningEntry(id);

            return Ok(auditHistory);
        }
    }
}