using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.InformConcent;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class econsentsetupController : BaseController
    {
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILanguageRepository _languageRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IPatientStatusRepository _patientStatusRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public econsentsetupController(
            IEconsentSetupRepository econsentSetupRepository,
            IUnitOfWork uow,
            IMapper mapper,
            ILanguageRepository languageRepository,
            IDocumentTypeRepository documentTypeRepository,
            IPatientStatusRepository patientStatusRepository,
            IProjectRepository projectRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _econsentSetupRepository = econsentSetupRepository;
            _uow = uow;
            _mapper = mapper;
            _languageRepository = languageRepository;
            _documentTypeRepository = documentTypeRepository;
            _patientStatusRepository = patientStatusRepository;
            _projectRepository = projectRepository;
            _uploadSettingRepository = uploadSettingRepository;
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
                item.ProjectName = _projectRepository.Find(item.ProjectId).ProjectName;
                item.DocumentTypeName = _documentTypeRepository.Find(item.DocumentTypeId).TypeName;
                item.PatientStatusName = _patientStatusRepository.Find(item.PatientStatusId).StatusName;
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
            var econsentSetup = _econsentSetupRepository.Find(id);
            var econsentSetupDto = _mapper.Map<EconsentSetupDto>(econsentSetup);
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
            var validate = _econsentSetupRepository.Duplicate(econsentSetupDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            obj.Language = _languageRepository.Find(econsentSetupDto.LanguageId).LanguageName;
            obj.Version = econsentSetupDto.Version;

            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                econsentSetupDto.DocumentPath = DocumentService.SaveEconsentFile(obj.FileModel, obj.Path, obj.FolderType, obj.Language, obj.Version, obj.RootName);
            }

            var econsent = _mapper.Map<EconsentSetup>(econsentSetupDto);
            
            _econsentSetupRepository.Add(econsent);
            string root = Path.Combine(obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName);
            if (_uow.Save() <= 0)
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
                throw new Exception($"Creating EConsent File failed on save.");
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
            document.PatientStatusId = econsentSetupDto.PatientStatusId;

            var language = _languageRepository.Find(econsentSetupDto.LanguageId);
            var version = econsentSetupDto.Version;

            if (document.DocumentPath == null || document.DocumentPath == "")
            {
                if (econsentSetupDto.FileModel?.Base64?.Length > 0)
                {
                    document.DocumentPath = DocumentService.SaveEconsentFile(econsentSetupDto.FileModel, _uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent, language.LanguageName, econsentSetupDto.Version, "MedAscii");

                }
            }
            

            var econsent = _mapper.Map<EconsentSetup>(document);
            _econsentSetupRepository.Update(econsent);

            if (_uow.Save() <= 0)
            {
                throw new Exception($"Updating Econsent file failed on save.");
            }
            return Ok(econsent.Id);
        }
    }
}
