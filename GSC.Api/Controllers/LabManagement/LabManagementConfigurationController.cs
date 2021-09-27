using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.LabManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.LabManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabManagementConfigurationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ILabManagementConfigurationRepository _configurationRepository;
        private readonly ILabManagementVariableMappingRepository _labManagementVariableMappingRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public LabManagementConfigurationController(
            ILabManagementConfigurationRepository configurationRepository,
            ILabManagementVariableMappingRepository labManagementVariableMappingRepository,
        IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _configurationRepository = configurationRepository;
            _labManagementVariableMappingRepository = labManagementVariableMappingRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_configurationRepository.GetConfigurationList(isDeleted));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var configuration = _configurationRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesignTemplate.ProjectDesignVisit).FirstOrDefault();
            var configurationDto = _mapper.Map<LabManagementConfigurationDto>(configuration);
            return Ok(configurationDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] LabManagementConfigurationDto configurationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            configurationDto.Id = 0;

            //set file path and extension
            if (configurationDto.FileModel?.Base64?.Length > 0)
            {
                configurationDto.PathName = DocumentService.SaveUploadDocument(configurationDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.LabManagement, "");
                configurationDto.MimeType = configurationDto.FileModel.Extension;
                configurationDto.FileName = "ProductReceipt_" + DateTime.Now.Ticks + "." + configurationDto.FileModel.Extension;
            }

            var configuration = _mapper.Map<Data.Entities.LabManagement.LabManagementConfiguration>(configurationDto);
            var validate = _configurationRepository.Duplicate(configuration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _configurationRepository.Add(configuration);

            if (_uow.Save() <= 0) throw new Exception("Creating Configuration failed on save.");
            return Ok(configuration.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LabManagementConfigurationDto configurationDto)
        {
            if (configurationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //added by vipul for if document send empty if they cant want to change docuemnt
            var productRec = _configurationRepository.Find(configurationDto.Id);
            if (configurationDto.FileModel?.Base64?.Length > 0)
            {
                configurationDto.PathName = DocumentService.SaveUploadDocument(configurationDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.LabManagement, "");
                configurationDto.MimeType = configurationDto.FileModel.Extension;
                configurationDto.FileName = "LabManagement_" + DateTime.Now.Ticks + "." + configurationDto.FileModel.Extension;
            }
            else
            {
                configurationDto.PathName = productRec.PathName;
                configurationDto.MimeType = productRec.MimeType;
                configurationDto.FileName = productRec.FileName;
            }

            var configuration = _mapper.Map<Data.Entities.LabManagement.LabManagementConfiguration>(configurationDto);
            var validate = _configurationRepository.Duplicate(configuration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _configurationRepository.AddOrUpdate(configuration);

            if (_uow.Save() <= 0) throw new Exception("Updating Configuration failed on save.");
            return Ok(configuration.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _configurationRepository.Find(id);

            if (record == null)
                return NotFound();

            _configurationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _configurationRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _configurationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _configurationRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetMappingData/{LabManagementConfigurationId}")]
        public IActionResult GetMappingData(int LabManagementConfigurationId)
        {
            return Ok(_configurationRepository.GetMappingData(LabManagementConfigurationId));
        }
    }
}
