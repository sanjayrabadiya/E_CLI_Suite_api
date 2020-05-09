using GSC.Api.Controllers.Common;
using GSC.Respository.Pharmacy;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Pharmacy
{
    [Route("api/[controller]")]
    public class PharmacyTemplateValueAuditController : BaseController
    {
        private readonly IPharmacyTemplateValueAuditRepository _pharmacyTemplateValueAuditRepository;

        public PharmacyTemplateValueAuditController(
            IPharmacyTemplateValueAuditRepository pharmacyTemplateValueAuditRepository)
        {
            _pharmacyTemplateValueAuditRepository = pharmacyTemplateValueAuditRepository;
        }

        [HttpGet("{pharmacyTemplateValueId}")]
        public IActionResult Get(int pharmacyTemplateValueId)
        {
            if (pharmacyTemplateValueId <= 0) return BadRequest();

            var auditsDto = _pharmacyTemplateValueAuditRepository.GetAudits(pharmacyTemplateValueId);

            return Ok(auditsDto);
        }
    }
}