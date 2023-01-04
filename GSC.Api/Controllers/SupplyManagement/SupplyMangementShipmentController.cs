using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
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
    public class SupplyMangementShipmentController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        private readonly IUnitOfWork _uow;
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly IGSCContext _context;
        public SupplyMangementShipmentController(ISupplyManagementShipmentRepository supplyManagementShipmentRepository,
            IUnitOfWork uow, IMapper mapper,
            ISupplyManagementRequestRepository supplyManagementRequestRepository, IGSCContext context)
        {
            _supplyManagementShipmentRepository = supplyManagementShipmentRepository;
            _uow = uow;
            _mapper = mapper;

            _supplyManagementRequestRepository = supplyManagementRequestRepository;
            _context = context;
        }
        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementShipmentDto supplyManagementshipmentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            if (supplyManagementshipmentDto.SupplyManagementRequestId <= 0)
            {
                return BadRequest();
            }
            var shipmentdata = _supplyManagementRequestRepository.All.Include(x => x.PharmacyStudyProductType).Where(x => x.Id == supplyManagementshipmentDto.SupplyManagementRequestId).FirstOrDefault();
            if (shipmentdata == null)
            {
                ModelState.AddModelError("Message", "Shipment request is not available!");
                return BadRequest(ModelState);
            }
            var message = _supplyManagementShipmentRepository.ApprovalValidation(supplyManagementshipmentDto);
            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }

            supplyManagementshipmentDto.Id = 0;
            var supplyManagementRequest = _mapper.Map<SupplyManagementShipment>(supplyManagementshipmentDto);
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                supplyManagementRequest.ShipmentNo = _supplyManagementShipmentRepository.GetShipmentNo();
            }
            _supplyManagementShipmentRepository.Add(supplyManagementRequest);
            if (_uow.Save() <= 0) throw new Exception("Creating shipment failed on save.");

            //assign kit
            supplyManagementshipmentDto.Id = supplyManagementRequest.Id;
            _supplyManagementShipmentRepository.Assignkits(shipmentdata, supplyManagementshipmentDto);

            return Ok(supplyManagementRequest.Id);
        }
        [HttpGet]
        [Route("GetSupplyShipmentList/{parentProjectId}/{siteId}/{isDeleted}")]
        public IActionResult Get(int parentProjectId, int siteId, bool isDeleted)
        {
            var data = _supplyManagementShipmentRepository.GetSupplyShipmentList(parentProjectId, siteId, isDeleted);
            return Ok(data);
        }
        [HttpGet]
        [Route("GetRemainingQty/{SupplyManagementRequestId}")]
        public IActionResult GetRemainingQty(int SupplyManagementRequestId)
        {
            var shipmentData = _context.SupplyManagementRequest.Include(x => x.PharmacyStudyProductType).Include(x => x.FromProject).Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (shipmentData == null)
            {
                ModelState.AddModelError("Message", "Request data not found!");
                return BadRequest(ModelState);
            }
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == shipmentData.FromProject.ParentProjectId).FirstOrDefault();
            if (setting != null && setting.IsBlindedStudy == true)
            {
                return Ok(_supplyManagementRequestRepository.GetAvailableRemainingKitBlindedStudy(SupplyManagementRequestId));
            }
            else
            {
                if (shipmentData.PharmacyStudyProductType != null && shipmentData.PharmacyStudyProductType.ProductUnitType == Helper.ProductUnitType.Kit)
                    return Ok(_supplyManagementRequestRepository.GetAvailableRemainingKit(SupplyManagementRequestId));
            }
            return Ok(_supplyManagementRequestRepository.GetAvailableRemainingQty((int)shipmentData.FromProject.ParentProjectId, (int)shipmentData.StudyProductTypeId));
        }

        [HttpGet]
        [Route("GetAvailableKit/{SupplyManagementRequestId}")]
        public IActionResult GetAvailableKit(int SupplyManagementRequestId)
        {
            return Ok(_supplyManagementRequestRepository.GetAvailableKit(SupplyManagementRequestId));
        }
    }
}
