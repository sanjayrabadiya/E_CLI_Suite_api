using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.LabManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.LabManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabManagementUploadDataController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILabManagementUploadDataRepository _labManagementUploadDataRepository;
        private readonly ILabManagementConfigurationRepository _configurationRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public LabManagementUploadDataController(
            IUnitOfWork uow, IMapper mapper,
             ILabManagementUploadDataRepository labManagementUploadDataRepository,
             ILabManagementConfigurationRepository configurationRepository,
        IUploadSettingRepository uploadSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _configurationRepository = configurationRepository;
            _labManagementUploadDataRepository = labManagementUploadDataRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_labManagementUploadDataRepository.GetUploadDataList(isDeleted));
        }

        [HttpPost]
        public IActionResult Post([FromBody] LabManagementUploadDataDto labManagementUploadDataDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            labManagementUploadDataDto.Id = 0;

            labManagementUploadDataDto.LabManagementConfigurationId = _configurationRepository.All.Where(x => x.ProjectDesignTemplateId == labManagementUploadDataDto.ProjectDesignTemplateId).FirstOrDefault().Id;

            //set file path and extension
            if (labManagementUploadDataDto.FileModel?.Base64?.Length > 0)
            {
                labManagementUploadDataDto.PathName = DocumentService.SaveUploadDocument(labManagementUploadDataDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.LabManagement, "");
                labManagementUploadDataDto.MimeType = labManagementUploadDataDto.FileModel.Extension;
                labManagementUploadDataDto.FileName = "LabManagementData_" + DateTime.Now.Ticks + "." + labManagementUploadDataDto.FileModel.Extension;
            }

            var labManagementUploadData = _mapper.Map<LabManagementUploadData>(labManagementUploadDataDto);
            //var validate = _labManagementUploadDataRepository.Duplicate(labManagementUploadData);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}

            _labManagementUploadDataRepository.Add(labManagementUploadData);

            if (_uow.Save() <= 0) throw new Exception("Creating updaload data failed on save.");
            return Ok(labManagementUploadData.Id);
        }

    }
}
