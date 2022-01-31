using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.LabManagement;
using GSC.Respository.Master;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace GSC.Api.Controllers.LabManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabManagementConfigurationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ILabManagementConfigurationRepository _configurationRepository;
        private readonly ILabManagementUploadDataRepository _labManagementUploadDataRepository;
        private readonly ILabManagementSendEmailUserRepository _labManagementSendEmailUserRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectRepository _projectRepository;
        private readonly IGSCContext _context;

        public LabManagementConfigurationController(
            ILabManagementConfigurationRepository configurationRepository,
            ILabManagementUploadDataRepository labManagementUploadDataRepository,
            ILabManagementSendEmailUserRepository labManagementSendEmailUserRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectRepository projectRepository,
            IGSCContext context,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _configurationRepository = configurationRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectRepository = projectRepository;
            _labManagementUploadDataRepository = labManagementUploadDataRepository;
            _labManagementSendEmailUserRepository = labManagementSendEmailUserRepository;
            _context = context;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectid, bool isDeleted)
        {
            return Ok(_configurationRepository.GetConfigurationList(projectid, isDeleted));
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var configuration = _configurationRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesignTemplate.ProjectDesignVisit, x => x.LabManagementSendEmailUser.Where(t => t.DeletedDate == null)).FirstOrDefault();
            var configurationDto = _mapper.Map<LabManagementConfigurationDto>(configuration);
            return Ok(configurationDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] LabManagementConfigurationDto configurationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            configurationDto.Id = 0;

            var checkConfiguration = _configurationRepository.All.Where(x => x.ProjectDesignTemplateId == configurationDto.ProjectDesignTemplateId && x.DeletedDate == null).ToList();

            if (checkConfiguration.All(x => x.ProjectId == null))
            {
                if (checkConfiguration.Count() != 0)
                {
                    ModelState.AddModelError("Message", "Form already configured!");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                if (checkConfiguration.Any(x => x.ProjectId == null))
                {
                    if (checkConfiguration.Count() != 0)
                    {
                        ModelState.AddModelError("Message", "Form already configured study wise!");
                        return BadRequest(ModelState);
                    }
                }
                if (checkConfiguration.Where(x => x.ProjectId == configurationDto.ProjectId).Count() != 0)
                {
                    ModelState.AddModelError("Message", "Form already configured!");
                    return BadRequest(ModelState);
                }
            }

            //set file path and extension
            if (configurationDto.FileModel?.Base64?.Length > 0)
            {
                configurationDto.PathName = DocumentService.SaveUploadDocument(configurationDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(configurationDto.ParentProjectId), FolderType.LabManagement, "");
                configurationDto.MimeType = configurationDto.FileModel.Extension;
                configurationDto.FileName = "LabManagement_" + DateTime.Now.Ticks + "." + configurationDto.FileModel.Extension;
            }

            var configuration = _mapper.Map<LabManagementConfiguration>(configurationDto);
            var validate = _configurationRepository.Duplicate(configuration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _configurationRepository.Add(configuration);

            if (_uow.Save() <= 0) throw new Exception("Creating Configuration failed on save.");

            if (configurationDto.UserIds.Length != 0)
            {
                configurationDto.UserIds.ToList().ForEach(x =>
                {
                    LabManagementSendEmailUser obj = new LabManagementSendEmailUser();
                    obj.LabManagementConfigurationId = configuration.Id;
                    obj.UserId = (int)x;
                    _context.LabManagementSendEmailUser.Add(obj);
                });
            }
            _uow.Save();
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
                configurationDto.PathName = DocumentService.SaveUploadDocument(configurationDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(configurationDto.ParentProjectId), FolderType.LabManagement, "");
                configurationDto.MimeType = configurationDto.FileModel.Extension;
                configurationDto.FileName = "LabManagement_" + DateTime.Now.Ticks + "." + configurationDto.FileModel.Extension;
            }
            else
            {
                configurationDto.PathName = productRec.PathName;
                configurationDto.MimeType = productRec.MimeType;
                configurationDto.FileName = productRec.FileName;
            }

            var configuration = _mapper.Map<LabManagementConfiguration>(configurationDto);
            var validate = _configurationRepository.Duplicate(configuration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _configurationRepository.Update(configuration);

            if (_uow.Save() <= 0) throw new Exception("Updating Configuration failed on save.");
            return Ok(configuration.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _configurationRepository.Find(id);

            if (record == null)
                return NotFound();

            var Exists = _labManagementUploadDataRepository.CheckDataIsUploadForDeleteConfiguration(id);
            if (!string.IsNullOrEmpty(Exists))
            {
                ModelState.AddModelError("Message", Exists);
                return BadRequest(ModelState);
            }


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

            return Ok(_configurationRepository.GetMappingData<object>(LabManagementConfigurationId));
        }

        // Add by vipul for only bind that project which map in lab management configuration
        [HttpGet]
        [Route("GetParentProjectDropDownForUploadLabData")]
        public IActionResult GetParentProjectDropDownForUploadLabData()
        {
            return Ok(_configurationRepository.GetParentProjectDropDownForUploadLabData());
        }

        // Add by vipul for only bind that visit which map in lab management configuration
        [HttpGet]
        [Route("GetVisitDropDownForUploadLabData/{projectDesignPeriodId}")]
        public IActionResult GetVisitDropDownForUploadLabData(int projectDesignPeriodId)
        {
            return Ok(_configurationRepository.GetVisitDropDownForUploadLabData(projectDesignPeriodId));
        }

        // Add by vipul for only bind that template which map in lab management configuration
        [HttpGet]
        [Route("GetTemplateDropDownForUploadLabData/{projectDesignVisitId}")]
        public IActionResult GetTemplateDropDownForUploadLabData(int projectDesignVisitId)
        {
            return Ok(_configurationRepository.GetTemplateDropDownForUploadLabData(projectDesignVisitId));
        }

        // Get user for role profile
        [HttpGet]
        [Route("EmailUsers/{ProjectId}")]
        public IActionResult EmailUsers(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            return Ok(_configurationRepository.EmailUsers(ProjectId));
        }

        [HttpPut]
        [Route("UpdateLabManagementConfiguration")]
        public IActionResult UpdateLabManagementConfiguration([FromBody] LabManagementConfigurationEdit configurationDto)
        {
            //var configure = _configurationRepository.Find(configurationDto.Id);
            //configure.SecurityRoleId = configurationDto.SecurityRoleId;
            //_configurationRepository.Update(configure);

            // get role by Lab Management Configuration id
            var data = _labManagementSendEmailUserRepository.FindBy(x => x.LabManagementConfigurationId == configurationDto.Id && x.DeletedDate == null).ToList();

            foreach (var item in configurationDto.UserIds)
            {
                var role = data.Where(t => t.UserId == item).FirstOrDefault();
                // add role if new select in dropdown
                if (role == null)
                {
                    LabManagementSendEmailUser obj = new LabManagementSendEmailUser();
                    obj.LabManagementConfigurationId = configurationDto.Id;
                    obj.UserId = (int)item;
                    _labManagementSendEmailUserRepository.Add(obj);
                }
            }

            var Exists = data.Where(x => !configurationDto.UserIds.Contains(x.UserId)).ToList();
            if (Exists.Count != 0)
                foreach (var item in Exists)
                    _labManagementSendEmailUserRepository.Delete(item);

            if (_uow.Save() <= 0) throw new Exception("Updating Configuration failed on save.");
            return Ok();
        }

        [HttpGet]
        [Route("GetFilePathByProjectDesignTemplateId/{projectDesignTemplateId}/{projectId}")]
        public IActionResult GetFilePathByProjectDesignTemplateId(int projectDesignTemplateId, int projectId)
        {
            if (projectDesignTemplateId <= 0) return BadRequest();

            var isExist = _configurationRepository.All.Where(x => x.ProjectId == projectId && x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null).FirstOrDefault();
            var configuration = new LabManagementConfiguration();
            if (isExist != null)
            {
                configuration = _configurationRepository.All.Where(x => x.ProjectId == projectId && x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null).FirstOrDefault();
            }
            else
            {
                configuration = _configurationRepository.All.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null).FirstOrDefault();
            }


            //var configuration = _configurationRepository.FindByInclude(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.DeletedDate == null).FirstOrDefault();
            var configurationDto = _mapper.Map<LabManagementConfigurationDto>(configuration);
            configurationDto.FullPath = Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), configurationDto.PathName);
            return Ok(configurationDto);
        }
    }
}
