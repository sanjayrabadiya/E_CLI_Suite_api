using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Domain.Context;
using GSC.Respository.InformConcent;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentReviewDetailsAuditController : BaseController
    {
        private readonly IEconsentReviewDetailsAuditRepository _econsentReviewDetailsAuditRepository;

        public EconsentReviewDetailsAuditController(IEconsentReviewDetailsAuditRepository econsentReviewDetailsAuditRepository)
        {
            _econsentReviewDetailsAuditRepository = econsentReviewDetailsAuditRepository;
        }

        [HttpPost]
        [Route("GenerateICfDetailReport")]
        [TransactionRequired]
        public IActionResult GenerateICfDetailReport([FromBody] EconsentReviewDetailsAuditParameterDto details)
        {
            _econsentReviewDetailsAuditRepository.GenerateICFDetailReport(details);
            return Ok();
        }
    }
}
