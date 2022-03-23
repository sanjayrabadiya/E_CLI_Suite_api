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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        private readonly IUnitOfWork _uow;
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly IGSCContext _context;
        public SupplyMangementShipmentController(ISupplyManagementShipmentRepository supplyManagementShipmentRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, ISupplyManagementRequestRepository supplyManagementRequestRepository, IGSCContext context)
        {
            _supplyManagementShipmentRepository = supplyManagementShipmentRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
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

            var shipmentData = _context.SupplyManagementRequest.Where(x => x.Id == supplyManagementshipmentDto.SupplyManagementRequestId).FirstOrDefault();
            if (shipmentData == null)
            {
                ModelState.AddModelError("Message", "Request data not found!");
                return BadRequest(ModelState);
            }
            var project = _context.Project.Where(x => x.Id == shipmentData.FromProjectId).FirstOrDefault();
            if (project == null)
            {
                ModelState.AddModelError("Message", "From project code not found!");
                return BadRequest(ModelState);
            }
            var productreciept = _context.ProductReceipt.Where(x => x.ProjectId == project.ParentProjectId
                                && x.Status == Helper.ProductVerificationStatus.Approved).FirstOrDefault();
            if (productreciept == null)
            {
                ModelState.AddModelError("Message", "Product receipt is pending!");
                return BadRequest(ModelState);
            }
            var productVerificationDetail = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == project.ParentProjectId
                                && x.ProductReceipt.Status == Helper.ProductVerificationStatus.Approved).FirstOrDefault();
            if (productVerificationDetail == null)
            {
                ModelState.AddModelError("Message", "Product verification is pending!");
                return BadRequest(ModelState);
            }
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                if (!_supplyManagementRequestRepository.CheckAvailableRemainingQty(supplyManagementshipmentDto.ApprovedQty, (int)project.ParentProjectId, shipmentData.StudyProductTypeId))
                {
                    ModelState.AddModelError("Message", "Approve Qauntity is greater than remaining Qauntity!");
                    return BadRequest(ModelState);
                }
            }

            supplyManagementshipmentDto.Id = 0;
            var supplyManagementRequest = _mapper.Map<SupplyManagementShipment>(supplyManagementshipmentDto);
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                bool isnotexist = false;
                while (!isnotexist)
                {
                    var str = _supplyManagementShipmentRepository.GenerateShipmentNo();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var data = _supplyManagementShipmentRepository.All.Where(x => x.ShipmentNo == str).FirstOrDefault();
                        if (data == null)
                        {
                            isnotexist = true;
                            supplyManagementRequest.ShipmentNo = str;
                            break;
                        }
                    }

                }
            }
            _supplyManagementShipmentRepository.Add(supplyManagementRequest);
            if (_uow.Save() <= 0) throw new Exception("Creating shipment failed on save.");
            return Ok(supplyManagementRequest.Id);
        }
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var data = _supplyManagementShipmentRepository.GetSupplyShipmentList(isDeleted);
            return Ok(data);
        }
        [HttpGet]
        [Route("GetRemainingQty/{SupplyManagementRequestId}")]
        public IActionResult GetRemainingQty(int SupplyManagementRequestId)
        {
            var shipmentData = _context.SupplyManagementRequest.Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (shipmentData == null)
            {
                ModelState.AddModelError("Message", "Request data not found!");
                return BadRequest(ModelState);
            }
            int getParentProjectId = (int)_context.Project.Where(x => x.Id == shipmentData.FromProjectId).FirstOrDefault().ParentProjectId;
            return Ok(_supplyManagementRequestRepository.GetAvailableRemainingQty(getParentProjectId, shipmentData.StudyProductTypeId));
        }
    }
}
