using GSC.Api.Controllers.Common;
using GSC.Respository.SupplyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationApprovalTemplateValueAuditController : BaseController
    {
        private readonly IVerificationApprovalTemplateValueAuditRepository _verificationApprovalTemplateValueAuditRepository;

        public VerificationApprovalTemplateValueAuditController(
            IVerificationApprovalTemplateValueAuditRepository verificationApprovalTemplateValueAuditRepository)
        {
            _verificationApprovalTemplateValueAuditRepository = verificationApprovalTemplateValueAuditRepository;
        }

        [HttpGet("{verificationTemplateValueId}")]
        public IActionResult Get(int verificationTemplateValueId)
        {
            if (verificationTemplateValueId <= 0) return BadRequest();

            var auditsDto = _verificationApprovalTemplateValueAuditRepository.GetAudits(verificationTemplateValueId);

            return Ok(auditsDto);
        }
    }
}
