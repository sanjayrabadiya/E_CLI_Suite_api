using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.IDVerificationSystem;
using GSC.Data.Dto.LabReportManagement;
using GSC.Data.Entities.IDVerificationSystem;
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
                foreach (var document in idVerificationDto.IDVerificationFiles)
                {
                    var path = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), document.DocumentPath).Replace('\\', '/');
                    document.DocumentPath = path;
                }
                return Ok(idVerificationDto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetDocumentByUser/{userId}")]
        public IActionResult GetDocumentByUser(int userId)
        {
            var idVerificationDto = _iIDVerificationRepository.GetIDVerificationByUser(userId);
            return Ok(idVerificationDto);
        }


        [HttpPost("ChangeDocumentStatus")]
        public IActionResult ChangeDocumentStatus(IDVerificationUpdateDto iDVerification)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            iDVerification.VerifyOrRejectBy = _jwtTokenAccesser.UserId;
            var verification = _mapper.Map<IDVerification>(iDVerification);
            _iIDVerificationRepository.Update(verification);
            var result = _uow.Save();
            if (result > 0)
            {
                return Ok(result);
            }
            else
            {
                return Ok("Failed to save change");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] IDVerificationDto idVerificationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var result = _iIDVerificationRepository.SaveIDVerificationDocument(idVerificationDto);
            if (result <= 0)
            {
                ModelState.AddModelError("Message", "Failed to save document");
                return BadRequest(ModelState);
            }
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
