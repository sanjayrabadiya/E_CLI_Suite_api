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
using System.Collections.Generic;
using System.Linq;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementKitSeriesController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementKitRepository _supplyManagementKITRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementKitSeriesRepository _supplyManagementKITSeriesRepository;
        public SupplyManagementKitSeriesController(ISupplyManagementKitRepository supplyManagementKITRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, ISupplyManagementKitSeriesRepository supplyManagementKITSeriesRepository)
        {
            _supplyManagementKITRepository = supplyManagementKITRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _supplyManagementKITSeriesRepository = supplyManagementKITSeriesRepository;
        }

        [HttpGet("GetKITSeriesList/{projectId}/{siteId}/{isDeleted:bool?}")]
        public IActionResult GetKITSeriesList(int projectId, int siteId, bool isDeleted)
        {
            var productTypes = _supplyManagementKITRepository.GetKITSeriesList(isDeleted, projectId, siteId);
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

            var expire = _supplyManagementKITSeriesRepository.CheckExpiryDateSequenceWise(supplyManagementKITSeriesDto);
            if (!string.IsNullOrEmpty(expire))
            {
                ModelState.AddModelError("Message", expire);
                return BadRequest(ModelState);
            }

            if (kitsettings.IsUploadWithKit)
            {
                var uploadedkits = _context.SupplyManagementUploadFileDetail.Include(s => s.SupplyManagementUploadFile).Where(s => s.SupplyManagementUploadFile.ProjectId == supplyManagementKITSeriesDto.ProjectId
                                    && s.TreatmentType.ToLower() == supplyManagementKITSeriesDto.TreatmentType.ToLower() && s.SupplyManagementKITSeriesId == null && s.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve && s.DeletedDate == null).Count();
                if (uploadedkits < supplyManagementKITSeriesDto.NoofPatient)
                {
                    ModelState.AddModelError("Message", "You can not create kits more than mention into the randomization sheet");
                    return BadRequest(ModelState);
                }
            }

            for (int i = 0; i < supplyManagementKITSeriesDto.NoofPatient; i++)
            {
                bool isexist = false;
                while (!isexist)
                {

                    supplyManagementKITSeriesDto.Id = 0;
                    var supplyManagementKitSeries = _mapper.Map<SupplyManagementKITSeries>(supplyManagementKITSeriesDto);
                    supplyManagementKitSeries.Status = KitStatus.AllocationPending;
                    supplyManagementKitSeries.KitNo = _supplyManagementKITSeriesRepository.GenerateKitSequenceNo(kitsettings, 1, supplyManagementKITSeriesDto);
                    supplyManagementKitSeries.KitExpiryDate = _supplyManagementKITSeriesRepository.GetExpiryDateSequenceWise(supplyManagementKITSeriesDto);
                    supplyManagementKitSeries.IpAddress = _jwtTokenAccesser.IpAddress;
                    supplyManagementKitSeries.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                    if (kitsettings.IsBarcodeScan)
                        supplyManagementKitSeries.Barcode = _supplyManagementKITSeriesRepository.GenerateKitPackBarcode(supplyManagementKITSeriesDto);
                    _supplyManagementKITSeriesRepository.Add(supplyManagementKitSeries);
                    if (!_supplyManagementKITSeriesRepository.All.Any(x => x.KitNo == supplyManagementKitSeries.KitNo && x.ProjectId == supplyManagementKITSeriesDto.ProjectId && x.DeletedDate == null))
                    {
                        if (_uow.Save() <= 0) return Ok(new Exception("Creating Kit Series Creation failed on save."));

                        supplyManagementKITSeriesDto.Id = supplyManagementKitSeries.Id;
                        supplyManagementKITSeriesDto.KitNo = supplyManagementKitSeries.KitNo;
                        _supplyManagementKITSeriesRepository.AddKitSeriesVisitDetail(supplyManagementKITSeriesDto);
                        isexist = true;
                        _uow.Save();
                    }
                    else
                    {
                        isexist = false;
                    }

                }
            }
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


            var visitdetails = _context.SupplyManagementKITSeriesDetail.Where(x => x.SupplyManagementKITSeriesId == record.Id).ToList();
            if (visitdetails.Any())
            {
                foreach (var item1 in visitdetails)
                {
                    item1.DeletedDate = DateTime.Now;
                    item1.DeletedBy = _jwtTokenAccesser.UserId;
                    _context.SupplyManagementKITSeriesDetail.Update(item1);
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
                    ModelState.AddModelError("Message", "Kit should not be deleted once the shipment/receipt has been generated!");
                    return BadRequest(ModelState);
                }

                var visitdetails = _context.SupplyManagementKITSeriesDetail.Where(x => x.SupplyManagementKITSeriesId == record.Id).ToList();
                if (visitdetails.Any())
                {
                    foreach (var item1 in visitdetails)
                    {
                        item1.DeletedDate = DateTime.Now;
                        item1.DeletedBy = _jwtTokenAccesser.UserId;
                        _context.SupplyManagementKITSeriesDetail.Update(item1);
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
        [HttpGet]
        [Route("GetExpiryDate/{id}")]
        public IActionResult GetExpiryDate(int id)
        {

            KitListApprove obj = new KitListApprove();
            var expirydate = _context.ProductVerification.Include(x => x.ProductReceipt)
                .Where(x => x.DeletedDate == null && x.ProductReceipt.Status == ProductVerificationStatus.Approved
                       && x.ProductReceiptId == id).FirstOrDefault();
            if (expirydate != null)
            {
                obj.RetestExpirystr = Convert.ToDateTime(expirydate.RetestExpiryDate).ToString("dd MMM yyyy");

            }
            return Ok(obj);
        }

        [HttpGet]
        [Route("GetLotBatchList/{projectId}/{pharmacyStudyProductTypeId}")]
        public IActionResult GetLotBatchList(int projectId, int pharmacyStudyProductTypeId)
        {
            var data = _context.ProductVerification.Include(x => x.ProductReceipt)
                .Where(x => x.DeletedDate == null && x.ProductReceipt.Status == ProductVerificationStatus.Approved
                && x.ProductReceipt.PharmacyStudyProductTypeId == pharmacyStudyProductTypeId
                && x.ProductReceipt.ProjectId == projectId)
                .GroupBy(s => s.BatchLotNumber)
                .Select(x => new { Id = x.FirstOrDefault().ProductReceiptId, Value = x.FirstOrDefault().BatchLotNumber, Code = x.FirstOrDefault().BatchLotNumber })
                .Distinct()
                .ToList();
            return Ok(data);
        }
    }
}
