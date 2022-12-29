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
                _supplyManagementKITDetailRepository.Add(obj);
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
        [Route("getIMPPerKitByKitAllocation/{visitId}")]
        public IActionResult getIMPPerKitByKitAllocation(int visitId)
        {
            var data = _context.SupplyManagementKitAllocationSettings.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == visitId).FirstOrDefault();
            return Ok(data);
        }
    }
}
