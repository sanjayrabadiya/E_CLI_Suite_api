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

        private readonly IMapper _mapper;
        private readonly ISupplyManagementReceiptRepository _supplyManagementReceiptRepository;
        private readonly IUnitOfWork _uow;
        private readonly ISupplyManagementKITDetailRepository _supplyManagementKITDetailRepository;
        public SupplyManagementReceiptController(ISupplyManagementReceiptRepository supplyManagementReceiptRepository,
            IUnitOfWork uow, IMapper mapper,
            ISupplyManagementKITDetailRepository supplyManagementKITDetailRepository)
        {
            _supplyManagementReceiptRepository = supplyManagementReceiptRepository;
            _uow = uow;
            _mapper = mapper;
            _supplyManagementKITDetailRepository = supplyManagementKITDetailRepository;
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
            if (supplyManagementshipmentDto.Kits != null)
            {

                foreach (var item in supplyManagementshipmentDto.Kits.Where(x => x.Status != null && x.Status != Helper.KitStatus.WithoutIssue))
                {
                    if (string.IsNullOrEmpty(item.Comments))
                    {
                        ModelState.AddModelError("Message", "Please enter comments!");
                        return BadRequest(ModelState);
                    }
                    var data = _supplyManagementKITDetailRepository.All.Where(x => x.Id == item.Id).FirstOrDefault();
                    if (data != null)
                    {
                        data.Status = item.Status;
                        data.Comments = item.Comments;
                        _supplyManagementKITDetailRepository.Update(data);
                        //_uow.Save();
                    }
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating shipment receipt failed on save.");
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
    }
}
