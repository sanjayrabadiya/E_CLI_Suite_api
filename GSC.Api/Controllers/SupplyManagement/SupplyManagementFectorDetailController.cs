﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementFectorDetailController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementFectorRepository _supplyManagementFectorRepository;
        private readonly ISupplyManagementFectorDetailRepository _supplyManagementFectorDetailRepository;
        public SupplyManagementFectorDetailController(
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
            var data = _supplyManagementFectorDetailRepository.GetDetail(id);
            return Ok(data);
        }
        [HttpGet("GetDetailList/{fectoreId}")]
        public IActionResult GetDetailList(int fectoreId)
        {
            var data = _supplyManagementFectorDetailRepository.GetDetailList(fectoreId);
            return Ok(data);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementFectorDetailDto supplyManagementFectorDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!_supplyManagementFectorDetailRepository.CheckType(supplyManagementFectorDetailDto))
            {
                ModelState.AddModelError("Message", "You can not change type");
                return BadRequest(ModelState);
            }

            if (!_supplyManagementFectorDetailRepository.CheckrandomizationStarted(supplyManagementFectorDetailDto.SupplyManagementFectorId))
            {
                ModelState.AddModelError("Message", "You can't update the factor once the Randomization is started");
                return BadRequest(ModelState);
            }
            if (!_supplyManagementFectorDetailRepository.CheckUploadRandomizationsheet(supplyManagementFectorDetailDto))
            {
                ModelState.AddModelError("Message", "Please select type as you uploaded for randomization sheet");
                return BadRequest(ModelState);
            }
            var supplyManagementFectorDetail = _mapper.Map<SupplyManagementFectorDetail>(supplyManagementFectorDetailDto);

            _supplyManagementFectorDetailRepository.Add(supplyManagementFectorDetail);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating fector detail failed on save."));

            _supplyManagementFectorRepository.UpdateFactorFormula(supplyManagementFectorDetailDto.SupplyManagementFectorId);
            return Ok(supplyManagementFectorDetail.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementFectorDetailDto supplyManagementFectorDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            if (!_supplyManagementFectorDetailRepository.CheckType(supplyManagementFectorDetailDto))
            {
                ModelState.AddModelError("Message", "You can not change type!");
                return BadRequest(ModelState);
            }
            if (!_supplyManagementFectorDetailRepository.CheckrandomizationStarted(supplyManagementFectorDetailDto.SupplyManagementFectorId))
            {
                ModelState.AddModelError("Message", "You can't update the factor once the Randomization is started!");
                return BadRequest(ModelState);
            }
            if (!_supplyManagementFectorDetailRepository.CheckUploadRandomizationsheet(supplyManagementFectorDetailDto))
            {
                ModelState.AddModelError("Message", "Please select type as you uploaded for randomization sheet");
                return BadRequest(ModelState);
            }
            var supplyManagementFectorDetail = _mapper.Map<SupplyManagementFectorDetail>(supplyManagementFectorDetailDto);

            _supplyManagementFectorDetailRepository.Update(supplyManagementFectorDetail);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating fector detail failed on save."));
            _supplyManagementFectorRepository.UpdateFactorFormula(supplyManagementFectorDetailDto.SupplyManagementFectorId);
            return Ok(supplyManagementFectorDetail.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementFectorDetailRepository.Find(id);
            if (record == null)
                return NotFound();
            var SupplyManagementFector = _context.SupplyManagementFector.Where(x => x.Id == record.SupplyManagementFectorId).FirstOrDefault();
            var randomization = _context.Randomization.Where(x => x.Project.ParentProjectId == SupplyManagementFector.ProjectId
            && x.RandomizationNumber != null).FirstOrDefault();

            if (randomization != null)
            {
                ModelState.AddModelError("Message", "You can't delete the factor once the Randomization is started!");
                return BadRequest(ModelState);
            }
            _supplyManagementFectorDetailRepository.Delete(record);
            if (!string.IsNullOrEmpty(_jwtTokenAccesser.GetHeader("audit-reason-oth")))
                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            if (!string.IsNullOrEmpty(_jwtTokenAccesser.GetHeader("audit-reason-id")))
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementFectorDetailRepository.Update(record);
            _uow.Save();
            _supplyManagementFectorRepository.UpdateFactorFormula(record.SupplyManagementFectorId);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementFectorDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementFectorDetailRepository.Active(record);

            _uow.Save();
            _supplyManagementFectorRepository.UpdateFactorFormula(record.SupplyManagementFectorId);
            _uow.Save();
            return Ok();
        }
    }
}
