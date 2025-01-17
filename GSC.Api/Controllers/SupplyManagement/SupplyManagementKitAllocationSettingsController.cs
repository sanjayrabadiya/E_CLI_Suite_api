﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
        private readonly IGSCContext _context;
        public SupplyManagementKitAllocationSettingsController(
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
             ISupplyManagementKitAllocationSettingsRepository supplyManagementKitAllocationSettingsRepository,
             IGSCContext context)
        {

            _uow = uow;
            _mapper = mapper;
            _supplyManagementKitAllocationSettingsRepository = supplyManagementKitAllocationSettingsRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKitAllocationSettingsRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKitAllocationSettingsDto>(centralDepo);
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
            if (_supplyManagementKitAllocationSettingsRepository.All.Any(x => x.DeletedDate == null && x.PharmacyStudyProductTypeId == supplyManagementKitAllocationSettingsDto.PharmacyStudyProductTypeId && x.ProjectDesignVisitId == supplyManagementKitAllocationSettingsDto.ProjectDesignVisitId))
            {
                ModelState.AddModelError("Message", "You already added visit!");
                return BadRequest(ModelState);
            }

            var supplyManagementKitAllocationSettings = _mapper.Map<SupplyManagementKitAllocationSettings>(supplyManagementKitAllocationSettingsDto);
            supplyManagementKitAllocationSettings.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementKitAllocationSettings.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyManagementKitAllocationSettingsRepository.Add(supplyManagementKitAllocationSettings);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Kit Alloation failed on save."));

            return Ok(supplyManagementKitAllocationSettings.Id);

        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] SupplyManagementKitAllocationSettingsDto supplyManagementKitAllocationSettingsDto)
        {
            if (_supplyManagementKitAllocationSettingsRepository.All.Any(x => x.DeletedDate == null && x.Id != supplyManagementKitAllocationSettingsDto.Id && x.PharmacyStudyProductTypeId == supplyManagementKitAllocationSettingsDto.PharmacyStudyProductTypeId && x.ProjectDesignVisitId == supplyManagementKitAllocationSettingsDto.ProjectDesignVisitId))
            {
                ModelState.AddModelError("Message", "You already added visit!");
                return BadRequest(ModelState);
            }
            var project = _context.ProjectDesignVisit.Include(x => x.ProjectDesignPeriod).ThenInclude(x => x.ProjectDesign).Where(x => x.DeletedDate == null && x.Id == supplyManagementKitAllocationSettingsDto.ProjectDesignVisitId).FirstOrDefault();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == project.ProjectDesignPeriod.ProjectDesign.ProjectId).FirstOrDefault();
            if (setting != null)
            {
                if (setting.KitCreationType == KitCreationType.KitWise)
                {
                    if (_context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Any(z => z.DeletedDate == null && z.SupplyManagementKIT.ProjectId == project.ProjectDesignPeriod.ProjectDesign.ProjectId && z.SupplyManagementKIT.PharmacyStudyProductTypeId == supplyManagementKitAllocationSettingsDto.PharmacyStudyProductTypeId))
                    {
                        ModelState.AddModelError("Message", "Kit already been prepared you can not modify record!");
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    if (_context.SupplyManagementKITSeriesDetail.Include(s => s.SupplyManagementKITSeries).Any(z => z.DeletedDate == null && z.SupplyManagementKITSeries.ProjectId == project.ProjectDesignPeriod.ProjectDesign.ProjectId && z.PharmacyStudyProductTypeId == supplyManagementKitAllocationSettingsDto.PharmacyStudyProductTypeId))
                    {
                        ModelState.AddModelError("Message", "Kit already been prepared you can not modify record!");
                        return BadRequest(ModelState);
                    }
                }
            }

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var supplyManagementKitAllocationSettings = _mapper.Map<SupplyManagementKitAllocationSettings>(supplyManagementKitAllocationSettingsDto);
            supplyManagementKitAllocationSettings.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementKitAllocationSettings.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyManagementKitAllocationSettingsRepository.Update(supplyManagementKitAllocationSettings);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Kit Alloation failed on save."));

            return Ok(supplyManagementKitAllocationSettings.Id);

        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKitAllocationSettingsRepository.Find(id);

            if (record == null)
                return NotFound();

            var project = _context.ProjectDesignVisit.Include(x => x.ProjectDesignPeriod).ThenInclude(x => x.ProjectDesign).Where(x => x.DeletedDate == null && x.Id == record.ProjectDesignVisitId).FirstOrDefault();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == project.ProjectDesignPeriod.ProjectDesign.ProjectId).FirstOrDefault();
            if (setting != null)
            {
                if (setting.KitCreationType == KitCreationType.KitWise)
                {
                    var kit = _context.SupplyManagementKIT.Where(x => x.ProjectDesignVisitId == record.ProjectDesignVisitId && x.PharmacyStudyProductTypeId == record.PharmacyStudyProductTypeId && x.DeletedDate == null).Select(x => x.Id).ToList();
                    if (kit.Count > 0)
                    {
                        ModelState.AddModelError("Message", "You can't able to delete because kit has already created!");
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    if (_context.SupplyManagementKITSeriesDetail.Include(s => s.SupplyManagementKITSeries).Any(z => z.DeletedDate == null && z.SupplyManagementKITSeries.ProjectId == project.ProjectDesignPeriod.ProjectDesign.ProjectId && z.PharmacyStudyProductTypeId == record.PharmacyStudyProductTypeId))
                    {
                        ModelState.AddModelError("Message", "Kit already been prepared you can not modify record!");
                        return BadRequest(ModelState);
                    }
                }
            }
            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementKitAllocationSettingsRepository.Update(record);
            _supplyManagementKitAllocationSettingsRepository.Delete(record);
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
