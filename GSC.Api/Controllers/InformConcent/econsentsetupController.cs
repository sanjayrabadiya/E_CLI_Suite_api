using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using GSC.Shared.DocumentService;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class econsentsetupController : ControllerBase
    {
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly ILanguageRepository _languageRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IPatientStatusRepository _patientStatusRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEconsentSetupPatientStatusRepository _econsentSetupPatientStatusRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IEconsentSetupRolesRepository _econsentSetupRolesRepository;
        private readonly IGSCContext _context;
        public econsentsetupController(
            IEconsentSetupRepository econsentSetupRepository,
            IUnitOfWork uow,
            IMapper mapper,
            ILanguageRepository languageRepository,
            IDocumentTypeRepository documentTypeRepository,
            IPatientStatusRepository patientStatusRepository,
            IProjectRepository projectRepository,
            IUploadSettingRepository uploadSettingRepository,
            IEmailSenderRespository emailSenderRespository,
            IEconsentSetupPatientStatusRepository econsentSetupPatientStatusRepository,
            IRandomizationRepository randomizationRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IEconsentSetupRolesRepository econsentSetupRolesRepository, IGSCContext context)
        {
            _econsentSetupRepository = econsentSetupRepository;
            _uow = uow;
            _mapper = mapper;
            _languageRepository = languageRepository;
            _documentTypeRepository = documentTypeRepository;
            _patientStatusRepository = patientStatusRepository;
            _projectRepository = projectRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _econsentSetupPatientStatusRepository = econsentSetupPatientStatusRepository;
            _emailSenderRespository = emailSenderRespository;
            _randomizationRepository = randomizationRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _econsentSetupRolesRepository = econsentSetupRolesRepository;
            _context = context;
        }


        [HttpGet]
        [HttpGet("{projectid}/{isDeleted:bool?}")]
        public IActionResult Get(int projectid,bool isDeleted)
        {
            var econsentSetups = _econsentSetupRepository.GetEconsentSetupList(projectid,isDeleted);
            foreach (var item in econsentSetups)
            {
                item.LanguageName = _languageRepository.Find(item.LanguageId).LanguageName;
                item.ProjectName = _projectRepository.Find(item.ProjectId).ProjectCode;
                item.IsDeleted = isDeleted;
            }
            return Ok(econsentSetups);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var econsentSetup = _econsentSetupRepository.FindByInclude(x => x.Id == id, x => x.PatientStatus, x => x.Roles).FirstOrDefault();
            var econsentSetupDto = _mapper.Map<EconsentSetupDto>(econsentSetup);
            if (econsentSetupDto != null && econsentSetupDto.PatientStatus != null)
                econsentSetupDto.PatientStatus = econsentSetupDto.PatientStatus.Where(x => x.DeletedDate == null).ToList();
            if (econsentSetupDto != null && econsentSetupDto.Roles != null)
                econsentSetupDto.Roles = econsentSetupDto.Roles.Where(x => x.DeletedDate == null).ToList();
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

        string validateDocument(string path)
        {
            bool isheaderpresent = false;
            bool isheaderblank = false;
            Stream stream = System.IO.File.OpenRead(path);
            string sfdtText = "";
            EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
            wdocument.Dispose();
            string json = sfdtText;
            stream.Position = 0;
            stream.Close();
            JObject jsonstr = JObject.Parse(json);
            Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
            foreach (var e1 in jsonobj.sections)
            {
                foreach (var e2 in e1.blocks)
                {
                    string headerstring = "";
                    if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                    {
                        foreach (var e3 in e2.inlines)
                        {
                            if (e3.text != null)
                            {
                                headerstring = headerstring + e3.text;
                            }
                        }
                        isheaderpresent = true;
                        if (headerstring == "")
                        {
                            isheaderblank = true;
                            break;
                        }
                    }
                }
            }
            if (isheaderpresent == false)
            {
                return "Please apply 'Heading 1' style in all headings in the document for proper sections.";
            }
            if (isheaderblank == true)
            {
                return "Please check all occurances of 'Heading 1' format in the document, content in 'Heading 1' format must be not empty";
            }
            return "";
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
            if (_econsentSetupRepository.All.Where(x => x.DocumentName == econsentSetupDto.DocumentName && x.LanguageId == econsentSetupDto.LanguageId && x.ProjectId == econsentSetupDto.ProjectId).ToList().Count > 0)
            {
                ModelState.AddModelError("Message", "Please add different document name");
                return BadRequest(ModelState);
            }

            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                econsentSetupDto.DocumentPath = DocumentService.SaveEconsentFile(obj.FileModel, obj.Path, obj.FolderType, obj.RootName);
                string fullpath = Path.Combine(obj.Path, econsentSetupDto.DocumentPath);
                string isvaliddoc = validateDocument(fullpath);
                if (isvaliddoc != "")
                {
                    ModelState.AddModelError("Message", isvaliddoc);
                    if (Directory.Exists(fullpath))
                    {
                        Directory.Delete(fullpath, true);
                    }
                    return BadRequest(ModelState);
                }
            }


            var econsent = _mapper.Map<EconsentSetup>(econsentSetupDto);

            //EconsentSetupPatientStatus econsentSetupPatientStatus = new EconsentSetupPatientStatus();
            //econsentSetupPatientStatus.Id = 0;
            //econsentSetupPatientStatus.EconsentDocumentId = 0;
            //econsentSetupPatientStatus.PatientStatusId = (int)ScreeningPatientStatus.ConsentInProcess;
            //econsent.PatientStatus.Add(econsentSetupPatientStatus);

            _econsentSetupRepository.Add(econsent);
            for (int i = 0; i <= econsent.PatientStatus.Count - 1; i++)
            {
                _econsentSetupPatientStatusRepository.Add(econsent.PatientStatus[i]);
            }

            for (int i = 0; i <= econsent.Roles.Count -1; i++)
            {
                _econsentSetupRolesRepository.Add(econsent.Roles[i]);
            }
            string root = Path.Combine(obj.Path, obj.FolderType.ToString(), obj.RootName);
            if (_uow.Save() <= 0)
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
                throw new Exception($"Creating EConsent File failed on save.");
            }
            var result = (from patients in _context.Randomization.Include(x => x.Project).Where(x => x.Project.ParentProjectId == econsent.ProjectId && x.LanguageId == econsent.LanguageId)
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
            for (var i = 0; i <= result.Count - 1; i++)
            {
                if (result[i].PatientStatusId == ScreeningPatientStatus.ConsentInProcess || result[i].PatientStatusId == ScreeningPatientStatus.ReConsentInProcess || result[i].PatientStatusId == ScreeningPatientStatus.ConsentCompleted)
                {
                    EconsentReviewDetails econsentReviewDetails = new EconsentReviewDetails();
                    econsentReviewDetails.RandomizationId = result[i].Id;
                    econsentReviewDetails.EconsentSetupId = econsent.Id;
                    econsentReviewDetails.IsReviewedByPatient = false;
                    _econsentReviewDetailsRepository.Add(econsentReviewDetails);
                    if (result[i].PatientStatusId == ScreeningPatientStatus.ConsentCompleted)
                    {
                        _randomizationRepository.ChangeStatustoReConsentInProgress(result[i].Id);
                    }
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

            if (_econsentSetupRepository.All.Where(x => x.DocumentName == econsentSetupDto.DocumentName && x.LanguageId == econsentSetupDto.LanguageId && x.ProjectId == econsentSetupDto.ProjectId && x.Id != econsentSetupDto.Id).ToList().Count > 0)
            {
                ModelState.AddModelError("Message", "Please add different document name");
                return BadRequest(ModelState);
            }

            var document = _econsentSetupRepository.Find(econsentSetupDto.Id);
            document.ProjectId = econsentSetupDto.ProjectId;
            document.DocumentName = econsentSetupDto.DocumentName;
            document.LanguageId = econsentSetupDto.LanguageId;
            document.Version = econsentSetupDto.Version;
            document.PatientStatus = econsentSetupDto.PatientStatus;
            document.Roles = econsentSetupDto.Roles;
            document.OriginalFileName = econsentSetupDto.OriginalFileName;

            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
                {
                    document.DocumentPath = DocumentService.SaveEconsentFile(econsentSetupDto.FileModel, _uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent, "EconsentSetup");

                }

            var patientstatusDelete = _econsentSetupPatientStatusRepository.FindBy(x => x.EconsentDocumentId == document.Id && x.PatientStatusId != (int)ScreeningPatientStatus.ConsentInProcess).ToList();//_context.EconsentSetupPatientStatus.Where(x => x.EconsentDocumentId == econsentSetup.Id).ToList();
            foreach (var item in patientstatusDelete)
            {
                _econsentSetupPatientStatusRepository.Remove(item);
            }

            var RolesDelete = _econsentSetupRolesRepository.FindBy(x => x.EconsentDocumentId == document.Id).ToList();
            foreach (var item in RolesDelete)
            {
                _econsentSetupRolesRepository.Remove(item);
            }
            for (int i = 0; i <= document.PatientStatus.Count - 1; i++)
            {
                _econsentSetupPatientStatusRepository.Add(document.PatientStatus[i]);
            }

            for (int i = 0; i <= document.Roles.Count - 1; i++)
            {
                _econsentSetupRolesRepository.Add(document.Roles[i]);
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
