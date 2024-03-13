
using GSC.Api.Controllers.Common;
using GSC.Respository.SupplyManagement;
using Microsoft.AspNetCore.Mvc;
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
