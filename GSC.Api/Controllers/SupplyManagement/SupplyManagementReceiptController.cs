using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
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
    public class SupplyManagementReceiptController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementReceiptRepository _supplyManagementReceiptRepository;
        private readonly IUnitOfWork _uow;

        public SupplyManagementReceiptController(ISupplyManagementReceiptRepository supplyManagementReceiptRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _supplyManagementReceiptRepository = supplyManagementReceiptRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementReceiptDto supplyManagementshipmentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            if (supplyManagementshipmentDto.WithIssue && string.IsNullOrEmpty(supplyManagementshipmentDto.Description))
            {
                ModelState.AddModelError("Message", "Description is mandatory!");
                return BadRequest(ModelState);
            }
            supplyManagementshipmentDto.Id = 0;
            var supplyManagementRequest = _mapper.Map<SupplyManagementReceipt>(supplyManagementshipmentDto);

            _supplyManagementReceiptRepository.Add(supplyManagementRequest);
            if (_uow.Save() <= 0) throw new Exception("Creating shipment receipt failed on save.");
            return Ok(supplyManagementRequest.Id);
        }
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var data = _supplyManagementReceiptRepository.GetSupplyShipmentReceiptList(isDeleted);
            return Ok(data);
        }
    }
}
