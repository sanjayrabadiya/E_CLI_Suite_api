using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportVariableValueController : BaseController
    {
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly IStudyLevelFormVariableRepository _studyLevelFormVariableRepository;
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectRepository _projectRepository;

        public CtmsMonitoringReportVariableValueController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IStudyLevelFormRepository studyLevelFormRepository,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            IStudyLevelFormVariableRepository studyLevelFormVariableRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IUploadSettingRepository uploadSettingRepository, IProjectRepository projectRepository)
        {
            _studyLevelFormRepository = studyLevelFormRepository;
            _studyLevelFormVariableRepository = studyLevelFormVariableRepository;
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _projectRepository = projectRepository;
        }

        /// Get Form & Variable & VariableValue by manageMonitoringReportId
        /// Created By Swati
        [HttpGet]
        [Route("GetReportFormVariable/{Id}/{CtmsMonitoringReportId}")]
        public IActionResult GetReportFormVariable([FromRoute] int Id, int CtmsMonitoringReportId)
        {
            var designTemplate = _studyLevelFormRepository.GetReportFormVariable(Id);

            return Ok(_ctmsMonitoringReportRepository.GetCtmsMonitoringReportVariableValue(designTemplate, CtmsMonitoringReportId));
        }

        /// Save Variable value 
        /// Created By Swati
        [HttpPut]
        [TransactionRequired]
        [Route("SaveVariableValue")]
        public IActionResult SaveVariableValue([FromBody] CtmsMonitoringReportVariableValueSaveDto ctmsMonitoringReportVariableValueSaveDto)
        {
            _ctmsMonitoringReportVariableValueRepository.SaveVariableValue(ctmsMonitoringReportVariableValueSaveDto);

            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList);
        }

        [HttpGet("GetQueryStatusByReportId/{id}")]
        public IActionResult GetQueryStatusByReportId(int id)
        {
            return Ok(_ctmsMonitoringReportVariableValueRepository.GetQueryStatusByReportId(id));
        }

        [HttpPut("UploadDocument")]
        public IActionResult UploadDocument([FromBody] CtmsMonitoringReportVariableValueDto ctmsMonitoringReportVariableValueSaveDto)
        {
            if (ctmsMonitoringReportVariableValueSaveDto.Id <= 0) return BadRequest();

            var screeningTemplateValue = _ctmsMonitoringReportVariableValueRepository.Find(ctmsMonitoringReportVariableValueSaveDto.Id);

            var documentPath = _uploadSettingRepository.GetDocumentPath();

            if (ctmsMonitoringReportVariableValueSaveDto.FileModel?.Base64?.Length > 0)
            {
                
                
                var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit((int)screeningTemplateValue.CtmsMonitoringReport.CtmsMonitoring.Project.ParentProjectId);
                if (!string.IsNullOrEmpty(validateuploadlimit))
                {
                    ModelState.AddModelError("Message", validateuploadlimit);
                    return BadRequest(ModelState);
                }
                DocumentService.RemoveFile(documentPath, screeningTemplateValue.DocPath);
                screeningTemplateValue.DocPath = DocumentService.SaveUploadDocument(ctmsMonitoringReportVariableValueSaveDto.FileModel,
                      documentPath, _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode((int)screeningTemplateValue.CtmsMonitoringReport.CtmsMonitoring.Project.ParentProjectId), FolderType.Ctms,"");

                screeningTemplateValue.MimeType = ctmsMonitoringReportVariableValueSaveDto.FileModel.Extension;
            }

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            ctmsMonitoringReportVariableValueSaveDto.DocPath = screeningTemplateValue.DocPath;
            ctmsMonitoringReportVariableValueSaveDto.DocFullPath = documentUrl + screeningTemplateValue.DocPath;
            screeningTemplateValue.DocPath = screeningTemplateValue.DocPath;

            _ctmsMonitoringReportVariableValueRepository.Update(screeningTemplateValue);
            _uow.Save();

            return Ok(ctmsMonitoringReportVariableValueSaveDto);
        }
    }
}