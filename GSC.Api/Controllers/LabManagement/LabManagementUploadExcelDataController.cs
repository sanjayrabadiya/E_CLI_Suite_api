using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.LabManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LabManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabManagementUploadExcelDataController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILabManagementUploadExcelDataRepository _labManagementUploadExcelDataRepository;

        public LabManagementUploadExcelDataController(
            IUnitOfWork uow, IMapper mapper,
            
        ILabManagementUploadExcelDataRepository labManagementUploadExcelDataRepository,
        IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _labManagementUploadExcelDataRepository = labManagementUploadExcelDataRepository;
        }

        // GET: api/<controller>
        [HttpGet("{labManagementUploadDataId}")]
        public IActionResult Get(int labManagementUploadDataId)
        {
            return Ok(_labManagementUploadExcelDataRepository.GetExcelDataList(labManagementUploadDataId));
        }


    }
}
