using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabReportManagement;
using GSC.Respository.Configuration;
using GSC.Respository.LabReportManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace GSC.Api.Controllers.LabReportManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabReportController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILabReportRepository _labReportRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public LabReportController(
          IUnitOfWork uow, IMapper mapper,
          IJwtTokenAccesser jwtTokenAccesser, ILabReportRepository labReportRepository, IUploadSettingRepository uploadSettingRepository)
        {
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _labReportRepository = labReportRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var labReportDto = _labReportRepository.GetLabReports(isDeleted);
            return Ok(labReportDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var labReport = _labReportRepository.Find(id);
            if (labReport.DeletedDate == null)
            {
                var labReportDto = _mapper.Map<LabReportGridDto>(labReport);
                var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), labReportDto.DocumentPath).Replace('\\', '/');
                labReportDto.DocumentPath = path;
                return Ok(labReportDto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] LabReportDto labReportDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var result = _labReportRepository.SaveLabReportDocument(labReportDto);
            if (result <= 0) throw new Exception("Failed to save lab report");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _labReportRepository.Find(id);

            if (record == null)
                return NotFound();

            _labReportRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _labReportRepository.Find(id);

            if (record == null)
                return NotFound();

            _labReportRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
