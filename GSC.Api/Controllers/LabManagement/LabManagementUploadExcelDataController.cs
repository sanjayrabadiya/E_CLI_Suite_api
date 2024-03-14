using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Respository.LabManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LabManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabManagementUploadExcelDataController : BaseController
    {
        private readonly ILabManagementUploadExcelDataRepository _labManagementUploadExcelDataRepository;

        public LabManagementUploadExcelDataController(
                    ILabManagementUploadExcelDataRepository labManagementUploadExcelDataRepository
       )
        {
            _labManagementUploadExcelDataRepository = labManagementUploadExcelDataRepository;
        }

        // GET: api/<controller>
        [HttpGet("{labManagementUploadDataId}")]
        public IActionResult Get(int labManagementUploadDataId)
        {
            return Ok(_labManagementUploadExcelDataRepository.GetExcelDataList(labManagementUploadDataId));
        }


        [HttpPost]
        [Route("GetDataNotUseInDataEntry")]
        public IActionResult GetDataNotUseInDataEntry([FromBody] LabManagementUploadDataDto search)
        {
            return _labManagementUploadExcelDataRepository.GetDataNotUseInDataEntry(search);
        }

    }
}
