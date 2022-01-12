using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Report;
using GSC.Respository.CTMS;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ManageMonitoringReportVariableAuditController : BaseController
    {
        private readonly IManageMonitoringReportVariableAuditRepository _manageMonitoringReportVariableAuditRepository;

        public ManageMonitoringReportVariableAuditController(
            IManageMonitoringReportVariableAuditRepository manageMonitoringReportVariableAuditRepository)
        {
            _manageMonitoringReportVariableAuditRepository = manageMonitoringReportVariableAuditRepository;
        }

        /// Get variable audit by manageMonitoringReportVariableId
        /// Created By Swati
        [HttpGet("{manageMonitoringReportVariableId}")]
        public IActionResult Get(int manageMonitoringReportVariableId)
        {
            if (manageMonitoringReportVariableId <= 0) return BadRequest();

            var auditsDto = _manageMonitoringReportVariableAuditRepository.GetAudits(manageMonitoringReportVariableId);

            return Ok(auditsDto);
        }
    }
}