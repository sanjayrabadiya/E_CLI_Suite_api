using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementUploadFileDetailController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementUploadFileDetailRepository _supplyManagementUploadFileDetailRepository;
        private readonly IUnitOfWork _uow;

        public SupplyManagementUploadFileDetailController(ISupplyManagementUploadFileDetailRepository supplyManagementUploadFileDetailRepository,
            IUnitOfWork uow, IMapper mapper,
            IUploadSettingRepository uploadSettingRepository,
            IProjectRepository projectRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _supplyManagementUploadFileDetailRepository = supplyManagementUploadFileDetailRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{supplyManagementUploadFileId}")]
        public IActionResult Get(int supplyManagementUploadFileId)
        {
            return Ok(_supplyManagementUploadFileDetailRepository.GetSupplyManagementUploadFileDetailList(supplyManagementUploadFileId));
        }
    }
}
