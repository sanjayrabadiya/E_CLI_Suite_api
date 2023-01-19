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
    public class SupplyManagementAllocationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementAllocationRepository _supplyManagementAllocationRepository;
        private readonly IUnitOfWork _uow;

        public SupplyManagementAllocationController(ISupplyManagementAllocationRepository supplyManagementAllocationRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _supplyManagementAllocationRepository = supplyManagementAllocationRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("GetSupplyAllocationList/{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementAllocationRepository.GetSupplyAllocationList(isDeleted, projectId);
            return Ok(productTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementAllocationRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementAllocationDto>(centralDepo);
            return Ok(centralDepoDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementAllocationDto centralDepotDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            centralDepotDto.Id = 0;
            var centralDepot = _mapper.Map<SupplyManagementAllocation>(centralDepotDto);
            if (!_supplyManagementAllocationRepository.CheckRandomizationAssign(centralDepot))
            {
                ModelState.AddModelError("Message", "You can't add the template configuration once the Randomization is started!");
                return BadRequest(ModelState);
            }
            var validate = _supplyManagementAllocationRepository.CheckDuplicate(centralDepot);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _supplyManagementAllocationRepository.Add(centralDepot);
            if (_uow.Save() <= 0) throw new Exception("Creating central depot failed on save.");
            return Ok(centralDepot.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementAllocationDto centralDepotDto)
        {
            if (centralDepotDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
           
            var centralDepot = _mapper.Map<SupplyManagementAllocation>(centralDepotDto);
            if (!_supplyManagementAllocationRepository.CheckRandomizationAssign(centralDepot))
            {
                ModelState.AddModelError("Message", "You can't update the template configuration once the Randomization is started!");
                return BadRequest(ModelState);
            }
            var validate = _supplyManagementAllocationRepository.CheckDuplicate(centralDepot);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            centralDepot.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            centralDepot.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementAllocationRepository.AddOrUpdate(centralDepot);

            if (_uow.Save() <= 0) throw new Exception("Updating central depot failed on save.");
            return Ok(centralDepot.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementAllocationRepository.Find(id);

            if (record == null)
                return NotFound();

            if (!_supplyManagementAllocationRepository.CheckRandomizationAssign(record))
            {
                ModelState.AddModelError("Message", "You can't delete the template configuration once the Randomization is started!");
                return BadRequest(ModelState);
            }
            _supplyManagementAllocationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementAllocationRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementAllocationRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet]
        [Route("GetParentProjectDropDownProjectRight")]
        public IActionResult GetParentProjectDropDownProjectRight()
        {
            return Ok(_supplyManagementAllocationRepository.GetParentProjectDropDownProjectRight());
        }

        [HttpGet]
        [Route("GetVisitDropDownByRandomization/{projectId}")]
        public IActionResult GetVisitDropDownByRandomization(int projectId)
        {
            return Ok(_supplyManagementAllocationRepository.GetVisitDropDownByRandomization(projectId));
        }
        [HttpGet]
        [Route("GetTemplateDropDownByVisitId/{visitId}")]
        public IActionResult GetTemplateDropDownByVisitId(int visitId)
        {
            return Ok(_supplyManagementAllocationRepository.GetTemplateDropDownByVisitId(visitId));
        }

        [HttpGet]
        [Route("GetVariableDropDownByTemplateId/{templateId}")]
        public IActionResult GetVariableDropDownByTemplateId(int templateId)
        {
            return Ok(_supplyManagementAllocationRepository.GetVariableDropDownByTemplateId(templateId));
        }

        [HttpGet]
        [Route("GetProductTypeByVisit/{visitId}")]
        public IActionResult GetProductTypeByVisit(int visitId)
        {
            return Ok(_supplyManagementAllocationRepository.GetProductTypeByVisit(visitId));
        }
        [HttpGet]
        [Route("GetPharmacyStudyProductTypeDropDown/{projectId}")]
        public IActionResult GetPharmacyStudyProductTypeDropDown(int projectId)
        {
            return Ok(_supplyManagementAllocationRepository.GetPharmacyStudyProductTypeDropDown(projectId));
        }
    }
}
