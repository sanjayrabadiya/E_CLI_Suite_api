using GSC.Api.Controllers.Common;
using GSC.Respository.SupplyManagement;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementUploadFileDetailController : BaseController
    {
      
        private readonly ISupplyManagementUploadFileDetailRepository _supplyManagementUploadFileDetailRepository;
        public SupplyManagementUploadFileDetailController(ISupplyManagementUploadFileDetailRepository supplyManagementUploadFileDetailRepository)
        {
            _supplyManagementUploadFileDetailRepository = supplyManagementUploadFileDetailRepository;
          
        }

        [HttpGet("{supplyManagementUploadFileId}")]
        public IActionResult Get(int supplyManagementUploadFileId)
        {
            return Ok(_supplyManagementUploadFileDetailRepository.GetSupplyManagementUploadFileDetailList(supplyManagementUploadFileId));
        }
    }
}
