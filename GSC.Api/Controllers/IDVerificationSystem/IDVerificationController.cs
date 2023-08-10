using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.IDVerificationSystem;
using GSC.Data.Dto.LabReportManagement;
using GSC.Respository.Configuration;
using GSC.Respository.IDVerificationSystem;
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
    public class IDVerificationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IIDVerificationRepository _iIDVerificationRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public IDVerificationController(
          IUnitOfWork uow, IMapper mapper,
          IJwtTokenAccesser jwtTokenAccesser, IIDVerificationRepository iIDVerificationRepository, IUploadSettingRepository uploadSettingRepository)
        {
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _iIDVerificationRepository = iIDVerificationRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var idVerificationDto = _iIDVerificationRepository.GetIDVerificationList(isDeleted);
            return Ok(idVerificationDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var idVerification = _iIDVerificationRepository.Find(id);
            if (idVerification.DeletedDate == null)
            {
                var idVerificationDto = _mapper.Map<IDVerificationDto>(idVerification);
                var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), idVerificationDto.DocumentPath).Replace('\\', '/');
                idVerificationDto.DocumentPath = path;
                return Ok(idVerificationDto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] IDVerificationDto idVerificationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var result = _iIDVerificationRepository.SaveIDVerificationDocument(idVerificationDto);
            if (result <= 0) throw new Exception("Failed to save document");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _iIDVerificationRepository.Find(id);

            if (record == null)
                return NotFound();

            _iIDVerificationRepository.Delete(record);
            var result = _uow.Save();

            return Ok(result);

        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _iIDVerificationRepository.Find(id);

            if (record == null)
                return NotFound();

            _iIDVerificationRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
