using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.Report;
using GSC.Respository.Audit;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Audit
{
    [Route("api/[controller]")]
    public class AuditTrailController : BaseController
    {
        private readonly IAuditTrailRepository _auditTrailRepository;

        public AuditTrailController(IAuditTrailRepository auditTrailRepository)
        {
            _auditTrailRepository = auditTrailRepository;
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] AuditTrailDto search)
        {
            var auditTrailDtos = _auditTrailRepository.Search(search);
            return Ok(auditTrailDtos);
        }

        //Get project design audit Report
        [HttpPost]
        [Route("GetDesignAuditReport")]
        public IActionResult GetDesignAuditReport([FromBody] ProjectDatabaseSearchDto search)
        {
            _auditTrailRepository.SearchProjectDesign(search);
            return Ok();
        }
    }
}