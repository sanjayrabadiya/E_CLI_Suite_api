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
    public class SupplyManagementKITController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementKITRepository _supplyManagementKITRepository;
        private readonly ISupplyManagementKITDetailRepository _supplyManagementKITDetailRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public SupplyManagementKITController(ISupplyManagementKITRepository supplyManagementKITRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, ISupplyManagementKITDetailRepository supplyManagementKITDetailRepository)
        {
            _supplyManagementKITRepository = supplyManagementKITRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _supplyManagementKITDetailRepository = supplyManagementKITDetailRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKITRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKITDto>(centralDepo);
            return Ok(centralDepoDto);
        }

        [HttpGet("GetKITList/{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementKITRepository.GetKITList(isDeleted, projectId);
            return Ok(productTypes);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] SupplyManagementKITDto supplyManagementUploadFileDto)
        {
            List<SupplyManagementKITDetail> list = new List<SupplyManagementKITDetail>();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementUploadFileDto.Id = 0;
            var kitsettings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == supplyManagementUploadFileDto.ProjectId).FirstOrDefault();
            if (kitsettings == null)
            {
                ModelState.AddModelError("Message", "please set kit number formate!");
                return BadRequest(ModelState);
            }
            var supplyManagementUploadFile = _mapper.Map<SupplyManagementKIT>(supplyManagementUploadFileDto);
            supplyManagementUploadFile.TotalUnits = (supplyManagementUploadFileDto.NoOfImp * supplyManagementUploadFileDto.NoofPatient);
            _supplyManagementKITRepository.Add(supplyManagementUploadFile);
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Creation failed on save.");


            for (int i = 0; i < supplyManagementUploadFile.NoofPatient; i++)
            {
                var kitnoseriese = kitsettings.KitNoseries;
                SupplyManagementKITDetail obj = new SupplyManagementKITDetail();
                obj.KitNo = _supplyManagementKITRepository.GenerateKitNo(kitsettings, kitnoseriese);
                obj.SupplyManagementKITId = supplyManagementUploadFile.Id;
                obj.Status = KitStatus.AllocationPending;
                obj.NoOfImp = supplyManagementUploadFileDto.NoOfImp;
                _supplyManagementKITDetailRepository.Add(obj);
                _uow.Save();

                SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                history.SupplyManagementKITDetailId = obj.Id;
                history.Status = KitStatus.AllocationPending;
                history.RoleId = _jwtTokenAccesser.RoleId;
                _supplyManagementKITRepository.InsertKitHistory(history);
                _uow.Save();

                ++kitsettings.KitNoseries;
            }
            _context.SupplyManagementKitNumberSettings.Update(kitsettings);
            _uow.Save();
            return Ok(supplyManagementUploadFile.Id);

        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKITDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            var kit = _context.SupplyManagementKIT.Where(x => x.Id == record.SupplyManagementKITId).FirstOrDefault();
            if (kit != null)
            {
                var kitnumber = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == kit.ProjectId && x.DeletedDate == null).FirstOrDefault();
                kitnumber.KitNoseries = kitnumber.KitNoseries - 1;
                _context.SupplyManagementKitNumberSettings.Update(kitnumber);
            }

            if (record.Status != KitStatus.AllocationPending)
            {
                ModelState.AddModelError("Message", "Kit should not be deleted once the shipment/receipt has been generated!");
                return BadRequest(ModelState);
            }

            _supplyManagementKITDetailRepository.Delete(record);
            _uow.Save();

            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

            _supplyManagementKITDetailRepository.Update(record);
            _uow.Save();
            return Ok();
        }
        [HttpPost("DeleteKits")]
        [TransactionRequired]
        public IActionResult DeleteKits([FromBody] DeleteKitDto deleteKitDto)
        {
            if (deleteKitDto.list.Count == 0)
            {
                ModelState.AddModelError("Message", "please select atleast one kit!");
                return BadRequest(ModelState);
            }
            foreach (var item in deleteKitDto.list)
            {
                var record = _supplyManagementKITDetailRepository.Find(item);

                var kit = _context.SupplyManagementKIT.Where(x => x.Id == record.SupplyManagementKITId).FirstOrDefault();
                if (kit != null)
                {
                    var kitnumber = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == kit.ProjectId && x.DeletedDate == null).FirstOrDefault();
                    kitnumber.KitNoseries = kitnumber.KitNoseries - 1;
                    _context.SupplyManagementKitNumberSettings.Update(kitnumber);
                }
                if (record.Status != KitStatus.AllocationPending)
                {
                    ModelState.AddModelError("Message", "Kit should not be deleted once the shipment/receipt has been generated!");
                    return BadRequest(ModelState);
                }

                if (record == null)
                    return NotFound();

                _supplyManagementKITDetailRepository.Delete(record);
                _uow.Save();

                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

                _supplyManagementKITDetailRepository.Update(record);
                _uow.Save();
            }

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementKITRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementKITRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVisitDropDownByAllocation/{projectId}")]
        public IActionResult GetVisitDropDownByAllocation(int projectId)
        {
            return Ok(_supplyManagementKITRepository.GetVisitDropDownByAllocation(projectId));
        }

        [HttpGet]
        [Route("getApprovedKit/{id}")]
        public IActionResult getApprovedKit(int id)
        {
            return Ok(_supplyManagementKITRepository.getApprovedKit(id));
        }

        [HttpGet]
        [Route("getIMPPerKitByKitAllocation/{visitId}/{pharmacyStudyProductTypeId}")]
        public IActionResult getIMPPerKitByKitAllocation(int visitId, int pharmacyStudyProductTypeId)
        {
            var data = _context.SupplyManagementKitAllocationSettings.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == visitId && x.PharmacyStudyProductTypeId == pharmacyStudyProductTypeId).FirstOrDefault();
            return Ok(data);
        }

        [HttpGet]
        [Route("GetAvailableRemainingkitCount/{projectId}/{projecttypeId}")]
        public IActionResult GetAvailableRemainingkitCount(int projectId, int projecttypeId)
        {
            return Ok(_supplyManagementKITRepository.GetAvailableRemainingkitCount(projectId, projecttypeId));
        }

        [HttpGet("GetRandomizationKitNumberAssignList/{projectId}/{siteId}/{id}")]
        public IActionResult GetRandomizationKitNumberAssignList(int projectId, int siteId, int id)
        {
            var productTypes = _supplyManagementKITRepository.GetRandomizationKitNumberAssignList(projectId, siteId, id);
            return Ok(productTypes);
        }
        [HttpGet("GetRandomizationDropdownKit/{projectId}")]
        public IActionResult GetRandomizationDropdownKit(int projectId)
        {
            var productTypes = _supplyManagementKITRepository.GetRandomizationDropdownKit(projectId);
            return Ok(productTypes);
        }

        [HttpPost]
        [Route("AssignKitNumber")]
        public IActionResult AssignKitNumber([FromBody] SupplyManagementVisitKITDetailDto supplyManagementVisitKITDetailDto)
        {
            supplyManagementVisitKITDetailDto = _supplyManagementKITRepository.SetKitNumber(supplyManagementVisitKITDetailDto);
            if (string.IsNullOrEmpty(supplyManagementVisitKITDetailDto.KitNo))
            {
                ModelState.AddModelError("Message", "Kit is not available");
                return BadRequest(ModelState);
            }
            return Ok();
        }

        [HttpGet("GetKitHistory/{id}")]
        public IActionResult GetKitHistory(int id)
        {
            var history = _supplyManagementKITRepository.KitHistoryList(id);
            return Ok(history);
        }

        [HttpGet]
        [Route("GetKitReturnList/{projectId}/{kitType}/{siteId?}/{visitId?}/{randomizationId?}")]
        public IActionResult GetKitReturnList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            return Ok(_supplyManagementKITRepository.GetKitReturnList(projectId, kitType, siteId, visitId, randomizationId));
        }

        [HttpPost]
        [Route("ReturnSave")]
        public IActionResult ReturnSave([FromBody] SupplyManagementKITReturnGridDto supplyManagementKITReturnGridDto)
        {
            var returnkit = _supplyManagementKITRepository.ReturnSave(supplyManagementKITReturnGridDto);
            return Ok(returnkit);
        }
        [HttpPost]
        [Route("ReturnSaveAll")]
        public IActionResult ReturnSaveAll([FromBody] SupplyManagementKITReturnDtofinal supplyManagementKITReturnGridDto)
        {
             _supplyManagementKITRepository.ReturnSaveAll(supplyManagementKITReturnGridDto);
            return Ok();
        }

        [HttpGet]
        [Route("GetKitDiscardList/{projectId}/{kitType}/{siteId?}/{visitId?}/{randomizationId?}")]
        public IActionResult GetKitDiscardList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            return Ok(_supplyManagementKITRepository.GetKitDiscardList(projectId, kitType, siteId, visitId, randomizationId));
        }

        [HttpPost]
        [Route("KitDiscard")]
        public IActionResult KitDiscard([FromBody] SupplyManagementKITDiscardDtofinal supplyManagementKITReturnGridDto)
        {
            _supplyManagementKITRepository.KitDiscard(supplyManagementKITReturnGridDto);
            return Ok();
        }
        [HttpPost]
        [Route("SendToSponser")]
        public IActionResult KitSendtoSponser([FromBody] SupplyManagementKITDiscardDtofinal supplyManagementKITReturnGridDto)
        {
            _supplyManagementKITRepository.KitSendtoSponser(supplyManagementKITReturnGridDto);
            return Ok();
        }

    }
}
