﻿using AutoMapper;
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
    public class SupplyManagementKitNumberSettingsController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly ISupplyManagementKitNumberSettingsRepository _supplyManagementKitNumberSettingsRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public SupplyManagementKitNumberSettingsController(
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
             ISupplyManagementKitNumberSettingsRepository supplyManagementKitNumberSettingsRepository)
        {

            _uow = uow;
            _mapper = mapper;
            _supplyManagementKitNumberSettingsRepository = supplyManagementKitNumberSettingsRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKitNumberSettingsRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKitNumberSettingsDto>(centralDepo);
            return Ok(centralDepoDto);
        }

        [HttpGet("GetKITNumberList/{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementKitNumberSettingsRepository.GetKITNumberList(isDeleted, projectId);
            return Ok(productTypes);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] SupplyManagementKitNumberSettingsDto supplyManagementKitNumberSettingsDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementKitNumberSettingsDto.Id = 0;

            var supplyManagementKitNumberSettings = _mapper.Map<SupplyManagementKitNumberSettings>(supplyManagementKitNumberSettingsDto);
            supplyManagementKitNumberSettings.KitNoseries = supplyManagementKitNumberSettingsDto.KitNumberStartWIth;
            _supplyManagementKitNumberSettingsRepository.Add(supplyManagementKitNumberSettings);
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Alloation failed on save.");

            return Ok(supplyManagementKitNumberSettings.Id);

        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] SupplyManagementKitNumberSettingsDto supplyManagementKitNumberSettingsDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var supplyManagementKitNumberSettings = _mapper.Map<SupplyManagementKitNumberSettings>(supplyManagementKitNumberSettingsDto);
            supplyManagementKitNumberSettings.KitNoseries = supplyManagementKitNumberSettingsDto.KitNumberStartWIth;
            _supplyManagementKitNumberSettingsRepository.Update(supplyManagementKitNumberSettings);
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Alloation failed on save.");

            return Ok(supplyManagementKitNumberSettings.Id);

        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKitNumberSettingsRepository.Find(id);

            if (record == null)
                return NotFound();

            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementKitNumberSettingsRepository.Update(record);
            _supplyManagementKitNumberSettingsRepository.Delete(record);
            _uow.Save();
            return Ok();
        }


        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementKitNumberSettingsRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementKitNumberSettingsRepository.Active(record);
            _uow.Save();

            return Ok();
        }
       
    }
}
