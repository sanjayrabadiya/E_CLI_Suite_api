using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        private readonly IUnitOfWork _uow;

        public SupplyManagementReceiptController(ISupplyManagementReceiptRepository supplyManagementReceiptRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, ISupplyManagementShipmentRepository supplyManagementShipmentRepository)
        {
            _supplyManagementReceiptRepository = supplyManagementReceiptRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _supplyManagementShipmentRepository = supplyManagementShipmentRepository;
        }
        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementReceiptDto supplyManagementshipmentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
          
            var shipment = _supplyManagementShipmentRepository.All.Include(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x => x.Id == supplyManagementshipmentDto.SupplyManagementShipmentId).FirstOrDefault();
            if (shipment == null)
            {

                ModelState.AddModelError("Message", "Shipment does not exist!");
                return BadRequest(ModelState);
            }
            var message = _supplyManagementReceiptRepository.CheckValidationKitReciept(supplyManagementshipmentDto);
            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }
            supplyManagementshipmentDto.Id = 0;
            var supplyManagementRequest = _mapper.Map<SupplyManagementReceipt>(supplyManagementshipmentDto);
            supplyManagementRequest.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementRequest.RoleId = _jwtTokenAccesser.RoleId;
            supplyManagementRequest.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyManagementReceiptRepository.Add(supplyManagementRequest);
            _supplyManagementReceiptRepository.UpdateKitStatus(supplyManagementshipmentDto, shipment);
            return Ok(supplyManagementRequest.Id);
        }

        [HttpGet]
        [Route("GetSupplyShipmentReceiptList/{parentProjectId}/{siteId}/{isDeleted}")]
        public IActionResult Get(int parentProjectId, int siteId, bool isDeleted)
        {
            var data = _supplyManagementReceiptRepository.GetSupplyShipmentReceiptList(parentProjectId, siteId, isDeleted);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetSupplyShipmentReceiptHistory/{id}")]
        public IActionResult GetSupplyShipmentReceiptHistory(int id)
        {
            var data = _supplyManagementReceiptRepository.GetSupplyShipmentReceiptHistory(id);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetKitAllocatedList/{id}/{type}")]
        public IActionResult GetKitAllocatedList(int id, string type)
        {
            var data = _supplyManagementReceiptRepository.GetKitAllocatedList(id, type);
            return Ok(data);
        }

        [HttpGet]
        [Route("CheckExpiryOnReceipt/{projectId}/{id}")]
        public IActionResult CheckExpiryOnReceipt(int projectId, int id)
        {
            var data = _supplyManagementReceiptRepository.CheckExpiryOnReceipt(projectId, id);
            if (!string.IsNullOrEmpty(data))
            {
                ModelState.AddModelError("Message", data);
                return BadRequest(ModelState);
            }
            return Ok(true);
        }
    }
}
