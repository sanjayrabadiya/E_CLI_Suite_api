using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Audit;
using GSC.Respository.Audit;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Audit
{
    [Route("api/[controller]")]
    public class AuditTrailCommonController : BaseController
    {
        private readonly IAuditTrailCommonRepository _auditTrailCommonRepository;

        public AuditTrailCommonController(IAuditTrailCommonRepository auditTrailCommonRepository)
        {
            _auditTrailCommonRepository = auditTrailCommonRepository;
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] AuditTrailCommonDto search)
        {
            var auditTrailDtos = _auditTrailCommonRepository.Search(search);
            return Ok(auditTrailDtos);
        }
    }
}