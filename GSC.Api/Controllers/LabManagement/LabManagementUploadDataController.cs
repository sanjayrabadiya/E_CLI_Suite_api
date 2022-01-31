using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.LabManagement;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IProjectRepository _projectRepository;

        public LabManagementUploadDataController(
            IUnitOfWork uow, IMapper mapper,
             ILabManagementUploadDataRepository labManagementUploadDataRepository,
             ILabManagementConfigurationRepository configurationRepository,
        IUploadSettingRepository uploadSettingRepository,
        IProjectRepository projectRepository,
        IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _configurationRepository = configurationRepository;
            _labManagementUploadDataRepository = labManagementUploadDataRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectRepository = projectRepository;
        }

        // GET: api/<controller>
        [HttpGet("{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectid, bool isDeleted)
        {
            return Ok(_labManagementUploadDataRepository.GetUploadDataList(projectid, isDeleted));
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] LabManagementUploadDataDto labManagementUploadDataDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            labManagementUploadDataDto.Id = 0;
            string SiteCode = "";

            var isExist = _configurationRepository.All.Where(x => x.ProjectId == labManagementUploadDataDto.ProjectId && x.DeletedDate == null && x.ProjectDesignTemplateId == labManagementUploadDataDto.ProjectDesignTemplateId).FirstOrDefault();
            var LabManagementConfiguration = new LabManagementConfiguration();
            if (isExist != null)
            {
                LabManagementConfiguration = _configurationRepository.All.Where(x => x.ProjectId == labManagementUploadDataDto.ProjectId && x.ProjectDesignTemplateId == labManagementUploadDataDto.ProjectDesignTemplateId && x.DeletedDate == null).FirstOrDefault();
            }
            else
            {
                LabManagementConfiguration = _configurationRepository.All.Where(x => x.ProjectDesignTemplateId == labManagementUploadDataDto.ProjectDesignTemplateId && x.DeletedDate == null).FirstOrDefault();
            }

            labManagementUploadDataDto.LabManagementConfigurationId = LabManagementConfiguration.Id;

            //set file path and extension
            if (labManagementUploadDataDto.FileModel?.Base64?.Length > 0)
            {
                SiteCode = _projectRepository.GetStudyCode(labManagementUploadDataDto.ProjectId);
                labManagementUploadDataDto.PathName = DocumentService.SaveUploadDocument(labManagementUploadDataDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), SiteCode, FolderType.LabManagement, "");
                labManagementUploadDataDto.MimeType = labManagementUploadDataDto.FileModel.Extension;
                labManagementUploadDataDto.FileName = "LabManagementData_" + DateTime.Now.Ticks + "." + labManagementUploadDataDto.FileModel.Extension;
            }

            labManagementUploadDataDto.LabManagementUploadStatus = LabManagementUploadStatus.Pending;
            var labManagementUploadData = _mapper.Map<LabManagementUploadData>(labManagementUploadDataDto);

            //Upload Excel data into database table
            var validate = _labManagementUploadDataRepository.InsertExcelDataIntoDatabaseTable(labManagementUploadData, _projectRepository.GetParentProjectCode(labManagementUploadDataDto.ProjectId));

            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            if (_uow.Save() <= 0) throw new Exception("Creating updaload data failed on save.");
            return Ok(labManagementUploadData.Id);
        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] LabManagementUploadDataDto labManagementUploadDataDto)
        {
            if (labManagementUploadDataDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var labManagementUpload = _labManagementUploadDataRepository.Find(labManagementUploadDataDto.Id);
            labManagementUpload.LabManagementUploadStatus = labManagementUploadDataDto.LabManagementUploadStatus;
            labManagementUpload.AuditReasonId = labManagementUploadDataDto.AuditReasonId;
            labManagementUpload.ReasonOth = labManagementUploadDataDto.ReasonOth;

            if (labManagementUploadDataDto.LabManagementUploadStatus == LabManagementUploadStatus.Approve)
                _labManagementUploadDataRepository.InsertDataIntoDataEntry(labManagementUpload);
            _labManagementUploadDataRepository.Update(labManagementUpload);

            if (_uow.Save() <= 0) throw new Exception("Updating lab management data failed on action.");
            return Ok(labManagementUpload.Id);
        }

    }
}
