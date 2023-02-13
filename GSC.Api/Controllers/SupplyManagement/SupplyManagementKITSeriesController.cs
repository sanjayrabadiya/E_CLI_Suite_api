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
    public class SupplyManagementKITSeriesController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementKITRepository _supplyManagementKITRepository;
        private readonly ISupplyManagementKITDetailRepository _supplyManagementKITDetailRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementKITSeriesRepository _supplyManagementKITSeriesRepository;
        public SupplyManagementKITSeriesController(ISupplyManagementKITRepository supplyManagementKITRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, ISupplyManagementKITDetailRepository supplyManagementKITDetailRepository, ISupplyManagementKITSeriesRepository supplyManagementKITSeriesRepository)
        {
            _supplyManagementKITRepository = supplyManagementKITRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _supplyManagementKITDetailRepository = supplyManagementKITDetailRepository;
            _supplyManagementKITSeriesRepository = supplyManagementKITSeriesRepository;
        }

        [HttpGet("GetKITSeriesList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetKITSeriesList(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementKITRepository.GetKITSeriesList(isDeleted, projectId);
            return Ok(productTypes);
        }
        [HttpGet("GetKITSeriesDetailList/{id}")]
        public IActionResult GetKITSeriesDetailList(int id)
        {
            var productTypes = _supplyManagementKITRepository.GetKITSeriesDetailList(id);
            return Ok(productTypes);
        }
        [HttpGet("GetKITSeriesDetailHistoryList/{id}")]
        public IActionResult GetKITSeriesDetailHistoryList(int id)
        {
            var productTypes = _supplyManagementKITRepository.GetKITSeriesDetailHistoryList(id);
            return Ok(productTypes);
        }

        [HttpPost]
        [Route("AddKitSequence")]
        [TransactionRequired]
        public IActionResult AddKitSequence([FromBody] SupplyManagementKITSeriesDto supplyManagementKITSeriesDto)
        {

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var kitsettings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == supplyManagementKITSeriesDto.ProjectId).FirstOrDefault();
            if (kitsettings == null)
            {
                ModelState.AddModelError("Message", "please set kit number formate!");
                return BadRequest(ModelState);
            }
            var message = _supplyManagementKITRepository.CheckAvailableQtySequenceKit(supplyManagementKITSeriesDto);
            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }

            for (int i = 0; i < supplyManagementKITSeriesDto.NoofPatient; i++)
            {
                supplyManagementKITSeriesDto.Id = 0;
                var kitnoseriese = kitsettings.KitNoseries;
                var supplyManagementKitSeries = _mapper.Map<SupplyManagementKITSeries>(supplyManagementKITSeriesDto);
                supplyManagementKitSeries.Status = KitStatus.AllocationPending;
                supplyManagementKitSeries.KitNo = _supplyManagementKITRepository.GenerateKitNo(kitsettings, kitnoseriese);
                _supplyManagementKITSeriesRepository.Add(supplyManagementKitSeries);
                if (_uow.Save() <= 0) throw new Exception("Creating Kit Series Creation failed on save.");

                supplyManagementKITSeriesDto.Id = supplyManagementKitSeries.Id;
                _supplyManagementKITSeriesRepository.AddKitSeriesVisitDetail(supplyManagementKITSeriesDto);
                ++kitsettings.KitNoseries;
            }
            _context.SupplyManagementKitNumberSettings.Update(kitsettings);
            _uow.Save();
            return Ok(supplyManagementKITSeriesDto.Id);

        }





        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKITSeriesRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.Status != KitStatus.AllocationPending)
            {
                ModelState.AddModelError("Message", "Kit should not be deleted once the shipment/receipt has been generated!");
                return BadRequest(ModelState);
            }

            var kitnumber = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == record.ProjectId && x.DeletedDate == null).FirstOrDefault();
            if (kitnumber != null)
            {
                kitnumber.KitNoseries = kitnumber.KitNoseries - 1;
                _context.SupplyManagementKitNumberSettings.Update(kitnumber);
            }
            var visitdetails = _context.SupplyManagementKITSeriesDetail.Where(x => x.SupplyManagementKITSeriesId == record.Id).ToList();
            if (visitdetails != null && visitdetails.Count > 0)
            {
                foreach (var item1 in visitdetails)
                {
                    item1.DeletedDate = DateTime.Now;
                    item1.DeletedBy = _jwtTokenAccesser.UserId;
                    _context.SupplyManagementKitNumberSettings.Update(kitnumber);
                }
            }
            _supplyManagementKITSeriesRepository.Delete(record);
            _uow.Save();

            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

            _supplyManagementKITSeriesRepository.Update(record);
            _uow.Save();
            return Ok();
        }
        [HttpPost("DeleteKitsSequence")]
        [TransactionRequired]
        public IActionResult DeleteKitsSequence([FromBody] DeleteKitDto deleteKitDto)
        {
            if (deleteKitDto.list.Count == 0)
            {
                ModelState.AddModelError("Message", "please select atleast one kit!");
                return BadRequest(ModelState);
            }
            foreach (var item in deleteKitDto.list)
            {
                var record = _supplyManagementKITSeriesRepository.Find(item);
                if (record == null)
                    return NotFound();
                if (record.Status != KitStatus.AllocationPending)
                {
                    ModelState.AddModelError("Message", "Kit " + record.KitNo + " should not be deleted once the shipment/receipt has been generated!");
                    return BadRequest(ModelState);
                }
                var kitnumber = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == record.ProjectId && x.DeletedDate == null).FirstOrDefault();
                if (kitnumber != null)
                {
                    kitnumber.KitNoseries = kitnumber.KitNoseries - 1;
                    _context.SupplyManagementKitNumberSettings.Update(kitnumber);
                }
                var visitdetails = _context.SupplyManagementKITSeriesDetail.Where(x => x.SupplyManagementKITSeriesId == record.Id).ToList();
                if (visitdetails != null && visitdetails.Count > 0)
                {
                    foreach (var item1 in visitdetails)
                    {
                        item1.DeletedDate = DateTime.Now;
                        item1.DeletedBy = _jwtTokenAccesser.UserId;
                        _context.SupplyManagementKitNumberSettings.Update(kitnumber);
                    }
                }

                _supplyManagementKITSeriesRepository.Delete(record);
                _uow.Save();

                record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));

                _supplyManagementKITSeriesRepository.Update(record);
                _uow.Save();
            }

            return Ok();
        }


    }
}
