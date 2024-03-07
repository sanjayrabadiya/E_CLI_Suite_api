using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Respository.Attendance;
using GSC.Respository.Barcode;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceBarcodeGenerateController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IAttendanceBarcodeGenerateRepository _attendanceBarcodeGenerateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IAppScreenRepository _appScreenRepository;
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;

        public AttendanceBarcodeGenerateController(
            IMapper mapper
            , IAttendanceBarcodeGenerateRepository attendanceBarcodeGenerateRepository
            , IAppScreenRepository appScreenRepository
            , IBarcodeConfigRepository barcodeConfigRepository
            , IBarcodeAuditRepository barcodeAuditRepository
            , IUnitOfWork uow)
        {

            _mapper = mapper;
            _attendanceBarcodeGenerateRepository = attendanceBarcodeGenerateRepository;
            _appScreenRepository = appScreenRepository;
            _barcodeConfigRepository = barcodeConfigRepository;
            _barcodeAuditRepository = barcodeAuditRepository;
            _uow = uow;
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<AttendanceBarcodeGenerateDto> attendanceBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var Page = _appScreenRepository.FindBy(x => x.TableName == "Attendance" && x.DeletedBy == null).FirstOrDefault();
            if (Page == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            var barcodeConfig = _barcodeConfigRepository.FindBy(x => x.PageId == Page.Id && x.DeletedBy == null).FirstOrDefault();
            if (barcodeConfig == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }

            int[] Ids = new int[attendanceBarcodeGenerateDto.Count];
            int index = 0;

            foreach (var item in attendanceBarcodeGenerateDto)
            {
                item.Id = 0;
                item.BarcodeConfigId = barcodeConfig.Id;
                item.BarcodeString = _attendanceBarcodeGenerateRepository.GetBarcodeString(item.AttendanceId);
                var attendanceBarcode = _mapper.Map<AttendanceBarcodeGenerate>(item);
                _attendanceBarcodeGenerateRepository.Add(attendanceBarcode);
                _uow.Save();
                Ids[index] = attendanceBarcode.Id;
                index++;
                _barcodeAuditRepository.Save("AttendanceBarcodeGenerate", AuditAction.Inserted, attendanceBarcode.Id);
            }

            var generatedData = _attendanceBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids);

            return Ok(generatedData);
        }

        [HttpGet]
        [Route("ViewBarcode/{attendanceId}")]
        public IActionResult ViewBarcode(int attendanceId)
        {
            var Page = _appScreenRepository.FindBy(x => x.TableName == "Attendance" && x.DeletedBy == null).FirstOrDefault();
            if (Page == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            var barcodeConfig = _barcodeConfigRepository.FindBy(x => x.PageId == Page.Id && x.DeletedBy == null).FirstOrDefault();
            if (barcodeConfig == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            return Ok(_attendanceBarcodeGenerateRepository.GetBarcodeDetail(attendanceId));
        }

        [HttpPost]
        [Route("RePrintBarcode")]
        public IActionResult RePrintBarcode([FromBody] List<AttendanceBarcodeGenerateDto> attendanceBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            int[] Ids = new int[attendanceBarcodeGenerateDto.Count];
            int index = 0;
            foreach (var item in attendanceBarcodeGenerateDto)
            {
                var barcodeData = _attendanceBarcodeGenerateRepository.FindBy(x => x.Id == item.Id).FirstOrDefault();
                if (barcodeData != null)
                {
                    item.BarcodeConfigId = barcodeData.BarcodeConfigId;
                    item.BarcodeString = barcodeData.BarcodeString;
                    item.AttendanceId = barcodeData.AttendanceId;
                    var attendanceBarcode = _mapper.Map<AttendanceBarcodeGenerate>(item);
                    Ids[index] = item.Id;
                    index++;
                    _attendanceBarcodeGenerateRepository.Update(attendanceBarcode);
                    _uow.Save();
                    _barcodeAuditRepository.Save("AttendanceBarcodeGenerate", AuditAction.RePrint, attendanceBarcode.Id);
                }
            }
            return Ok(_attendanceBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids));
        }

        [HttpPost]
        [Route("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] List<AttendanceBarcodeGenerateDto> attendanceBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            if (attendanceBarcodeGenerateDto != null && attendanceBarcodeGenerateDto.Count > 0)
            {
                foreach (var item in attendanceBarcodeGenerateDto.Select(s => s.Id))
                {
                    var record = _attendanceBarcodeGenerateRepository.FindByInclude(x => x.Id == item).OrderByDescending(d => d.Id).FirstOrDefault();

                    if (record == null)
                        return NotFound();
                    _attendanceBarcodeGenerateRepository.Delete(record);
                    _uow.Save();
                    _barcodeAuditRepository.Save("AttendanceBarcodeGenerate", AuditAction.Deleted, item);

                }
            }

            return Ok();
        }
    }
}
