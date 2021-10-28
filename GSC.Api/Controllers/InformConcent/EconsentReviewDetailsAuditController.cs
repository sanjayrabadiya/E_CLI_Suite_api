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

        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentReviewDetailsAuditRepository _econsentReviewDetailsAuditRepository;

        public EconsentReviewDetailsAuditController(
            IUnitOfWork uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IGSCContext context,
            IEconsentReviewDetailsAuditRepository econsentReviewDetailsAuditRepository
            )
        {
            _uow = uow;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentReviewDetailsAuditRepository = econsentReviewDetailsAuditRepository;
        }

        [HttpPost]
        [Route("GenerateICfDetailReport")]
        [TransactionRequired]
        public async Task<IActionResult> GenerateICfDetailReport([FromBody] EconsentReviewDetailsAuditParameterDto details)
        {

            _econsentReviewDetailsAuditRepository.GenerateICFDetailReport(details);
            return Ok();
        }
    }
}
