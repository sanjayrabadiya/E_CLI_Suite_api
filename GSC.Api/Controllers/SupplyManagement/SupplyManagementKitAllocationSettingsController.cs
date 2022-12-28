using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementKitAllocationSettingsController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly ISupplyManagementKitAllocationSettingsRepository _supplyManagementKitAllocationSettingsRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public SupplyManagementKitAllocationSettingsController(
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
             ISupplyManagementKitAllocationSettingsRepository supplyManagementKitAllocationSettingsRepository)
        {

            _uow = uow;
            _mapper = mapper;
            _supplyManagementKitAllocationSettingsRepository = supplyManagementKitAllocationSettingsRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKitAllocationSettingsRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKITDto>(centralDepo);
            return Ok(centralDepoDto);
        }

        [HttpGet("GetKITAllocationList/{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementKitAllocationSettingsRepository.GetKITAllocationList(isDeleted, projectId);
            return Ok(productTypes);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] SupplyManagementKitAllocationSettingsDto supplyManagementKitAllocationSettingsDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementKitAllocationSettingsDto.Id = 0;

            var supplyManagementKitAllocationSettings = _mapper.Map<SupplyManagementKitAllocationSettings>(supplyManagementKitAllocationSettingsDto);
            _supplyManagementKitAllocationSettingsRepository.Add(supplyManagementKitAllocationSettings);
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Alloation failed on save.");

            return Ok(supplyManagementKitAllocationSettings.Id);

        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] SupplyManagementKitAllocationSettingsDto supplyManagementKitAllocationSettingsDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var supplyManagementKitAllocationSettings = _mapper.Map<SupplyManagementKitAllocationSettings>(supplyManagementKitAllocationSettingsDto);
            _supplyManagementKitAllocationSettingsRepository.Update(supplyManagementKitAllocationSettings);
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Alloation failed on save.");

            return Ok(supplyManagementKitAllocationSettings.Id);

        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKitAllocationSettingsRepository.Find(id);

            if (record == null)
                return NotFound();

            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));


            _uow.Save();
            return Ok();
        }


        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementKitAllocationSettingsRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementKitAllocationSettingsRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet("GetVisitDropDownByProjectId/{projectId}")]
        public IActionResult GetVisitDropDownByProjectId(int projectId)
        {
            var productTypes = _supplyManagementKitAllocationSettingsRepository.GetVisitDropDownByProjectId(projectId);
            return Ok(productTypes);
        }
    }
}
