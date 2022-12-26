﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Respository.SupplyManagement;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementFectorController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementFectorRepository _supplyManagementFectorRepository;
        private readonly ISupplyManagementFectorDetailRepository _supplyManagementFectorDetailRepository;
        public SupplyManagementFectorController(
        IUnitOfWork uow, IMapper mapper,
        IJwtTokenAccesser jwtTokenAccesser,
         IGSCContext context,
         ISupplyManagementFectorRepository supplyManagementFectorRepository,
         ISupplyManagementFectorDetailRepository supplyManagementFectorDetailRepository
        )
        {

            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _supplyManagementFectorRepository = supplyManagementFectorRepository;
            _supplyManagementFectorDetailRepository = supplyManagementFectorDetailRepository;

        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var data = _supplyManagementFectorRepository.GetById(id);
            return Ok(data);
        }
        [HttpGet("GetFectorList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetFectorList(int projectId, bool isDeleted)
        {
            var productVerification = _supplyManagementFectorRepository.GetListByProjectId(projectId, isDeleted);
            return Ok(productVerification);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementFectorDto supplyManagementFectorDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            if (_supplyManagementFectorRepository.All.Any(x => x.ProjectId == supplyManagementFectorDto.ProjectId && x.DeletedDate == null))
            {
                ModelState.AddModelError("Message", "You already added for this study!");
                return BadRequest(ModelState);
            }
            supplyManagementFectorDto.Id = 0;
            var supplyManagementFector = _mapper.Map<SupplyManagementFector>(supplyManagementFectorDto);

            _supplyManagementFectorRepository.Add(supplyManagementFector);
            if (_uow.Save() <= 0) throw new Exception("Creating fector failed on save.");
            return Ok(supplyManagementFector.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementFectorDto supplyManagementFectorDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var supplyManagementFector = _mapper.Map<SupplyManagementFector>(supplyManagementFectorDto);
            _supplyManagementFectorRepository.DeleteChild(supplyManagementFectorDto.Id);
            _supplyManagementFectorRepository.Update(supplyManagementFector);
            if (_uow.Save() <= 0) throw new Exception("Updating fector failed on save.");

            return Ok(supplyManagementFector.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementFectorRepository.Find(id);
            if (record == null)
                return NotFound();
            _supplyManagementFectorRepository.Delete(record);

            var verifyRecord = _supplyManagementFectorDetailRepository.All.Where(x => x.SupplyManagementFectorId == record.Id).ToList();

            if (verifyRecord != null)
            {
                foreach (var item in verifyRecord)
                {
                    _supplyManagementFectorDetailRepository.Delete(item);
                }
            }

            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementFectorRepository.Update(record);
            _uow.Save();
           
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementFectorRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementFectorRepository.Active(record);
            var list = _supplyManagementFectorDetailRepository.All.Where(x => x.SupplyManagementFectorId == id).ToList();
            if (list != null)
            {
                foreach (var item in list)
                {
                    _supplyManagementFectorDetailRepository.Active(item);

                }
            }
            _uow.Save();
            _supplyManagementFectorRepository.UpdateFactorFormula(record.Id);
            _uow.Save();
            return Ok();
        }

        [HttpGet("GetProductTypeList/{projectId}")]
        public IActionResult GetProductTypeList(int projectId)
        {
            var productcodes = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == projectId).Distinct().Select(x => x.TreatmentType).ToList();
            if (productcodes.Any())
            {
                var data = _context.ProductType.Where(x => productcodes.Contains(x.ProductTypeCode)).Select(x => new DropDownEnum
                {
                    Code = x.ProductTypeCode,
                    Value = x.ProductTypeCode
                }).ToList();

                return Ok(data);
            }
            return Ok();
        }
    }
}
