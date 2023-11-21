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
    public class SupplyManagementKitNumberSettingsController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly ISupplyManagementKitNumberSettingsRepository _supplyManagementKitNumberSettingsRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public SupplyManagementKitNumberSettingsController(
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
             ISupplyManagementKitNumberSettingsRepository supplyManagementKitNumberSettingsRepository,
             IGSCContext context)
        {

            _uow = uow;
            _mapper = mapper;
            _supplyManagementKitNumberSettingsRepository = supplyManagementKitNumberSettingsRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKitNumberSettingsRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKitNumberSettingsDto>(centralDepo);
            centralDepoDto.RoleId = _context.SupplyManagementKitNumberSettingsRole.Where(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettingsId == id).Select(s => s.RoleId).Distinct().ToList();
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
            supplyManagementKitNumberSettingsDto.Id = supplyManagementKitNumberSettings.Id;
            _supplyManagementKitNumberSettingsRepository.SaveRoleNumberSetting(supplyManagementKitNumberSettingsDto);
            return Ok(supplyManagementKitNumberSettings.Id);

        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] SupplyManagementKitNumberSettingsDto supplyManagementKitNumberSettingsDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var setting = _supplyManagementKitNumberSettingsRepository.Find(supplyManagementKitNumberSettingsDto.Id);
            var supplyManagementKitNumberSettings = _mapper.Map<SupplyManagementKitNumberSettings>(supplyManagementKitNumberSettingsDto);
            supplyManagementKitNumberSettings.KitNoseries = supplyManagementKitNumberSettingsDto.KitNumberStartWIth;
            _supplyManagementKitNumberSettingsRepository.Update(supplyManagementKitNumberSettings);
            var mesage = _supplyManagementKitNumberSettingsRepository.CheckKitCreateion(setting);
            if (!string.IsNullOrEmpty(mesage))
            {
                ModelState.AddModelError("Message", mesage);
                return BadRequest(ModelState);
            }
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Alloation failed on save.");
            if (supplyManagementKitNumberSettingsDto.IsBlindedStudy == true)
                _supplyManagementKitNumberSettingsRepository.DeleteRoleNumberSetting(supplyManagementKitNumberSettingsDto.Id);
            _supplyManagementKitNumberSettingsRepository.SaveRoleNumberSetting(supplyManagementKitNumberSettingsDto);
            return Ok(supplyManagementKitNumberSettings.Id);

        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKitNumberSettingsRepository.Find(id);

            if (record == null)
                return NotFound();
            var mesage = _supplyManagementKitNumberSettingsRepository.CheckKitCreateion(record);
            if (!string.IsNullOrEmpty(mesage))
            {
                ModelState.AddModelError("Message", mesage);
                return BadRequest(ModelState);
            }

            if (_jwtTokenAccesser.GetHeader("audit-reason-oth") != null && _jwtTokenAccesser.GetHeader("audit-reason-oth") != "")
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (_jwtTokenAccesser.GetHeader("audit-reason-id") != null && _jwtTokenAccesser.GetHeader("audit-reason-id") != "")
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementKitNumberSettingsRepository.Update(record);
            _supplyManagementKitNumberSettingsRepository.Delete(record);
            _uow.Save();
            if (record.IsBlindedStudy == true)
                _supplyManagementKitNumberSettingsRepository.DeleteRoleNumberSetting(id);
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

        [HttpGet("CheckSettingExist/{projectId}")]
        public IActionResult CheckSettingExist(int projectId)
        {
            var data = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == projectId && x.DeletedDate == null).FirstOrDefault();
            return Ok(data);
        }
        [HttpGet("CheckIsBlidedStudy/{projectId}")]
        public IActionResult CheckIsBlidedStudy(int projectId)
        {
            bool Isblined = false;
            if (_context.SupplyManagementKitNumberSettings.Any(x => x.ProjectId == projectId && x.DeletedDate == null && x.IsBlindedStudy == true))
            {
                Isblined = true;
            }
            else
            {
                Isblined = false;
            }
            return Ok(Isblined);
        }
        [HttpGet("CheckKitMappingWithSheet/{projectId}")]
        public IActionResult CheckKitMappingWithSheet(int projectId)
        {
            bool Isblined = false;
            if (_context.SupplyManagementKitNumberSettings.Any(x => x.ProjectId == projectId && x.DeletedDate == null && x.IsUploadWithKit == true))
            {
                Isblined = true;
            }
            else
            {
                Isblined = false;
            }
            return Ok(Isblined);
        }
    }
}
