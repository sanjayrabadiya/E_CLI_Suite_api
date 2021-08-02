using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Audit;
using GSC.Respository.Audit;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Audit
{
    [Route("api/[controller]")]
    public class VolunteerAuditTrailController : BaseController
    {
        private readonly IVolunteerAuditTrailRepository _volunteerAuditTrailRepository;

        public VolunteerAuditTrailController(IVolunteerAuditTrailRepository volunteerAuditTrailRepository)
        {
            _volunteerAuditTrailRepository = volunteerAuditTrailRepository;
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] VolunteerAuditTrailDto search)
        {
            var auditTrailDtos = _volunteerAuditTrailRepository.Search(search);
            return Ok(auditTrailDtos);
        }
    }
}