using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
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
    public class VerificationApprovalTemplateHistoryController : BaseController
    {
        
        private readonly IVerificationApprovalTemplateHistoryRepository _verificationApprovalTemplateHistoryRepository;
       

        public VerificationApprovalTemplateHistoryController(IVerificationApprovalTemplateHistoryRepository verificationApprovalTemplateHistoryRepository)
        {
            _verificationApprovalTemplateHistoryRepository = verificationApprovalTemplateHistoryRepository;
          
          
        }

        [HttpGet("{ProductVerificationDetailId}")]
        public IActionResult Get(int ProductVerificationDetailId)
        {
            return Ok(_verificationApprovalTemplateHistoryRepository.GetHistoryByVerificationDetail(ProductVerificationDetailId));
        }
    }
}
