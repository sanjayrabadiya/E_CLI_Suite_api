using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class econsentsetupController : ControllerBase
    {
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IMapper _mapper;
        private readonly ILanguageRepository _languageRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IPatientStatusRepository _patientStatusRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEconsentSetupPatientStatusRepository _econsentSetupPatientStatusRepository;
        private readonly GscContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;

        public econsentsetupController(
            IEconsentSetupRepository econsentSetupRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            ILanguageRepository languageRepository,
            IDocumentTypeRepository documentTypeRepository,
            IPatientStatusRepository patientStatusRepository,
            IProjectRepository projectRepository,
            IUploadSettingRepository uploadSettingRepository,
            IEmailSenderRespository emailSenderRespository,
            IEconsentSetupPatientStatusRepository econsentSetupPatientStatusRepository,
            IRandomizationRepository randomizationRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository)
        {
            _econsentSetupRepository = econsentSetupRepository;
            _uow = uow;
            _mapper = mapper;
            _context = uow.Context;
            _languageRepository = languageRepository;
            _documentTypeRepository = documentTypeRepository;
            _patientStatusRepository = patientStatusRepository;
            _projectRepository = projectRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _econsentSetupPatientStatusRepository = econsentSetupPatientStatusRepository;
            _emailSenderRespository = emailSenderRespository;
            _randomizationRepository = randomizationRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
        }


        [HttpGet]
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var econsentSetups = _econsentSetupRepository.FindByInclude(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).OrderByDescending(x => x.Id).ToList();

            var econsentSetupsdto = _mapper.Map<IEnumerable<EconsentSetupDto>>(econsentSetups).ToList();
            foreach (var item in econsentSetupsdto)
            {
                item.LanguageName = _languageRepository.Find(item.LanguageId).LanguageName;
                item.ProjectName = _projectRepository.Find(item.ProjectId).ProjectCode;
                item.DocumentTypeName = _documentTypeRepository.Find(item.DocumentTypeId).TypeName;
                //item.PatientStatusName = _patientStatusRepository.Find(item.PatientStatusId).StatusName;
                item.IsDeleted = isDeleted;
            }
            return Ok(econsentSetupsdto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var econsentSetup = _econsentSetupRepository.FindByInclude(x => x.Id == id, x => x.PatientStatus).FirstOrDefault();
            var econsentSetupDto = _mapper.Map<EconsentSetupDto>(econsentSetup);
            if (econsentSetupDto != null && econsentSetupDto.PatientStatus != null)
                econsentSetupDto.PatientStatus = econsentSetupDto.PatientStatus.Where(x => x.DeletedDate == null).ToList();
            return Ok(econsentSetupDto);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _econsentSetupRepository.Find(id);

            if (record == null)
                return NotFound();

            _econsentSetupRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _econsentSetupRepository.Find(id);

            if (record == null)
                return NotFound();


            EconsentSetupDto econsentSetupDto = new EconsentSetupDto();
            econsentSetupDto.Id = record.Id;
            econsentSetupDto.LanguageId = record.LanguageId;
            econsentSetupDto.Version = record.Version;
            econsentSetupDto.PatientStatus = record.PatientStatus;

            var validate = _econsentSetupRepository.Duplicate(econsentSetupDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _econsentSetupRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpPost]
        public IActionResult Post([FromBody] EconsentSetupDto econsentSetupDto)
        {
            Data.Dto.InformConcent.SaveFileDto obj = new Data.Dto.InformConcent.SaveFileDto();
            obj.Path = _uploadSettingRepository.GetDocumentPath();
            obj.FolderType = FolderType.InformConcent;
            obj.RootName = "EconsentSetup";
            obj.FileModel = econsentSetupDto.FileModel;

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            econsentSetupDto.Id = 0;

            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                econsentSetupDto.DocumentPath = DocumentService.SaveEconsentFile(obj.FileModel, obj.Path, obj.FolderType, obj.RootName);
            }

            var econsent = _mapper.Map<EconsentSetup>(econsentSetupDto);

            EconsentSetupPatientStatus econsentSetupPatientStatus = new EconsentSetupPatientStatus();
            econsentSetupPatientStatus.Id = 0;
            econsentSetupPatientStatus.EconsentDocumentId = 0;
            econsentSetupPatientStatus.PatientStatusId = (int)ScreeningPatientStatus.ConsentInProcess;
            econsent.PatientStatus.Add(econsentSetupPatientStatus);

            _econsentSetupRepository.Add(econsent);
            string root = Path.Combine(obj.Path, obj.FolderType.ToString(), obj.RootName);
            if (_uow.Save() <= 0)
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
                throw new Exception($"Creating EConsent File failed on save.");
            }
            var result = (from patients in _context.Randomization.Where(x => x.ProjectId == econsent.ProjectId && x.LanguageId == econsent.LanguageId)
                          join status in _context.EconsentSetupPatientStatus.Where(x => x.EconsentDocumentId == econsent.Id) on (int)patients.PatientStatusId equals status.PatientStatusId
                          select new Randomization
                          {
                              Id = patients.Id,
                              FirstName = patients.FirstName,
                              MiddleName = patients.MiddleName,
                              LastName = patients.LastName,
                              Initial = patients.Initial,
                              ScreeningNumber = patients.ScreeningNumber,
                              Email = patients.Email,
                              PatientStatusId = patients.PatientStatusId
                          }).ToList();
            string projectcode = _projectRepository.Find(econsent.ProjectId).ProjectCode;
            for (var i = 0; i < result.Count; i++)
            {
                if (result[i].PatientStatusId == ScreeningPatientStatus.ConsentCompleted || result[i].PatientStatusId == ScreeningPatientStatus.OnTrial)
                {
                    EconsentReviewDetails econsentReviewDetails = new EconsentReviewDetails();
                    econsentReviewDetails.AttendanceId = result[i].Id;
                    econsentReviewDetails.EconsentDocumentId = econsent.Id;
                    econsentReviewDetails.IsReviewedByPatient = false;
                    _econsentReviewDetailsRepository.Add(econsentReviewDetails);
                    _randomizationRepository.ChangeStatustoReConsentInProgress(result[i].Id);
                }
                if (result[i].Email != "")
                {
                    string patientName = "";
                    if (result[i].ScreeningNumber != "")
                    {
                        patientName = result[i].Initial + " " + result[i].ScreeningNumber;
                    } else
                    {
                        patientName = result[i].FirstName + " " + result[i].MiddleName + " " + result[i].LastName;
                    }
                    _emailSenderRespository.SendEmailOfEconsentDocumentuploaded(result[i].Email, patientName, econsent.DocumentName, projectcode);
                }
            }
            
            return Ok(econsent.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] EconsentSetupDto econsentSetupDto)
        {
            if (econsentSetupDto.Id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var document = _econsentSetupRepository.Find(econsentSetupDto.Id);
            document.ProjectId = econsentSetupDto.ProjectId;
            document.DocumentTypeId = econsentSetupDto.DocumentTypeId;
            document.DocumentName = econsentSetupDto.DocumentName;
            document.LanguageId = econsentSetupDto.LanguageId;
            document.Version = econsentSetupDto.Version;
            document.PatientStatus = econsentSetupDto.PatientStatus;
            //document.PatientStatusId = econsentSetupDto.PatientStatusId;

                if (econsentSetupDto.FileModel?.Base64?.Length > 0)
                {
                    document.DocumentPath = DocumentService.SaveEconsentFile(econsentSetupDto.FileModel, _uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent, "EconsentSetup");

                }

            var patientstatusDelete = _econsentSetupPatientStatusRepository.FindBy(x => x.EconsentDocumentId == document.Id && x.PatientStatusId != (int)ScreeningPatientStatus.ConsentInProcess).ToList();//_context.EconsentSetupPatientStatus.Where(x => x.EconsentDocumentId == econsentSetup.Id).ToList();
            foreach (var item in patientstatusDelete)
            {
                _econsentSetupPatientStatusRepository.Remove(item);
            }

            _econsentSetupRepository.Update(document);

            if (_uow.Save() <= 0)
            {
                throw new Exception($"Updating Econsent file failed on save.");
            }
            return Ok(document.Id);
        }

        [HttpGet]
        [Route("GetEconsentDocumentDropDown/{projectId}")]
        public IActionResult GetEconsentDocumentDropDown(int projectId)
        {
            return Ok(_econsentSetupRepository.GetEconsentDocumentDropDown(projectId));
        }

        [HttpGet]
        [Route("GetPatientStatusDropDown")]
        public IActionResult GetPatientStatusDropDown()
        {
            return Ok(_econsentSetupRepository.GetPatientStatusDropDown());
        }
    }
}
