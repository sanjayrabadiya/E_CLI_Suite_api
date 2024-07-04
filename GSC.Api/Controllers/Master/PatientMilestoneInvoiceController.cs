using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class PatientMilestoneInvoiceController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPatientMilestoneInvoiceRepository _patientMilestoneInvoiceRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public PatientMilestoneInvoiceController(IPatientMilestoneInvoiceRepository patientMilestoneInvoiceRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository)
        {
            _patientMilestoneInvoiceRepository = patientMilestoneInvoiceRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uploadSettingRepository = uploadSettingRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var patientMilestoneInvoices = _patientMilestoneInvoiceRepository.GetPatientMilestoneInvoiceList(isDeleted);
            return Ok(patientMilestoneInvoices);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var patientMilestoneInvoice = _patientMilestoneInvoiceRepository.Find(id);
            var patientMilestoneInvoiceDto = _mapper.Map<PatientMilestoneInvoiceDto>(patientMilestoneInvoice);
            return Ok(patientMilestoneInvoiceDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PatientMilestoneInvoiceDto patientMilestoneInvoiceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            patientMilestoneInvoiceDto.Id = 0;
            var patientMilestoneInvoice = _mapper.Map<PatientMilestoneInvoice>(patientMilestoneInvoiceDto);
            var validate = _patientMilestoneInvoiceRepository.Duplicate(patientMilestoneInvoice);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _patientMilestoneInvoiceRepository.Add(patientMilestoneInvoice);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Patient Milestone Invoice failed on save.");
                return BadRequest(ModelState);
            }

            if (patientMilestoneInvoiceDto.File?.Base64?.Length > 0 && patientMilestoneInvoiceDto.File?.Base64 != null)
            {
                patientMilestoneInvoice.FilePath = DocumentService.SaveUploadDocument(patientMilestoneInvoiceDto.File, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "PatientMilestoneInvoice");
            }

            _uow.Save();
            return Ok(patientMilestoneInvoice.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PatientMilestoneInvoiceDto patientMilestoneInvoiceDto)
        {
            if (patientMilestoneInvoiceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var patientMilestoneInvoice = _mapper.Map<PatientMilestoneInvoice>(patientMilestoneInvoiceDto);
            var validate = _patientMilestoneInvoiceRepository.Duplicate(patientMilestoneInvoice);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            if (patientMilestoneInvoiceDto.File?.Base64?.Length > 0 && patientMilestoneInvoiceDto.File?.Base64 != null)
            {
                patientMilestoneInvoice.FilePath = DocumentService.SaveUploadDocument(patientMilestoneInvoiceDto.File, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "PatientMilestoneInvoice");
            }

            _patientMilestoneInvoiceRepository.AddOrUpdate(patientMilestoneInvoice);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Patient Milestone Invoice failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(patientMilestoneInvoice.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _patientMilestoneInvoiceRepository.Find(id);

            if (record == null)
                return NotFound();

            _patientMilestoneInvoiceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _patientMilestoneInvoiceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _patientMilestoneInvoiceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _patientMilestoneInvoiceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetPatientMilestoneInvoiceById/{milestoneId}")]
        public ActionResult GetPatientMilestoneInvoiceById(int milestoneId)
        {
            var record = _patientMilestoneInvoiceRepository.GetPatientMilestoneInvoiceById(milestoneId);
            return Ok(record);
        }

        [HttpGet]
        [Route("DownloadDocument/{id}")]
        public IActionResult DownloadDocument(int id)
        {
            var file = _patientMilestoneInvoiceRepository.Find(id);
            var filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), file.FilePath);
            if (System.IO.File.Exists(filepath))
            {
                return File(System.IO.File.OpenRead(filepath), ObjectExtensions.GetMIMEType(file.FileName), file.FileName);
            }
            return NotFound();
        }
    }
}
