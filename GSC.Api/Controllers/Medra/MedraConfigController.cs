using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Respository.Medra;
using GSC.Respository.Configuration;
using GSC.Data.Dto.Medra;
using GSC.Shared.DocumentService;
using GSC.Data.Entities.Medra;
using GSC.Respository.UserMgt;
using System.IO;
using GSC.Shared.JWTAuth;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    public class MedraConfigController : BaseController
    {
        private readonly IMedraConfigRepository _medraConfigRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IMedraVersionRepository _medraVersionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMedraLanguageRepository _languageRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMedraConfigCommonRepository _medraConfigCommonRepository;

        private readonly IMeddraHlgtHltCompRepository _meddraHlgtHltCompRepository;
        private readonly IMeddraHlgtPrefTermRepository _meddraHlgtPrefTermRepository;
        private readonly IMeddraHltPrefCompRepository _meddraHltPrefCompRepository;
        private readonly IMeddraHltPrefTermRepository _meddraHltPrefTermRepository;
        private readonly IMeddraLowLevelTermRepository _meddraLowLevelTermRepository;
        private readonly IMeddraMdHierarchyRepository _meddraMdHierarchyRepository;
        private readonly IMeddraPrefTermRepository _meddraPrefTermRepository;
        private readonly IMeddraSmqContentRepository _meddraSmqContentRepository;
        private readonly IMeddraSmqListRepository _meddraSmqListRepository;
        private readonly IMeddraSocHlgtCompRepository _meddraSocHlgtCompRepository;
        private readonly IMeddraSocIntlOrderRepository _meddraSocIntlOrderRepository;
        private readonly IMeddraSocTermRepository _meddraSocTermRepository;
        private readonly IRoleRepository _securityRoleRepository;

        public MedraConfigController(IMedraConfigRepository medraConfigRepository,
           IDictionaryRepository dictionaryRepository,
           IMedraVersionRepository medraVersionRepository,
           IUserRepository userRepository,
           IMedraLanguageRepository languageRepository,
           IMedraConfigCommonRepository medraConfigCommonRepository,
           IUnitOfWork uow,
           IMapper mapper,
           IUploadSettingRepository uploadSettingRepository,
           IJwtTokenAccesser jwtTokenAccesser,
           IDocumentTypeRepository documentTypeRepository,
           IMeddraHlgtHltCompRepository meddraHlgtHltCompRepository,
        IMeddraHlgtPrefTermRepository meddraHlgtPrefTermRepository,
        IMeddraHltPrefCompRepository meddraHltPrefCompRepository,
        IMeddraHltPrefTermRepository meddraHltPrefTermRepository,
        IMeddraLowLevelTermRepository meddraLowLevelTermRepository,
        IMeddraMdHierarchyRepository meddraMdHierarchyRepository,
        IMeddraPrefTermRepository meddraPrefTermRepository,
        IMeddraSmqContentRepository meddraSmqContentRepository,
        IMeddraSmqListRepository meddraSmqListRepository,
        IMeddraSocHlgtCompRepository meddraSocHlgtCompRepository,
        IMeddraSocIntlOrderRepository meddraSocIntlOrderRepository,
        IMeddraSocTermRepository meddraSocTermRepository,
        IMeddraCodingRepository meddraCodingRepository,
        IRoleRepository securityRoleRepository
          )
        {
            _medraConfigRepository = medraConfigRepository;
            _dictionaryRepository = dictionaryRepository;
            _medraVersionRepository = medraVersionRepository;
            _userRepository = userRepository;
            _languageRepository = languageRepository;
            _medraConfigCommonRepository = medraConfigCommonRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _meddraHlgtHltCompRepository = meddraHlgtHltCompRepository;
            _meddraHlgtPrefTermRepository = meddraHlgtPrefTermRepository;
            _meddraHltPrefCompRepository = meddraHltPrefCompRepository;
            _meddraHltPrefTermRepository = meddraHltPrefTermRepository;
            _meddraLowLevelTermRepository = meddraLowLevelTermRepository;
            _meddraMdHierarchyRepository = meddraMdHierarchyRepository;
            _meddraPrefTermRepository = meddraPrefTermRepository;
            _meddraSmqContentRepository = meddraSmqContentRepository;
            _meddraSmqListRepository = meddraSmqListRepository;
            _meddraSocHlgtCompRepository = meddraSocHlgtCompRepository;
            _meddraSocIntlOrderRepository = meddraSocIntlOrderRepository;
            _meddraSocTermRepository = meddraSocTermRepository;
            _securityRoleRepository = securityRoleRepository;
        }

        [HttpGet]
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var medra = _medraConfigRepository.FindByInclude(x => (x.CompanyId == null
                                                           || x.CompanyId == _jwtTokenAccesser.CompanyId) && isDeleted ? x.DeletedDate != null : x.DeletedDate == null, x => x.MedraVersion
                                                           , x => x.MedraVersion.Dictionary, x => x.Language).OrderByDescending(x => x.Id).ToList();

            var medraDto = _mapper.Map<IEnumerable<MedraConfigDto>>(medra).ToList();
            foreach (var item in medraDto)
            {
                item.UserName = _userRepository.Find(item.CreatedBy).UserName;
                item.Summary = _medraConfigCommonRepository.getSummary(item.Id);
                item.RoleName = _securityRoleRepository.Find(item.CreatedRole).RoleName;
            }
            return Ok(medraDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var medra = _medraConfigRepository.Find(id);
            var medraDto = _mapper.Map<MedraConfigDto>(medra);
            return Ok(medraDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] MedraConfigDto medraDto)
        {
            SaveFileDto obj = new SaveFileDto();
            obj.Path = _uploadSettingRepository.GetDocumentPath();
            obj.FolderType = FolderType.MedraDictionary;
            obj.RootName = "MedAscii";
            obj.FileModel = medraDto.FileModel;

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }
            medraDto.Id = 0;
            var validate = _medraConfigRepository.Duplicate(medraDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            obj.Language = _languageRepository.Find(medraDto.LanguageId).LanguageName;
            obj.Version = _medraVersionRepository.Find(medraDto.MedraVersionId).Version;

            if (medraDto.FileModel?.Base64?.Length > 0)
            {
                medraDto.PathName = DocumentService.SaveMedraFile(obj.FileModel, obj.Path, obj.FolderType, obj.Language, obj.Version, obj.RootName);
                medraDto.MimeType = medraDto.FileModel.Extension;
            }

            var medra = _mapper.Map<MedraConfig>(medraDto);
            medra.IsActive = false;
            medra.CreatedRole = _jwtTokenAccesser.RoleId;
            _medraConfigRepository.Add(medra);
            string root = Path.Combine(obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName) + "\\Unzip";
            if (_uow.Save() <= 0)
            {
                _medraConfigCommonRepository.DeleteDirectory(root);
                ModelState.AddModelError("Message", "Creating Medra Config failed on save.");
                return BadRequest(ModelState);
            }

            obj.MedraId = medra.Id;

            int Soccount = _meddraSocTermRepository.AddSocFileData(obj);
            if (Soccount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int Hlgtcount = _meddraHlgtPrefTermRepository.AddHlgtFileData(obj);
            if (Hlgtcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int Hltcount = _meddraHltPrefTermRepository.AddHltFileData(obj);
            if (Hltcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int Ptcount = _meddraPrefTermRepository.AddPtFileData(obj);
            if (Ptcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int Lltcount = _meddraLowLevelTermRepository.AddLltFileData(obj);
            if (Lltcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int SmqListcount = _meddraSmqListRepository.AddSmqListFileData(obj);
            if (SmqListcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int HlgtHltcount = _meddraHlgtHltCompRepository.AddHlgtHltFileData(obj);
            if (HlgtHltcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int HltPtcount = _meddraHltPrefCompRepository.AddHltPtFileData(obj);
            if (HltPtcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int Mdhiercount = _meddraMdHierarchyRepository.AddMdhierFileData(obj);
            if (Mdhiercount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int SmqContentcount = _meddraSmqContentRepository.AddSmqContentFileData(obj);
            if (SmqContentcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int SocHlgtcount = _meddraSocHlgtCompRepository.AddSocHlgtFileData(obj);
            if (SocHlgtcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            int IntlOrdcount = _meddraSocIntlOrderRepository.AddIntlOrdFileData(obj);
            if (IntlOrdcount != 0)
            {
                ModelState.AddModelError("Message", InsertFileData(root));
                return BadRequest(ModelState);
            }

            _medraConfigCommonRepository.DeleteDirectory(root);
            return Ok(medra.Id);
        }

        private string InsertFileData(string root)
        {
            if (_uow.Save() <= 0)
            {
                _medraConfigCommonRepository.DeleteDirectory(root);
                return $"Creating Medra Config failed on save.";
            }
            return "";
        }

        [HttpPut]
        public IActionResult Put([FromBody] MedraConfigDto medraDto)
        {
            if (medraDto.Id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var document = _medraConfigRepository.Find(medraDto.Id);
            document.MedraVersionId = medraDto.MedraVersionId;
            document.Password = medraDto.Password;
            document.Description = medraDto.Description;
            document.LanguageId = medraDto.LanguageId;
            var language = _languageRepository.Find(medraDto.LanguageId);
            var version = _medraVersionRepository.Find(medraDto.MedraVersionId);

            if (medraDto.FileModel?.Base64?.Length > 0)
            {
                document.PathName = DocumentService.SaveMedraFile(medraDto.FileModel, _uploadSettingRepository.GetDocumentPath(), FolderType.MedraDictionary, language.LanguageName, version.Version, "MedAscii");
                document.MimeType = medraDto.FileModel.Extension;
            }

            var medra = _mapper.Map<MedraConfig>(document);
            _medraConfigRepository.Update(medra);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Medra Config failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(medra.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _medraConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            _medraConfigRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _medraConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            MedraConfigDto obj = new MedraConfigDto();
            obj.Id = record.Id;
            obj.LanguageId = record.LanguageId;
            obj.MedraVersionId = record.MedraVersionId;

            var validate = _medraConfigRepository.Duplicate(obj);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _medraConfigRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDictionaryDropDown")]
        public IActionResult GetDictionaryDropDown()
        {
            return Ok(_dictionaryRepository.GetDictionaryDropDown());
        }

        [HttpGet]
        [Route("GetMedraVesrionByDictionaryDropDown/{DictionaryId}")]
        public IActionResult GetMedraVesrionByDictionaryDropDown(int DictionaryId)
        {
            return Ok(_medraConfigRepository.GetMedraVesrionByDictionaryDropDown(DictionaryId));
        }

        [HttpGet]
        [Route("GetMedraLanguageVersionDropDown")]
        public IActionResult GetMedraLanguageVersionDropDown()
        {
            return Ok(_medraConfigRepository.GetMedraLanguageVersionDropDown());
        }

        [HttpGet]
        [Route("GetDetailByMeddraConfigId/{MeddraConfigId}")]
        public IActionResult GetDetailByMeddraConfigId(int MeddraConfigId)
        {
            var details = _medraConfigRepository.GetDetailByMeddraConfigId(MeddraConfigId);
            return Ok(details);
        }

        [HttpPut]
        [Route("UpdateDictionaryVersion")]
        public IActionResult UpdateDictionaryVersion([FromBody] int id)
        {
            var activeResult = _medraConfigRepository.GetActiveDictionaryId();
            if (activeResult != null)
            {
                var record = _medraConfigRepository.Find(activeResult.Id);
                if (record != null)
                {
                    record.ModifiedBy = _jwtTokenAccesser.UserId;
                    record.ModifiedDate = _jwtTokenAccesser.GetClientDate();
                    record.IsActive = false;
                    record.CreatedRole = _jwtTokenAccesser.RoleId;
                    var medra = _mapper.Map<MedraConfig>(record);
                    _medraConfigRepository.Update(medra);

                    if (_uow.Save() <= 0)
                    {
                        ModelState.AddModelError("Message", "Active Medra dictionary failed.");
                        return BadRequest(ModelState);
                    }
                }
            }

            var active = _medraConfigRepository.Find(id);
            if (active != null)
            {
                active.ModifiedBy = _jwtTokenAccesser.UserId;
                active.ModifiedDate = _jwtTokenAccesser.GetClientDate();
                active.IsActive = true;
                var medra = _mapper.Map<MedraConfig>(active);
                _medraConfigRepository.Update(medra);

                if (_uow.Save() <= 0)
                {
                    ModelState.AddModelError("Message", "Active Medra dictionary failed.");
                    return BadRequest(ModelState);
                }
            }
            return Ok(active?.Id);
        }
    }
}
