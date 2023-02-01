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
    public class SupplyMangementRequestController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public SupplyMangementRequestController(ISupplyManagementRequestRepository supplyManagementRequestRepository,
            IUnitOfWork uow, IMapper mapper,
             IGSCContext context)
        {
            _supplyManagementRequestRepository = supplyManagementRequestRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        [Route("GetShipmentRequestList/{parentProjectId}/{siteId}/{isDeleted}")]
        public IActionResult GetShipmentRequestList(int parentProjectId, int siteId, bool isDeleted)
        {
            var productTypes = _supplyManagementRequestRepository.GetShipmentRequestList(parentProjectId, siteId, isDeleted);
            return Ok(productTypes);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementRequestDto supplyManagementRequestDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var project = _context.Project.Where(x => x.Id == supplyManagementRequestDto.FromProjectId).FirstOrDefault();
            if (project == null)
            {
                ModelState.AddModelError("Message", "From site not found");
                return BadRequest(ModelState);
            }

            supplyManagementRequestDto.Id = 0;
            var supplyManagementRequest = _mapper.Map<SupplyManagementRequest>(supplyManagementRequestDto);

            _supplyManagementRequestRepository.Add(supplyManagementRequest);
            if (_uow.Save() <= 0) throw new Exception("Creating request failed on save.");

            _supplyManagementRequestRepository.SendrequestEmail(supplyManagementRequest.Id);

            return Ok(supplyManagementRequest.Id);
        }
        [HttpGet]
        [Route("GetSiteDropdownforShipmentRequest/{ProjectId}/{ParenrProjectId}")]
        public IActionResult GetSiteDropdownforShipmentRequest(int ProjectId, int ParenrProjectId)
        {
            return Ok(_supplyManagementRequestRepository.GetSiteDropdownforShipmentRequest(ProjectId, ParenrProjectId));
        }
        [HttpGet]
        [Route("GetPharmacyStudyProductUnitType/{id}")]
        public IActionResult GetPharmacyStudyProductUnitType(int id)
        {
            return Ok(_supplyManagementRequestRepository.GetPharmacyStudyProductUnitType(id));
        }
    }
}
