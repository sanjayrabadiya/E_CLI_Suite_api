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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IAttendanceBarcodeGenerateRepository _attendanceBarcodeGenerateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IAppScreenRepository _appScreenRepository;
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;

        public AttendanceBarcodeGenerateController(
            IMapper mapper
            , IVolunteerRepository volunteerRepository
            , IJwtTokenAccesser jwtTokenAccesser
            , IAttendanceBarcodeGenerateRepository attendanceBarcodeGenerateRepository
            , IAttendanceRepository attendanceRepository
            , IAppScreenRepository appScreenRepository
            , IBarcodeConfigRepository barcodeConfigRepository
            , IBarcodeAuditRepository barcodeAuditRepository
            , IUnitOfWork uow)
        {
            _volunteerRepository = volunteerRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _attendanceBarcodeGenerateRepository = attendanceBarcodeGenerateRepository;
            _attendanceRepository = attendanceRepository;
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

            int[] Ids = new int[attendanceBarcodeGenerateDto.Count()];
            int index = 0;

            foreach (var item in attendanceBarcodeGenerateDto)
            {
                item.Id = 0;
                item.BarcodeConfigId = barcodeConfig.Id;
                item.BarcodeString = (Convert.ToInt64(DateTime.Now.ToString("yyMMddHHmmssff")) + item.AttendanceId).ToString();
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
            int[] Ids = new int[attendanceBarcodeGenerateDto.Count()];
            int index = 0;
            foreach (var item in attendanceBarcodeGenerateDto)
            {
                var barcodeData = _attendanceBarcodeGenerateRepository.FindBy(x => x.Id == item.Id).FirstOrDefault();
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
            return Ok(_attendanceBarcodeGenerateRepository.GetReprintBarcodeGenerateData(Ids));
        }

        [HttpPost]
        [Route("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] List<AttendanceBarcodeGenerateDto> attendanceBarcodeGenerateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in attendanceBarcodeGenerateDto)
            {
                var record = _attendanceBarcodeGenerateRepository.FindByInclude(x => x.Id == item.Id).OrderByDescending(d => d.Id);

                if (record == null)
                    return NotFound();

                if (record != null)
                {
                    _attendanceBarcodeGenerateRepository.Delete(record.FirstOrDefault());
                    _uow.Save();
                    _barcodeAuditRepository.Save("AttendanceBarcodeGenerate", AuditAction.Deleted, item.Id);
                }
            }

            //if (_uow.Save() <= 0)
            //    throw new Exception("Barcode failed on delete.");

            return Ok();
        }
    }
}
