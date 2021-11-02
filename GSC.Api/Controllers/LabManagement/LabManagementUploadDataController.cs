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
        private readonly ILabManagementUploadExcelDataRepository _labManagementUploadExcelDataRepository;
        //private readonly ILabManagementVariableMappingRepository _labManagementVariableMappingRepository;
        //private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        //private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        //private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        //private readonly IScreeningVisitRepository _screeningVisitRepository;
        //private readonly IRandomizationRepository _randomizationRepository;
        //private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        //private readonly IAppSettingRepository _appSettingRepository;
        private readonly IProjectRepository _projectRepository;

        public LabManagementUploadDataController(
            IUnitOfWork uow, IMapper mapper,
             ILabManagementUploadDataRepository labManagementUploadDataRepository,
             ILabManagementConfigurationRepository configurationRepository,
        IUploadSettingRepository uploadSettingRepository,
        ILabManagementUploadExcelDataRepository labManagementUploadExcelDataRepository,
        // ILabManagementVariableMappingRepository labManagementVariableMappingRepository,
        // IProjectDesignVariableRepository projectDesignVariableRepository,
        // IScreeningTemplateValueRepository screeningTemplateValueRepository,
        // IScreeningVisitRepository screeningVisitRepository,
        //IRandomizationRepository randomizationRepository,
        //IScreeningTemplateRepository screeningTemplateRepository,
        //IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
        //IAppSettingRepository appSettingRepository,
        IProjectRepository projectRepository,
        IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _configurationRepository = configurationRepository;
            _labManagementUploadDataRepository = labManagementUploadDataRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _labManagementUploadExcelDataRepository = labManagementUploadExcelDataRepository;
            //_labManagementVariableMappingRepository = labManagementVariableMappingRepository;
            //_projectDesignVariableRepository = projectDesignVariableRepository;
            //_screeningTemplateValueRepository = screeningTemplateValueRepository;
            //_screeningTemplateRepository = screeningTemplateRepository;
            //_screeningVisitRepository = screeningVisitRepository;
            //_randomizationRepository = randomizationRepository;
            //_screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            //_appSettingRepository = appSettingRepository;
            _projectRepository = projectRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_labManagementUploadDataRepository.GetUploadDataList(isDeleted));
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] LabManagementUploadDataDto labManagementUploadDataDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            labManagementUploadDataDto.Id = 0;
             
            var LabManagementConfiguration = _configurationRepository.All.Where(x => x.ProjectDesignTemplateId == labManagementUploadDataDto.ProjectDesignTemplateId).FirstOrDefault();
            
            labManagementUploadDataDto.LabManagementConfigurationId = LabManagementConfiguration.Id;

            //set file path and extension
            if (labManagementUploadDataDto.FileModel?.Base64?.Length > 0)
            {
                labManagementUploadDataDto.PathName = DocumentService.SaveUploadDocument(labManagementUploadDataDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(labManagementUploadDataDto.ProjectId), FolderType.LabManagement, "");
                labManagementUploadDataDto.MimeType = labManagementUploadDataDto.FileModel.Extension;
                labManagementUploadDataDto.FileName = "LabManagementData_" + DateTime.Now.Ticks + "." + labManagementUploadDataDto.FileModel.Extension;
            }

            labManagementUploadDataDto.LabManagementUploadStatus = LabManagementUploadStatus.Pending;
            var labManagementUploadData = _mapper.Map<LabManagementUploadData>(labManagementUploadDataDto);

            //Upload Excel data into database table
            var validate = _labManagementUploadDataRepository.InsertExcelDataIntoDatabaseTable(labManagementUploadData);

            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            //labManagementUploadData.LabManagementUploadExcelDatas = ExcelData;

            //_labManagementUploadDataRepository.Add(labManagementUploadData);

            //foreach (var item in labManagementUploadData.LabManagementUploadExcelDatas)
            //{
            //    _labManagementUploadExcelDataRepository.Add(item);
            //}

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
            {
                _labManagementUploadDataRepository.InsertDataIntoDataEntry(labManagementUpload);

                #region Comment
                //var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
                //GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

                //// variable mapping data
                //var MappingData = _labManagementVariableMappingRepository.All.Where(x => x.LabManagementConfigurationId == labManagementUpload.LabManagementConfigurationId
                // && x.DeletedDate == null)
                //    .Include(x => x.LabManagementConfiguration)
                //    .ThenInclude(x => x.ProjectDesignTemplate)
                //    .ToList();

                //if (MappingData != null)
                //{
                //    // Upload Excel sheet data
                //    var ExcelData = _labManagementUploadExcelDataRepository.All.Where(x => x.LabManagementUploadDataId == labManagementUpload.Id).ToList();

                //    if (ExcelData != null)
                //    {
                //        foreach (var item in MappingData)
                //        {
                //            // filter Excel data
                //            var result = _labManagementUploadExcelDataRepository.All.Where(x => x.LabManagementUploadDataId == labManagementUpload.Id && x.TestName == item.TargetVariable).ToList();
                //            var dataType = _projectDesignVariableRepository.Find(item.ProjectDesignVariableId).CollectionSource;

                //            if (result != null)
                //            {
                //                foreach (var r in result)
                //                {
                //                    var screeningTemplate = _screeningTemplateRepository.All.Where(x => x.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber == r.ScreeningNo                              
                //                                    && x.ProjectDesignTemplateId == item.LabManagementConfiguration.ProjectDesignTemplateId).FirstOrDefault();

                //                    if (screeningTemplate != null)
                //                    {
                //                        ScreeningTemplateValue obj = new ScreeningTemplateValue();
                //                        obj.ScreeningTemplateId = screeningTemplate.Id;
                //                        obj.ProjectDesignVariableId = item.ProjectDesignVariableId;
                //                        if (dataType == CollectionSources.Date || dataType == CollectionSources.DateTime || dataType == CollectionSources.Time)
                //                        {
                //                            DateTime dDate;
                //                            string variablevalueformat = r.Result;
                //                            var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                //                            obj.Value = dt;
                //                        }
                //                        else
                //                            obj.Value = r.Result;
                //                        obj.ReviewLevel = 0;
                //                        obj.IsNa = false;
                //                        obj.IsSystem = false;
                //                        obj.LabManagementUploadExcelDataId = r.Id;
                //                        _screeningTemplateValueRepository.Add(obj);

                //                        var aduit = new ScreeningTemplateValueAudit
                //                        {
                //                            ScreeningTemplateValue = obj,
                //                            Value = r.Result
                //                        };
                //                        _screeningTemplateValueAuditRepository.Save(aduit);

                //                        //if (screeningTemplate.Status == Helper.ScreeningTemplateStatus.Pending)
                //                        //{
                //                        //    screeningTemplate.Status = Helper.ScreeningTemplateStatus.InProcess;
                //                        //    screeningTemplate.IsDisable = false;
                //                        //    _screeningTemplateRepository.Update(screeningTemplate);
                //                        //}
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion Comment
            }
            _labManagementUploadDataRepository.Update(labManagementUpload);

            if (_uow.Save() <= 0) throw new Exception("Updating lab management data failed on action.");
            return Ok(labManagementUpload.Id);
        }

    }
}
