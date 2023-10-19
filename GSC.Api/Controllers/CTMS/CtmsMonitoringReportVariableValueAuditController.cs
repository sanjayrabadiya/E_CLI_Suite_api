using GSC.Api.Controllers.Common;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportVariableValueAuditController : BaseController
    {
        private readonly ICtmsMonitoringReportVariableValueAuditRepository _ctmsMonitoringReportVariableValueAuditRepository;

        public CtmsMonitoringReportVariableValueAuditController(
            ICtmsMonitoringReportVariableValueAuditRepository ctmsMonitoringReportVariableValueAuditRepository)
        {
            _ctmsMonitoringReportVariableValueAuditRepository = ctmsMonitoringReportVariableValueAuditRepository;
        }

        /// Get variable audit by CtmsMonitoringReportVariableValueId
        /// Created By Swati
        [HttpGet("{CtmsMonitoringReportVariableValueId}")]
        public IActionResult Get(int CtmsMonitoringReportVariableValueId)
        {
            if (CtmsMonitoringReportVariableValueId <= 0) return BadRequest();

            var auditsDto = _ctmsMonitoringReportVariableValueAuditRepository.GetAudits(CtmsMonitoringReportVariableValueId);

            return Ok(auditsDto);
        }
    }
}