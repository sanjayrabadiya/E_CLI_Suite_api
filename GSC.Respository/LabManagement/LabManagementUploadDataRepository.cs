using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.LabManagement
{
    public class LabManagementUploadDataRepository : GenericRespository<LabManagementUploadData>, ILabManagementUploadDataRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly ILabManagementVariableMappingRepository _labManagementVariableMappingRepository;
        private readonly ILabManagementUploadExcelDataRepository _labManagementUploadExcelDataRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;

        public LabManagementUploadDataRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IUploadSettingRepository uploadSettingRepository,
            ILabManagementVariableMappingRepository labManagementVariableMappingRepository,
            ILabManagementUploadExcelDataRepository labManagementUploadExcelDataRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
         IScreeningTemplateValueRepository screeningTemplateValueRepository,
        IScreeningTemplateRepository screeningTemplateRepository,
        IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IAppSettingRepository appSettingRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
            _appSettingRepository = appSettingRepository;
            _labManagementVariableMappingRepository = labManagementVariableMappingRepository;
            _labManagementUploadExcelDataRepository = labManagementUploadExcelDataRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
        }

        public List<LabManagementUploadDataGridDto> GetUploadDataList(bool isDeleted)
        {
            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<LabManagementUploadDataGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            result.ForEach(t => t.FullPath = documentUrl + t.PathName);
            return result;
        }

        // Upload excel data insert into database
        //public List<LabManagementUploadExcelData> InsertExcelDataIntoDatabaseTable(LabManagementUploadData labManagementUploadData)
        public string InsertExcelDataIntoDatabaseTable(LabManagementUploadData labManagementUploadData, string StudyCode)
        {
            if (!_labManagementVariableMappingRepository.All.Any(x => x.LabManagementConfigurationId == labManagementUploadData.LabManagementConfigurationId && x.DeletedDate == null))
                return "You can not upload excel data before mapping variable in configuration.";

            var details = _labManagementVariableMappingRepository.All
                  .Where(x => x.LabManagementConfigurationId == labManagementUploadData.LabManagementConfigurationId && x.DeletedDate == null)
                 .Select(t => new
                 {
                     t.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                     t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                 }).ToList();
            
            // Add Lab Management Upload Data and status
            Add(labManagementUploadData);

            var documentUrl = _uploadSettingRepository.GetDocumentPath();
            string pathname = documentUrl + labManagementUploadData.PathName;
            FileStream streamer = new FileStream(pathname, FileMode.Open);
            IExcelDataReader reader = null;
            if (Path.GetExtension(pathname) == ".xls")
                reader = ExcelReaderFactory.CreateBinaryReader(streamer);
            else
                reader = ExcelReaderFactory.CreateOpenXmlReader(streamer);
            DataSet results = reader.AsDataSet();
            results.Tables[0].Rows[0].Delete();
            results.Tables[0].AcceptChanges();

            List<LabManagementUploadExcelData> objLst = new List<LabManagementUploadExcelData>();

            foreach (var item in results.Tables[0].Rows)
            {
                if (((DataRow)item).ItemArray[0].ToString().Trim() != StudyCode.Trim())
                    return "Can not upload excel data due to study code not match.";

                if (((DataRow)item).ItemArray[4].ToString().Trim() != details.FirstOrDefault().DisplayName.Trim())
                    return "Can not upload excel data due to visit name not match.";

                //if (((DataRow)item).ItemArray[0].ToString().Trim() != details.FirstOrDefault().TemplateName.Trim())
                //    return "Can not upload excel data due to template not match.";

                LabManagementUploadExcelData obj = new LabManagementUploadExcelData();
                obj.ScreeningNo = ((DataRow)item).ItemArray[2].ToString();
                obj.RandomizationNo = ((DataRow)item).ItemArray[3].ToString();
                obj.Visit = ((DataRow)item).ItemArray[4].ToString();
                obj.RepeatSampleCollection = ((DataRow)item).ItemArray[5].ToString();
                obj.LaboratryName = ((DataRow)item).ItemArray[6].ToString();
                obj.DateOfSampleCollection = (DateTime)((DataRow)item).ItemArray[7];
                obj.DateOfReport = (DateTime)((DataRow)item).ItemArray[8];
                obj.Panel = ((DataRow)item).ItemArray[9].ToString();
                obj.TestName = ((DataRow)item).ItemArray[10].ToString();
                obj.Result = ((DataRow)item).ItemArray[11].ToString();
                obj.Unit = ((DataRow)item).ItemArray[12].ToString();
                obj.AbnoramalFlag = ((DataRow)item).ItemArray[13].ToString();
                obj.ReferenceRangeLow = ((DataRow)item).ItemArray[14].ToString();
                obj.ReferenceRangeHigh = ((DataRow)item).ItemArray[15].ToString();
                obj.ClinicallySignificant = ((DataRow)item).ItemArray[16].ToString();
                obj.CreatedBy = _jwtTokenAccesser.UserId;
                obj.CreatedDate = _jwtTokenAccesser.GetClientDate();
                objLst.Add(obj);
            }
            streamer.Dispose();
            labManagementUploadData.LabManagementUploadExcelDatas = objLst;

            // Add Lab Management Upload excel Data into db table
            foreach (var item in labManagementUploadData.LabManagementUploadExcelDatas)
            {
                _labManagementUploadExcelDataRepository.Add(item);
            }
            return "";
            // return objLst;
        }

        // Insert data into data entry screening template, screening template value and screening template audit
        public void InsertDataIntoDataEntry(LabManagementUploadData labManagementUpload)
        {
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

            // get variable mapping data by Lab Management configuration id
            var MappingData = _labManagementVariableMappingRepository.All
                   .Where(x => x.LabManagementConfigurationId == labManagementUpload.LabManagementConfigurationId && x.DeletedDate == null)
                  .Select(t => new
                  {
                      t.TargetVariable,
                      t.ProjectDesignVariableId,
                      t.ProjectDesignVariable.CollectionSource,
                      t.LabManagementConfiguration.ProjectDesignTemplateId
                  }).ToList();

            if (MappingData != null)
            {
                // Get Upload Excel sheet data
                var ExcelDataResult = _labManagementUploadExcelDataRepository.All.Where(x => x.LabManagementUploadDataId == labManagementUpload.Id).ToList();

                if (ExcelDataResult != null)
                {
                    var ScreningNumberList = ExcelDataResult.Select(x => x.ScreeningNo).ToList().Distinct();

                    foreach (var ScreningNumber in ScreningNumberList)
                    {
                        var GetExcelDataByScreeningNumber = ExcelDataResult.Where(x => x.ScreeningNo == ScreningNumber).ToList();

                        var screeningTemplate = _screeningTemplateRepository.All.Where(x => x.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber == ScreningNumber
                                                   && x.ProjectDesignTemplateId == MappingData.FirstOrDefault().ProjectDesignTemplateId).FirstOrDefault();

                        var isRepatTemplated = GetExcelDataByScreeningNumber.Any(t => t.RepeatSampleCollection == "Yes");

                        if (isRepatTemplated && screeningTemplate!= null)
                        {
                            screeningTemplate = _screeningTemplateRepository.TemplateRepeat(screeningTemplate.Id);
                            screeningTemplate.Status = Helper.ScreeningTemplateStatus.InProcess;
                        }
                        else if (screeningTemplate != null)
                        {
                            // update screening template status
                            if (screeningTemplate.Status == Helper.ScreeningTemplateStatus.Pending)
                            {
                                screeningTemplate.Status = Helper.ScreeningTemplateStatus.InProcess;
                                screeningTemplate.IsDisable = false;
                                _screeningTemplateRepository.Update(screeningTemplate);
                            }
                        }

                        if (screeningTemplate != null)
                        {
                            foreach (var item in MappingData)
                            {
                                // get upload Excel data by lab management upload id and variale name
                                var result = GetExcelDataByScreeningNumber.Where(x => x.TestName == item.TargetVariable).ToList();
                                var dataType = item.CollectionSource;
                                foreach (var r in result)
                                {
                                    // insert screening template value
                                    var obj = _screeningTemplateValueRepository.All.Where(x => x.ProjectDesignVariableId == item.ProjectDesignVariableId
                                      && x.ScreeningTemplateId == screeningTemplate.Id).FirstOrDefault() ?? new ScreeningTemplateValue();

                                    obj.ScreeningTemplateId = screeningTemplate.Id;
                                    obj.ScreeningTemplate = screeningTemplate;

                                    obj.ProjectDesignVariableId = item.ProjectDesignVariableId;

                                    if (item.TargetVariable.Trim().ToLower() == "date of sample collection")
                                    {
                                        DateTime dDate;
                                        string variablevalueformat = r.DateOfSampleCollection.ToString();
                                        var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat)
                                                                                    .ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                                        obj.Value = dt;
                                    }
                                    else if (item.TargetVariable.Trim().ToLower() == "date of report")
                                    {

                                        DateTime dDate;
                                        string variablevalueformat = r.DateOfReport.ToString();
                                        var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat)
                                                                                    .ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                                        obj.Value = dt;
                                    }
                                    else if (item.TargetVariable.Trim().ToLower() == "laboratory Name")
                                        obj.Value = r.LaboratryName;
                                    else if (item.TargetVariable.Trim().ToLower() == "clinically significant")
                                        obj.Value = r.ClinicallySignificant;
                                    else
                                    {
                                        // set date time format if data type is date datetime or time
                                        if (dataType == CollectionSources.Date || dataType == CollectionSources.DateTime || dataType == CollectionSources.Time)
                                        {
                                            DateTime dDate;
                                            string variablevalueformat = r.Result;
                                            var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat)
                                                                                        .ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                                            obj.Value = dt;
                                        }
                                        else
                                            obj.Value = r.Result;
                                    }

                                    obj.ReviewLevel = 0;
                                    obj.IsNa = false;
                                    obj.IsSystem = false;
                                    obj.LabManagementUploadExcelDataId = r.Id;

                                    if (obj.Id > 0)
                                        _screeningTemplateValueRepository.Update(obj);
                                    else
                                        _screeningTemplateValueRepository.Add(obj);

                                    // insert screening template value audit
                                    var aduit = new ScreeningTemplateValueAudit
                                    {
                                        ScreeningTemplateValue = obj,
                                        Value = r.Result,
                                        Note = "Added by Lab management"
                                    };
                                    _screeningTemplateValueAuditRepository.Save(aduit);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Add by vipul for check configuration already use in upload data or not
        public string CheckDataIsUploadForDeleteConfiguration(int Id)
        {
            if (All.Any(x => x.LabManagementConfigurationId == Id && x.DeletedDate == null))
                return "You can not delete configuration due to already upload excel data.";
            return "";
        }

        // Add by vipul for check configuration already use in upload data and status
        public string CheckDataIsUploadForRemapping(int Id)
        {
            if (All.Any(x => x.LabManagementConfigurationId == Id && x.DeletedDate == null && x.LabManagementUploadStatus == LabManagementUploadStatus.Approve))
                return "You can not re-map variable due to upload excel data already approve.";
            return "";
        }
    }
}
