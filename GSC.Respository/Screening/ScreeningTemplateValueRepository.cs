using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Respository.Reports;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueRepository : GenericRespository<ScreeningTemplateValue>,
        IScreeningTemplateValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IGSCContext _context;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly ITemplateVariableSequenceNoSettingRepository _templateVariableSequenceNoSettingRepository;
        private readonly IProjectWorkflowRepository _projectWorkflowRepository;
        private readonly IMapper _mapper;
        private readonly IProjectRightRepository _projectRightRepository;
        public ScreeningTemplateValueRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignVariableRepository projectDesignVariableRepository, IAppSettingRepository appSettingRepository,
            IJobMonitoringRepository jobMonitoringRepository,
            IUserRepository userRepository, IEmailSenderRespository emailSenderRespository,
            IUploadSettingRepository uploadSettingRepository,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            ITemplateVariableSequenceNoSettingRepository templateVariableSequenceNoSettingRepository,
        IProjectWorkflowRepository projectWorkflowRepository, IMapper mapper,
            IProjectRightRepository projectRightRepository)
            : base(context)
        {
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _appSettingRepository = appSettingRepository;
            _context = context;
            _jobMonitoringRepository = jobMonitoringRepository;
            _emailSenderRespository = emailSenderRespository;
            _userRepository = userRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _templateVariableSequenceNoSettingRepository = templateVariableSequenceNoSettingRepository;
            _projectWorkflowRepository = projectWorkflowRepository;
            _projectRightRepository = projectRightRepository;
            _mapper = mapper;
        }

        public void UpdateVariableOnSubmit(int projectDesignTemplateId, int screeningTemplateId)
        {
            var screeningDesignVariableId = All.Where(x => x.ScreeningTemplateId == screeningTemplateId).Select(r => r.ProjectDesignVariableId).ToList();

            var templateVariable = _projectDesignVariableRepository.All.Where(t => t.ProjectDesignTemplateId == projectDesignTemplateId).ToList();

            templateVariable = templateVariable.Where(r => !screeningDesignVariableId.Contains(r.Id)).ToList();

            foreach (var variable in templateVariable)
            {

                var screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplateId,
                    ProjectDesignVariableId = variable.Id,
                    Value = variable.DefaultValue,

                };
                Add(screeningTemplateValue);


                var audit = new ScreeningTemplateValueAudit
                {
                    ScreeningTemplateValue = screeningTemplateValue,
                    Value = string.IsNullOrEmpty(variable.DefaultValue) ? "" : variable.DefaultValue,
                    OldValue = null,
                    Note = "Submitted with default data"
                };
                _screeningTemplateValueAuditRepository.Save(audit);
            }
        }

        public void UpdateDefaultValue(IList<DesignScreeningVariableDto> variableList, int screeningTemplateId)
        {
            var screeningDesignVariableId = All.Where(x => x.ScreeningTemplateId == screeningTemplateId).Select(r => r.ProjectDesignVariableId).ToList();
            if (screeningDesignVariableId != null && screeningDesignVariableId.Count > 0)
                return;

            var templateVariable = variableList.Where(r => !screeningDesignVariableId.Contains(r.Id)
            && !string.IsNullOrEmpty(r.DefaultValue) && r.CollectionSource != CollectionSources.HorizontalScale
            ).ToList();

            foreach (var variable in templateVariable)
            {
                var screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplateId,
                    ProjectDesignVariableId = variable.Id,
                    Value = variable.DefaultValue,

                };
                Add(screeningTemplateValue);


                var audit = new ScreeningTemplateValueAudit
                {
                    ScreeningTemplateValue = screeningTemplateValue,
                    Value = string.IsNullOrEmpty(variable.DefaultValue) ? "" : variable.DefaultValue,
                    OldValue = null,
                    Note = "Submitted with default data"
                };
                _screeningTemplateValueAuditRepository.Save(audit);
            }
            _context.Save();
        }

        public void UpdateTemplateConfigurationUploadRandomizationValue(DesignScreeningTemplateDto designScreeningTemplateDto, int screeningTemplateId)
        {
            var screeningDesignVariableId = All.Where(x => x.ScreeningTemplateId == screeningTemplateId).Select(r => r.ProjectDesignVariableId).ToList();
            //if (screeningDesignVariableId != null && screeningDesignVariableId.Count > 0)
            //    return;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         

            var templateVariable = designScreeningTemplateDto.Variables.Where(r =>
            //!screeningDesignVariableId.Contains(r.Id) &&
            r.CollectionSource == CollectionSources.TextBox
            ).ToList();

            var projectdata = _context.ScreeningTemplate
                                            .Include(x => x.ScreeningVisit)
                                            .ThenInclude(x => x.ScreeningEntry)
                                            .ThenInclude(x => x.Randomization).Where(x => x.Id == screeningTemplateId).Select(x =>
                                          new
                                          {
                                              ProjectId = x.ScreeningVisit.ScreeningEntry.Project.ParentProjectId,
                                              RandomizationId = x.ScreeningVisit.ScreeningEntry.RandomizationId,
                                              RandomizationNo = x.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber,
                                              RandomizationDate = x.ScreeningVisit.ScreeningEntry.Randomization.DateOfRandomization
                                          }).FirstOrDefault();

            if (projectdata == null)
            {
                return;
            }

            var RandomizationSetting = _context.RandomizationNumberSettings.Where(x => x.ProjectId == projectdata.ProjectId).FirstOrDefault();
            if (RandomizationSetting == null)
            {
                return;
            }
            var numbersetting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectdata.ProjectId).FirstOrDefault();
            if (numbersetting == null)
                return;
            var verifyuploadsheetdata = _context.SupplyManagementUploadFileDetail.Include(x => x.SupplyManagementUploadFile).Where(x => x.SupplyManagementUploadFile.ProjectId == projectdata.ProjectId && x.RandomizationId == projectdata.RandomizationId).FirstOrDefault();

            if (RandomizationSetting.IsIGT && verifyuploadsheetdata != null)
            {
                foreach (var variable in templateVariable)
                {

                    string value = string.Empty;
                    var allocationsetting = _context.SupplyManagementAllocation.Where(x => x.DeletedDate == null && x.ProjectDesignVariableId == variable.Id).FirstOrDefault();
                    if (allocationsetting != null)
                    {
                        variable.IsDisabled = true;
                        if (allocationsetting.Type == SupplyManagementAllocationType.RandomizationNo)
                        {
                            value = projectdata.RandomizationNo;
                        }
                        if (allocationsetting.Type == SupplyManagementAllocationType.RandomizationDate)
                        {
                            value = projectdata.RandomizationDate.ToString();
                        }
                        if (allocationsetting.Type == SupplyManagementAllocationType.ProductCode)
                        {
                            var uploadvisits = _context.SupplyManagementUploadFileVisit
                                  .Where(x => x.SupplyManagementUploadFileDetailId == verifyuploadsheetdata.Id
                                        && x.ProjectDesignVisitId == designScreeningTemplateDto.ProjectDesignVisitId).FirstOrDefault();

                            if (uploadvisits != null)
                                value = uploadvisits.Value;
                        }
                        if (allocationsetting.Type == SupplyManagementAllocationType.ProductName)
                        {
                            var producttype = _context.ProductType.Where(x => x.ProductTypeCode == verifyuploadsheetdata.TreatmentType).FirstOrDefault();
                            value = producttype != null ? producttype.ProductTypeName : "";
                        }
                        if (allocationsetting.Type == SupplyManagementAllocationType.KitNo)
                        {
                            if (numbersetting.KitCreationType == KitCreationType.KitWise)
                            {
                                if (numbersetting.IsDoseWiseKit)
                                {
                                    var producttype = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Where(x => x.SupplyManagementKIT.ProjectDesignVisitId == designScreeningTemplateDto.ProjectDesignVisitId
                                 && x.DeletedDate == null && x.RandomizationId == projectdata.RandomizationId).Select(s => s.KitNo).ToList();
                                    value = producttype != null && producttype.Count > 0 ? String.Join(",", producttype.Distinct()) : "";
                                }
                                else
                                {
                                    var producttype = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Where(x => x.SupplyManagementKIT.ProjectDesignVisitId == designScreeningTemplateDto.ProjectDesignVisitId
                                 && x.DeletedDate == null && x.RandomizationId == projectdata.RandomizationId).FirstOrDefault();
                                    value = producttype != null ? producttype.KitNo : "";
                                }

                            }
                            if (numbersetting.KitCreationType == KitCreationType.SequenceWise)
                            {
                                var producttype = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).Where(x => x.ProjectDesignVisitId == designScreeningTemplateDto.ProjectDesignVisitId
                                    && x.DeletedDate == null && x.RandomizationId == projectdata.RandomizationId).FirstOrDefault();
                                value = producttype != null ? producttype.SupplyManagementKITSeries.KitNo : "";
                            }
                        }
                        var screeningTemplateValue = new ScreeningTemplateValue
                        {
                            ScreeningTemplateId = screeningTemplateId,
                            ProjectDesignVariableId = variable.Id,
                            Value = value,

                        };

                        var data = All.Where(x => x.ScreeningTemplateId == screeningTemplateId && x.ProjectDesignVariableId == variable.Id).FirstOrDefault();
                        if (data == null)
                        {

                            Add(screeningTemplateValue);

                            if (string.IsNullOrEmpty(variable.ScreeningValue))
                            {
                                var audit = new ScreeningTemplateValueAudit
                                {
                                    ScreeningTemplateValue = screeningTemplateValue,
                                    Value = value,
                                    OldValue = null,
                                    Note = "Submitted with IWRS data"
                                };
                                _screeningTemplateValueAuditRepository.Save(audit);
                            }
                        }
                        if (data != null)
                        {
                            data.Value = value;
                            Update(data);
                        }



                    }

                }
            }
            _context.Save();
        }

        public int GetQueryStatusCount(int screeningTemplateId)
        {
            return All.Where(x => x.DeletedDate == null
                    && x.ProjectDesignVariable.DeletedDate == null
                    && x.ScreeningTemplateId == screeningTemplateId
                    && x.QueryStatus != null
                    && x.QueryStatus != QueryStatus.Closed).Count();

        }

        public List<VariableQueryDto> GetTemplateQueryList(int screeningTemplateId)
        {
            var projectDesignId = _context.ScreeningTemplate.Where(x => x.Id == screeningTemplateId).Select(t => t.ScreeningVisit.ScreeningEntry.ProjectDesignId).FirstOrDefault();

            var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(projectDesignId);

            var workText = workflowlevel.WorkFlowText ?? new List<WorkFlowText>();

            workText.Add(new WorkFlowText
            {
                LevelNo = -1,
                RoleName = "Operator"
            });

            workText.Add(new WorkFlowText
            {
                LevelNo = 0,
                RoleName = "Independent"
            });

            var result = All.Where(x => x.DeletedDate == null
                     && x.ProjectDesignVariable.DeletedDate == null
                     && x.ScreeningTemplateId == screeningTemplateId
                     && x.QueryStatus != null
                     && x.QueryStatus != QueryStatus.Closed).Select(t => new
                     {
                         VariableName = (_jwtTokenAccesser.Language != 1 ?
                         t.ProjectDesignVariable.VariableLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && c.DeletedDate == null && t.ProjectDesignVariable.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : t.ProjectDesignVariable.VariableName),
                         t.QueryStatus,
                         t.AcknowledgeLevel,
                         t.SecurityRole.RoleShortName
                     }).GroupBy(a => new { a.QueryStatus, a.VariableName, a.AcknowledgeLevel, a.RoleShortName }).
                    Select(b => new VariableQueryDto
                    {
                        QueryStatus = b.Key.QueryStatus.GetDescription(),
                        VariableName = b.Key.VariableName,
                        Level = b.Key.AcknowledgeLevel ?? 0,
                        LevelName = b.Key.AcknowledgeLevel == 0 ? "Independent - " + b.Key.RoleShortName : "",
                        Total = b.Count()
                    }).ToList();

            result.Where(a => a.Level != 0).ToList().ForEach(x => x.LevelName = workText.FirstOrDefault(a => a.LevelNo == x.Level)?.RoleName);

            return result;

        }



        public List<TemplateTotalQueryDto> GetQueryStatusBySubject(int screeningEntryId)
        {
            var totalQueries = All.Where(x => x.DeletedDate == null
                && x.ProjectDesignVariable.DeletedDate == null
                && x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == screeningEntryId
                && x.QueryStatus != null
                && x.QueryStatus != QueryStatus.Closed).GroupBy(t => t.ScreeningTemplateId)
                .Select(r => new TemplateTotalQueryDto
                {
                    ScreeningTemplateId = r.Key,
                    Total = r.Count()
                }).ToList();

            return totalQueries;
        }


        public void DeleteChild(int screeningTemplateValueId)
        {
            var childs = _context.ScreeningTemplateValueChild
                .Where(t => t.ScreeningTemplateValueId == screeningTemplateValueId).ToList();
            _context.ScreeningTemplateValueChild.RemoveRange(childs);
        }

        public void UpdateChild(List<ScreeningTemplateValueChild> children)
        {
            _context.ScreeningTemplateValueChild.UpdateRange(children);
        }

        public string CheckCloseQueries(int screeningTemplateId)
        {
            var validateMsg = "";

            if (All.Any(x => x.ScreeningTemplateId == screeningTemplateId && x.DeletedDate == null &&
            x.ProjectDesignVariable.DeletedDate == null
            && x.QueryStatus != null && x.QueryStatus != QueryStatus.Closed))
                validateMsg = "Please close all queries! \n";

            return validateMsg;
        }

        public bool IsFitness(int screeningTemplateId)
        {
            return All.Any(x => x.DeletedDate == null &&
                                x.ScreeningTemplateId == screeningTemplateId &&
                                x.ProjectDesignVariable.SystemType != null &&
                                x.ProjectDesignVariable.Values != null &&
                                x.ProjectDesignVariable.Values.Any(r => r.ValueCode == "Fitness01"));
        }

        public bool IsDiscontinued(int screeningTemplateId)
        {
            return All.Any(x => x.DeletedDate == null &&
                                x.ScreeningTemplateId == screeningTemplateId &&
                                x.ProjectDesignVariable.SystemType != null &&
                                x.ProjectDesignVariable.Values != null &&
                                x.ProjectDesignVariable.Values.Any(r => r.ValueCode == "Dis01"));
        }



        public string GetValueForAudit(ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.IsDeleted) return null;

            if (screeningTemplateValueDto.Children?.Count > 0)
            {
                var child = screeningTemplateValueDto.Children.First();

                var variableValue = _context.ProjectDesignVariableValue.Find(child.ProjectDesignVariableValueId);
                if (variableValue != null)
                {
                    var valueChild = _context.ScreeningTemplateValueChild.AsNoTracking()
                        .FirstOrDefault(t => t.Id == child.Id);
                    if (valueChild != null && child.Value == "false")
                    {
                        screeningTemplateValueDto.OldValue = variableValue.ValueName;
                        return "";
                    }

                    if (screeningTemplateValueDto.CollectionSource == CollectionSources.Table)
                    {
                        if (valueChild != null && child.Value != screeningTemplateValueDto.Value)
                        {
                            screeningTemplateValueDto.OldValue = valueChild.Value + "_" + variableValue.ValueName + "_" + child.LevelNo;
                            return child.Value + "_" + variableValue.ValueName + "_" + child.LevelNo;
                        }
                        screeningTemplateValueDto.OldValue = "";
                        return child.Value + "_" + variableValue.ValueName + "_" + child.LevelNo;
                    }

                    screeningTemplateValueDto.OldValue = "";
                    return variableValue.ValueName;
                }

                return child.Value;
            }

            if (screeningTemplateValueDto.IsNa)
                return "N/A";

            return string.IsNullOrWhiteSpace(screeningTemplateValueDto.ValueName)
                ? screeningTemplateValueDto.Value
                : screeningTemplateValueDto.ValueName;
        }


        public void GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters)
        {
            var ProjectCode = _context.Project.Find(filters.ParentProjectId).ProjectCode;
            var sites = new List<int>();
            if (filters.SiteId != null)
            {
                sites = _context.Project.Where(x => x.Id == filters.SiteId).Select(x => x.Id).ToList();
            }
            else
            {
                // Change done by swati on 11 oct 2021 for sites bug
                //sites = _context.Project.Where(x => x.ParentProjectId == filters.ParentProjectId && x.IsTestSite == false && x.DeletedDate == null).ToList().Select(x => x.Id).ToList();

                var projectList = _projectRightRepository.GetProjectRightIdList();
                if (projectList == null || projectList.Count == 0) sites = null;

                sites = _context.Project.Where(x =>
                    x.DeletedDate == null && x.ParentProjectId == filters.ParentProjectId && x.IsTestSite == false
                    && projectList.Any(c => c == x.Id)).Select(y => y.Id).ToList();
            }
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

            CommonDto MainData = new CommonDto();

            #region Job Monitoring Save - Inprocess Status
            JobMonitoring jobMonitoring = new JobMonitoring();
            jobMonitoring.JobName = JobNameType.DBDSReport;
            jobMonitoring.JobDescription = filters.SelectedProject;
            jobMonitoring.JobType = filters.ExcelFormat ? JobTypeEnum.Excel : JobTypeEnum.Csv;
            jobMonitoring.JobStatus = JobStatusType.InProcess;
            jobMonitoring.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoring.SubmittedTime = _jwtTokenAccesser.GetClientDate();
            _jobMonitoringRepository.Add(jobMonitoring);
            _context.Save();

            #endregion

            var variableValues = _context.ProjectDesignVariableValue.
                Where(r => r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == filters.ParentProjectId).Select(r => new
                {
                    r.Id,
                    r.ValueName
                }).ToList();

            if (filters.FilterId == DBDSReportFilter.DBDS || filters.FilterId == null)
            {

                #region Main Query

                var tempValue = _context.ScreeningTemplateValue.Where(r => r.DeletedDate == null && r.ScreeningTemplate.DeletedDate == null && r.ScreeningTemplate.ScreeningVisit.DeletedDate == null
                 && r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.DeletedDate == null &&
                 sites.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId));

                if (filters.PeriodIds != null && filters.PeriodIds.Count() > 0)
                {
                    tempValue = tempValue.Where(r => filters.PeriodIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId));
                }

                if (filters.SubjectIds != null && filters.SubjectIds.Count() > 0)
                {
                    tempValue = tempValue.Where(r => filters.SubjectIds.Contains(r.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Id));
                }

                if (filters.TemplateIds != null && filters.TemplateIds.Count() > 0)
                {
                    tempValue = tempValue.Where(r => filters.TemplateIds.Contains(r.ScreeningTemplate.ProjectDesignTemplateId));
                }

                if (filters.VisitIds != null && filters.VisitIds.Count() > 0)
                {
                    tempValue = tempValue.Where(r => filters.VisitIds.Contains(r.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId));
                }

                if (filters.DomainIds != null && filters.DomainIds.Count() > 0)
                {
                    tempValue = tempValue.Where(r => filters.DomainIds.Contains(r.ScreeningTemplate.ProjectDesignTemplate.DomainId));
                }

                var result = tempValue.Select(x => new ProjectDatabaseDto
                {
                    ScreeningEntryId = x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId,
                    ScreeningTemplateId = x.ScreeningTemplate.Id,
                    RepeatSeqNo = x.ScreeningTemplate.RepeatSeqNo,
                    ScreeningTemplateParentId = x.ScreeningTemplate.ParentId,
                    ProjectId = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId,
                    ProjectCode = ProjectCode,
                    ParentProjectId = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ParentProjectId,
                    DesignOrder = x.ScreeningTemplate.ProjectDesignTemplate.DesignOrder,

                    TemplateId = x.ScreeningTemplate.ProjectDesignTemplateId,
                    TemplateName = x.ScreeningTemplate.ScreeningTemplateName,
                    DomainName = x.ScreeningTemplate.ProjectDesignTemplate.Domain.DomainName,
                    DomainCode = x.ScreeningTemplate.ProjectDesignTemplate.Domain.DomainCode,
                    DomainId = x.ScreeningTemplate.ProjectDesignTemplate.DomainId,
                    VisitId = x.ScreeningTemplate.ScreeningVisit.ProjectDesignVisitId,
                    RepeatedVisit = x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber,
                    Visit = x.ScreeningTemplate.ScreeningVisit.ScreeningVisitName +
                                          Convert.ToString(x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + x.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber),

                    VariableName = x.ProjectDesignVariable.VariableName,
                    VariableId = x.ProjectDesignVariableId,
                    Annotation = x.ProjectDesignVariable.Annotation,
                    UnitId = x.ProjectDesignVariable.UnitId,
                    Unit = x.ProjectDesignVariable.Unit.UnitName,
                    UnitAnnotation = x.ProjectDesignVariable.UnitAnnotation,
                    VariableUnit = x.ProjectDesignVariable.Unit.UnitName,
                    DesignOrderOfVariable = x.ProjectDesignVariable.DesignOrder,

                    CollectionSource = (int)x.ProjectDesignVariable.CollectionSource,
                    VariableNameValue = x.IsNa && string.IsNullOrEmpty(x.Value) ? "NA" : x.Value,
                    VariableChildValue = string.Join(";", x.Children.Where(a => a.DeletedDate == null && a.Value == "true").Select(c => c.ProjectDesignVariableValue.ValueName).ToList()),
                    Initial = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ?
                    x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.Initial :
                    x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,

                    SubjectNo = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber :
                    x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo,

                    RandomizationNumber = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.RandomizationId != null ? x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.RandomizationNumber : "",
                    ProjectName = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                    VisitDesignOrder = x.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DesignOrder,
                    PeriodId = x.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.ProjectDesignPeriodId,
                    ScreeningTemplateValueId = x.Id,
                }).ToList();

                result.ForEach(r =>
                {
                    if (r.CollectionSource == (int)CollectionSources.ComboBox || r.CollectionSource == (int)CollectionSources.RadioButton ||
                                                r.CollectionSource == (int)CollectionSources.NumericScale || r.CollectionSource == (int)CollectionSources.CheckBox)
                    {
                        int Id;
                        bool isNumeric = int.TryParse(r.VariableNameValue, out Id);
                        if (isNumeric)
                            r.VariableNameValue = variableValues.FirstOrDefault(c => c.Id == Id).ValueName;
                    }
                    else if (r.CollectionSource == (int)CollectionSources.MultiCheckBox)
                    {
                        r.VariableNameValue = r.VariableChildValue;
                    }
                });

                
                var grpquery = result.OrderBy(d => d.VisitId).ThenBy(x => x.DesignOrder).GroupBy(x => new { x.DomainName, x.DomainId }).Select(y => new ProjectDatabaseDomainDto
                {
                    DomainName = y.Key.DomainName,
                    DomainCode = y.FirstOrDefault().DomainCode,
                    TemplateId = y.FirstOrDefault().TemplateId,
                    DesignOrder = y.FirstOrDefault().DesignOrder,
                    LstVariable = y.Where(q => q.DomainId == y.Key.DomainId && q.VariableName != null).
                    GroupBy(vari => vari.VariableName).Select(v =>
                        new ProjectDatabaseVariableDto
                        {
                            DomainName = v.FirstOrDefault().DomainName,
                            VariableName = v.Key,
                            Annotation = v.FirstOrDefault().Annotation,
                            UnitId = v.FirstOrDefault().UnitId,
                            Unit = v.FirstOrDefault().Unit,
                            UnitAnnotation = v.FirstOrDefault().UnitAnnotation,
                            DesignOrderOfVariable = v.FirstOrDefault().DesignOrderOfVariable,
                            TemplateId = v.FirstOrDefault().TemplateId
                        }).OrderBy(o => o.TemplateId).ThenBy(d => d.DesignOrderOfVariable).ToList(),

                    LstProjectDataBase = y.Where(v => v.VariableName != null && v.SubjectNo != null).GroupBy(x => new { x.Initial, x.SubjectNo }).Select(s => new ProjectDatabaseInitialDto
                    {
                        Initial = s.Key.Initial,
                        DomainName = s.FirstOrDefault().DomainName,
                        ProjectId = s.FirstOrDefault().ProjectId,
                        ProjectCode = s.FirstOrDefault().ProjectCode,
                        ParentProjectId = s.FirstOrDefault().ParentProjectId,
                        ProjectName = s.FirstOrDefault().ProjectName,
                        SubjectNo = s.Key.SubjectNo,
                        RandomizationNumber = s.FirstOrDefault().RandomizationNumber,
                        LstProjectDataBaseVisit = s.GroupBy(vst => vst.Visit).Select(n => new ProjectDatabaseVisitDto
                        {
                            Visit = n.Key,
                            VisitId = n.FirstOrDefault().VisitId,
                            VisitDesignOrder = n.FirstOrDefault().VisitDesignOrder,
                            PeriodId = n.FirstOrDefault().PeriodId,
                            LstProjectDataBaseTemplate = n.GroupBy(x => x.TemplateId).Select(t => new ProjectDatabaseTemplateDto
                            {
                                DesignOrder = t.FirstOrDefault().DesignOrder,
                                TemplateId = t.FirstOrDefault().TemplateId,
                                TemplateName = t.FirstOrDefault().TemplateName,
                                Visit = t.FirstOrDefault().Visit,
                                RepeatSeqNo = t.FirstOrDefault().RepeatSeqNo,
                                VisitId = t.FirstOrDefault().VisitId,
                                LstProjectDataBaseitems = t.OrderBy(o => o.ScreeningTemplateId).Select(i => new ProjectDatabaseItemDto
                                {
                                    Initial = s.Key.Initial,
                                    SubjectNo = s.Key.SubjectNo,
                                    DesignOrder = i.DesignOrder,
                                    TemplateName = i.TemplateName,
                                    TemplateId = i.TemplateId,
                                    ScreeningTemplateParentId = i.ScreeningTemplateParentId,
                                    DomainName = i.DomainName,
                                    VariableName = i.VariableName,
                                    ScreeningTemplateId = i.ScreeningTemplateId,
                                    CollectionSource = i.CollectionSource,
                                    VariableNameValue = i.VariableNameValue,
                                    UnitId = i.UnitId,
                                    Unit = i.Unit,
                                    RepeatSeqNo = i.RepeatSeqNo,
                                    ScreeningTemplateValueId = i.ScreeningTemplateValueId
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).OrderBy(p => p.ProjectId).ThenBy(x => x.SubjectNo).ToList()
                }).ToList();

                MainData.Dbds = grpquery;

                #endregion

            }

            if (filters.FilterId == DBDSReportFilter.MedDRA || filters.FilterId == null)
            {
                #region MedDRA

                var MeddraDetails = (from Mcode in _context.MeddraCoding
                                     join Mconfig in _context.MedraConfig on Mcode.MeddraConfigId equals Mconfig.Id
                                     join mv in _context.MedraVersion on Mconfig.MedraVersionId equals mv.Id
                                     join meddraSoc in _context.MeddraSocTerm on Mcode.MeddraSocTermId equals meddraSoc.Id
                                     join meddraLLT in _context.MeddraLowLevelTerm on Mcode.MeddraLowLevelTermId equals meddraLLT.Id
                                     join meddraMD in _context.MeddraMdHierarchy on meddraSoc.soc_code equals meddraMD.soc_code
                                     join ml in _context.MedraLanguage on Mconfig.LanguageId equals ml.Id
                                     join U in _context.Users on Mcode.ModifiedBy equals U.Id
                                     join stv in _context.ScreeningTemplateValue on Mcode.ScreeningTemplateValueId equals stv.Id
                                     join pdv in _context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                                     join d in _context.Domain on pdv.DomainId equals d.Id
                                     join st in _context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id
                                     join pdt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pdt.Id
                                     join sv in _context.ScreeningVisit on st.ScreeningVisitId equals sv.Id
                                     join pdvisit in _context.ProjectDesignVisit on sv.ProjectDesignVisitId equals pdvisit.Id
                                     join se in _context.ScreeningEntry on sv.ScreeningEntryId equals se.Id
                                     join r in _context.Randomization on se.RandomizationId equals r.Id
                                     join p in _context.Project on r.ProjectId equals p.Id

                                     where (sites.Contains(se.ProjectId) && (filters.PeriodIds == null || filters.PeriodIds.Contains(se.ProjectDesignPeriodId))
                                          && (filters.SubjectIds == null || filters.SubjectIds.Count() == 0 || filters.SubjectIds.Contains(se.Id))
                                          && se.DeletedDate == null) && st.DeletedDate == null
                                          && ((filters.TemplateIds == null || filters.TemplateIds.Contains(st.ProjectDesignTemplateId))
                                      && (filters.VisitIds == null || filters.VisitIds.Contains(st.ProjectDesignTemplate.ProjectDesignVisitId))
                                      && (filters.DomainIds == null || filters.DomainIds.Contains(st.ProjectDesignTemplate.DomainId))
                                      && st.Status != ScreeningTemplateStatus.Pending && st.Status != ScreeningTemplateStatus.InProcess)
                                      && Mcode.DeletedDate == null
                                      && Mconfig.DeletedDate == null
                                      && meddraMD.DeletedDate == null
                                      && meddraLLT.pt_code == meddraMD.pt_code
                                         && meddraSoc.MedraConfigId == Mconfig.Id
                                         && meddraLLT.MedraConfigId == Mconfig.Id
                                         && meddraMD.MedraConfigId == Mconfig.Id
                                     select new MeddraDetails
                                     {
                                         ProjectCode = ProjectCode,
                                         SiteCode = se.Project.ParentProjectId != null ? se.Project.ProjectCode : "",
                                         DomainCode = pdv.Domain.DomainName,
                                         //ScreeningNumber = nonregister.ScreeningNumber,
                                         //RandomizationNumber = nonregister.RandomizationNumber,
                                         //Initial = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                         Initial = st.ScreeningVisit.ScreeningEntry.RandomizationId != null ? r.Initial : st.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                                         ScreeningNumber = st.ScreeningVisit.ScreeningEntry.RandomizationId != null ? r.ScreeningNumber : st.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                                         RandomizationNumber = st.ScreeningVisit.ScreeningEntry.RandomizationId != null ? r.RandomizationNumber : "",
                                         RepeatedVisit = st.ScreeningVisit.RepeatedVisitNumber,
                                         // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                                         Visit = st.ScreeningVisit.ScreeningVisitName + Convert.ToString(st.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + st.ScreeningVisit.RepeatedVisitNumber),
                                         // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                                         TemplateName = st.ScreeningTemplateName,
                                         VariableAnnotation = pdv.Annotation,
                                         CollectionSource = (int)stv.ProjectDesignVariable.CollectionSource,
                                         VariableChildValue = string.Join(";", stv.Children.Where(a => a.DeletedDate == null && a.Value == "true").Select(c => c.ProjectDesignVariableValue.ValueName).ToList()),
                                         Version = mv.Version.ToString(),
                                         Language = ml.LanguageName,
                                         SocCode = meddraSoc.soc_code.ToString(),
                                         SocName = meddraSoc.soc_name,
                                         SocAbbrev = meddraSoc.soc_abbrev,
                                         PrimaryIndicator = meddraMD.primary_soc_fg,
                                         HlgtCode = meddraMD.hlgt_code.ToString(),
                                         HlgtName = meddraMD.hlgt_name,
                                         HltCode = meddraMD.hlt_code.ToString(),
                                         HltName = meddraMD.hlt_name,
                                         PtCode = meddraMD.pt_code.ToString(),
                                         PtName = meddraMD.pt_name,
                                         PtSocCode = meddraMD.pt_soc_code.ToString(),
                                         LltCode = meddraLLT.llt_code.ToString(),
                                         LltName = meddraLLT.llt_name,
                                         LltCurrency = meddraLLT.llt_currency,
                                         CodedBy = U.UserName,
                                         CodedOn = Mcode.ModifiedDate
                                     }).OrderBy(x => x.ScreeningNumber).ToList();



                MeddraDetails.ForEach(r =>
                {
                    if (r.CollectionSource == (int)CollectionSources.ComboBox || r.CollectionSource == (int)CollectionSources.RadioButton ||
                                                r.CollectionSource == (int)CollectionSources.NumericScale || r.CollectionSource == (int)CollectionSources.CheckBox)
                    {
                        int Id;
                        bool isNumeric = int.TryParse(r.VariableTerm, out Id);
                        if (isNumeric)
                            r.VariableTerm = variableValues.FirstOrDefault(c => c.Id == Id).ValueName;
                    }
                    else if (r.CollectionSource == (int)CollectionSources.MultiCheckBox)
                    {
                        r.VariableTerm = r.VariableChildValue;
                    }
                });

                #endregion

                MainData.Meddra = MeddraDetails;
            }


            #region Report Design
            var repeatdata = new List<RepeatTemplateDto>();
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet;

                if ((filters.FilterId == DBDSReportFilter.DBDS || filters.FilterId == null)
                    && (filters.Type == DbdsReportType.Domain || filters.Type == null))
                {
                    MainData.Dbds.ForEach(d =>
                    {
                        worksheet = workbook.Worksheets.Add(d.DomainCode);

                        worksheet.Rows(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                        worksheet.Cell(1, 1).Value = "STUDY CODE";
                        worksheet.Cell(1, 2).Value = "SITE CODE";
                        worksheet.Cell(1, 3).Value = "SCRNUM";
                        worksheet.Cell(1, 4).Value = "RANDNUM";
                        worksheet.Cell(1, 5).Value = "INITIAL";
                        worksheet.Cell(1, 6).Value = "VISIT";
                        worksheet.Cell(1, 7).Value = d.DomainName;

                        worksheet.Cell(2, 1).Value = "Study Code";
                        worksheet.Cell(2, 2).Value = "Site Code";
                        worksheet.Cell(2, 3).Value = "Screening No";
                        worksheet.Cell(2, 4).Value = "Enrollment No";
                        worksheet.Cell(2, 5).Value = "Patient Initial";
                        worksheet.Cell(2, 6).Value = "Visit";
                        worksheet.Cell(2, 7).Value = "Panel Name";

                        var totalVariable = d.LstVariable.Count;
                        var index = 0;
                        for (var k = 8; k < (totalVariable + 8); k++)
                        {
                            worksheet.Cell(1, k).Value = d.LstVariable[index].Annotation;
                            worksheet.Cell(2, k).Value = d.LstVariable[index].VariableName;
                            if (d.LstVariable[index].UnitId != null)
                            {
                                k += 1;
                                totalVariable = totalVariable + 1;
                                worksheet.Cell(1, k).Value = d.LstVariable[index].Annotation + "U";
                                worksheet.Cell(2, k).Value = !string.IsNullOrEmpty(d.LstVariable[index].UnitAnnotation) ? d.LstVariable[index].UnitAnnotation : d.LstVariable[index].VariableName + "_Unit";
                            }
                            index++;
                        }

                        var j = 3;
                        d.LstProjectDataBase.ForEach(db =>
                        {
                            db.LstProjectDataBaseVisit.ForEach(vst =>
                            {
                                vst.LstProjectDataBaseTemplate.ForEach(t =>
                                {
                                    var repeatlength = t.LstProjectDataBaseitems.Where(m => m.ScreeningTemplateParentId != null).GroupBy(x => x.ScreeningTemplateId).ToList().Count;

                                    worksheet.Row(j).Cell(1).SetValue(db.ProjectCode);
                                    worksheet.Row(j).Cell(2).SetValue(db.ParentProjectId != null ? db.ProjectName : "");
                                    worksheet.Row(j).Cell(3).SetValue(db.SubjectNo);
                                    worksheet.Row(j).Cell(4).SetValue(db.RandomizationNumber);
                                    worksheet.Row(j).Cell(5).SetValue(db.Initial);
                                    worksheet.Row(j).Cell(6).SetValue(vst.Visit);
                                    worksheet.Row(j).Cell(7).SetValue(t.DesignOrder + ". " + t.TemplateName);

                                    var repeatorder = 1;
                                    if (repeatlength > 0)
                                    {
                                        for (var m = 0; m < repeatlength; m++)
                                        {
                                            j = j + 1;
                                            worksheet.Row(j).Cell(1).SetValue(db.ProjectCode);
                                            worksheet.Row(j).Cell(2).SetValue(db.ParentProjectId != null ? db.ProjectName : "");
                                            worksheet.Row(j).Cell(3).SetValue(db.SubjectNo);
                                            worksheet.Row(j).Cell(4).SetValue(db.RandomizationNumber);
                                            worksheet.Row(j).Cell(5).SetValue(db.Initial);
                                            worksheet.Row(j).Cell(6).SetValue(vst.Visit);
                                            worksheet.Row(j).Cell(7).SetValue(t.DesignOrder + "." + repeatorder + " " + t.TemplateName);
                                            repeatorder++;
                                        }
                                    }
                                    j++;
                                });
                            });
                        });

                        var rownumber = 3;
                        var totallen = d.LstProjectDataBase.Count;
                        for (var n = 0; n < totallen; n++)
                        {
                            var totalVariablevisit = d.LstProjectDataBase[n].LstProjectDataBaseVisit.Count;
                            for (var vst = 0; vst < totalVariablevisit; vst++)
                            {
                                var totalTemplate = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate.Count;
                                for (var temp = 0; temp < totalTemplate; temp++)
                                {
                                    var indexrow = 0;
                                    var totalVariablelen = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems.Count;
                                    for (var m = 7; m < (totalVariablelen + 7); m++)
                                    {
                                        var variableName = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].VariableName;
                                        var parent = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].ScreeningTemplateParentId;
                                        var templateID = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].ScreeningTemplateId;
                                        var collectionSource = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].CollectionSource;

                                        if (parent != null)
                                        {
                                            var findparent = repeatdata.Where(x => x.Parent == parent && x.TemplateId == templateID).FirstOrDefault();
                                            if (findparent != null)
                                            {
                                                rownumber = findparent.Row;
                                            }
                                            else
                                            {
                                                var repeat = new RepeatTemplateDto();
                                                repeat.TemplateId = templateID;
                                                repeat.Parent = parent;
                                                repeat.Row = rownumber + 1;
                                                repeatdata.Add(repeat);

                                                rownumber = rownumber + 1;
                                            }
                                        }

                                        var row = worksheet.Row(2).CellsUsed();
                                        var cellvalue = row.Where(x => x.Value.ToString() == variableName).ToList();
                                        var cellnumber = cellvalue[0].Address.ColumnNumber;
                                        if (collectionSource == (int)CollectionSources.DateTime)
                                        {
                                            DateTime dDate;
                                            var variablevalueformat = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].VariableNameValue;
                                            var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                                            worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                                        }
                                        else if (collectionSource == (int)CollectionSources.Date)
                                        {
                                            DateTime dDate;
                                            var variablevalueformat = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].VariableNameValue;
                                            string dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture) : variablevalueformat : "";
                                            worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                                        }
                                        else if (collectionSource == (int)CollectionSources.Time)
                                        {
                                            var variablevalueformat = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].VariableNameValue;
                                            var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";
                                            worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                                        }
                                        else
                                        {
                                            worksheet.Cell(rownumber, cellnumber).SetValue(
                                        d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].VariableNameValue);
                                        }

                                        if (d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].UnitId != null)
                                        {
                                            cellnumber += 1;
                                            m += 1;
                                            totalVariablelen += 1;
                                            worksheet.Cell(rownumber, cellnumber).SetValue(
                                                d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseTemplate[temp].LstProjectDataBaseitems[indexrow].Unit);
                                        }

                                        indexrow++;
                                    }

                                    rownumber++;
                                }
                            }
                        }
                    });
                }
                else if ((filters.FilterId == DBDSReportFilter.DBDS || filters.FilterId == null)
                    && (filters.Type == DbdsReportType.Patient || filters.Type == null))
                {
                    worksheet = workbook.Worksheets.Add("DBDS");

                    worksheet.Rows(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Cell(1, 1).Value = "STUDY CODE";
                    worksheet.Cell(1, 2).Value = "SITE CODE";
                    worksheet.Cell(1, 3).Value = "SCRNUM";
                    worksheet.Cell(1, 4).Value = "RANDNUM";
                    worksheet.Cell(1, 5).Value = "INITIAL";

                    worksheet.Cell(2, 1).Value = "Study Code";
                    worksheet.Cell(2, 2).Value = "Site Code";
                    worksheet.Cell(2, 3).Value = "Screening No";
                    worksheet.Cell(2, 4).Value = "Enrollment No";
                    worksheet.Cell(2, 5).Value = "Patient Initial";

                    int totalVariable = 0;
                    var variable = new List<ProjectDatabaseVariableDto>();
                    var Initial = new List<ProjectDatabaseInitialDto>();
                    var VisitList = new List<ProjectDatabaseVisitDto>();
                    var TemplateList = new List<ProjectDatabaseTemplateDto>();
                    var VariableValueList = new List<ProjectDatabaseItemDto>();
                    var RangeList = new List<RangeOfTemplate>();
                    MainData.Dbds.ForEach(d =>
                    {
                        totalVariable += d.LstVariable.Count;
                        variable.AddRange(d.LstVariable);
                        Initial.AddRange(d.LstProjectDataBase);

                        d.LstProjectDataBase.ForEach(t =>
                        {
                            t.LstProjectDataBaseVisit.ForEach(visit =>
                            {
                                VisitList.Add(visit);
                                visit.LstProjectDataBaseTemplate.OrderBy(s => s.DesignOrder).ToList().ForEach(temp =>
                                {
                                    var TemplateName = temp.TemplateName;
                                    temp.TemplateName = temp.DesignOrder + ". " + TemplateName;
                                    temp.LstProjectDataBaseitems.Where(x => x.ScreeningTemplateParentId == null).ToList().ForEach(v =>
                                    {
                                        v.TemplateName = temp.TemplateName;
                                        v.Visit = visit.Visit;
                                        VariableValueList.Add(v);
                                    });
                                    TemplateList.Add(temp);
                                    var repeatlength = temp.LstProjectDataBaseitems.Where(m => m.ScreeningTemplateParentId != null).GroupBy(x => x.ScreeningTemplateId).ToList().Count;
                                    var repeatorder = 1;
                                    if (repeatlength > 0)
                                    {
                                        for (var m = 0; m < repeatlength; m++)
                                        {
                                            var objData = new ProjectDatabaseTemplateDto();
                                            objData.VisitId = temp.VisitId;
                                            objData.Visit = temp.Visit;
                                            objData.TemplateId = temp.TemplateId;
                                            objData.DesignOrder = temp.DesignOrder;
                                            objData.RepeatSeqNo = temp.RepeatSeqNo;
                                            objData.TemplateName = temp.DesignOrder + "." + repeatorder + " " + TemplateName;
                                            TemplateList.Add(objData);

                                            temp.LstProjectDataBaseitems.Where(q => q.RepeatSeqNo == repeatorder).ToList().ForEach(l =>
                                            {
                                                l.TemplateName = temp.DesignOrder + "." + repeatorder + " " + TemplateName;
                                                l.Visit = visit.Visit;
                                                VariableValueList.Add(l);
                                            });


                                            repeatorder++;
                                        }

                                    }
                                });
                            });
                        });
                    });

                    var j = 3;
                    Initial.GroupBy(x => new { x.SubjectNo, x.Initial }).ToList().ForEach(db =>
                    {
                        worksheet.Row(j).Cell(1).SetValue(db.FirstOrDefault().ProjectCode);
                        worksheet.Row(j).Cell(2).SetValue(db.FirstOrDefault().ParentProjectId != null ? db.FirstOrDefault().ProjectName : "");
                        worksheet.Row(j).Cell(3).SetValue(db.Key.SubjectNo);
                        worksheet.Row(j).Cell(4).SetValue(db.FirstOrDefault().RandomizationNumber);
                        worksheet.Row(j).Cell(5).SetValue(db.Key.Initial);
                        j++;
                    });


                    var visitCell = 6;
                    VisitList.OrderBy(x => x.PeriodId).ThenBy(y => y.VisitDesignOrder).GroupBy(x => x.Visit).ToList().ForEach(vst =>
                    {
                        worksheet.Row(2).Cell(visitCell).SetValue(vst.Key);
                        worksheet.Row(2).Cell(visitCell).Style.Fill.BackgroundColor = XLColor.LightGreen;
                        visitCell++;

                        TemplateList.OrderBy(z => z.VisitId).ThenBy(z => z.DesignOrder).Where(x => x.Visit == vst.Key).GroupBy(x => new { x.TemplateName, x.TemplateId }).ToList().ForEach(temp =>
                        {
                            worksheet.Row(1).Cell(visitCell).SetValue(temp.Key.TemplateName);

                            var FirstCell = visitCell;

                            var ProjectDesignVariableList = _context.ProjectDesignVariable.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == temp.FirstOrDefault().TemplateId).OrderBy(x => x.DesignOrder).ToList();
                            ProjectDesignVariableList.ForEach(variable =>
                            {
                                worksheet.Row(2).Cell(visitCell).SetValue(variable.VariableName);
                                visitCell++;
                            });

                            var ObjRange = new RangeOfTemplate();
                            ObjRange.TemplateId = temp.FirstOrDefault().TemplateId;
                            ObjRange.TemplateName = temp.Key.TemplateName;
                            ObjRange.FirstCell = FirstCell;
                            ObjRange.LastCell = FirstCell + ProjectDesignVariableList.Count() - 1;
                            ObjRange.Visit = vst.Key;
                            RangeList.Add(ObjRange);

                            worksheet.Range(1, FirstCell, 1, FirstCell + ProjectDesignVariableList.Count() - 1).Merge(false);
                        });
                    });

                    VariableValueList.Where(x => x.SubjectNo != null).OrderByDescending(x => x.ScreeningTemplateValueId).ToList().ForEach(x =>
                    {
                        if (x.VariableNameValue != null)
                        {
                            var collectionSource = x.CollectionSource;

                            var cellvalue = worksheet.Row(2).CellsUsed().Where(y => y.Value.ToString() == x.VariableName).ToList();

                            var samevariable = worksheet.CellsUsed(cell => cell.GetString() == x.VariableName).Select(x => x.Address.ColumnNumber).ToList();
                            var IsExist = RangeList.Where(y => y.TemplateName == x.TemplateName && y.Visit == x.Visit).FirstOrDefault();

                            var cellnumber = samevariable.Where(a => a >= IsExist.FirstCell && a <= IsExist.LastCell).FirstOrDefault();
                            var rownumber = worksheet.Column(3).CellsUsed(q => q.GetString() == x.SubjectNo).Select(a => a.Address.RowNumber).FirstOrDefault();

                            if (collectionSource == (int)CollectionSources.DateTime)
                            {
                                DateTime dDate;
                                var variablevalueformat = x.VariableNameValue;
                                var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                                worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                            }
                            else if (collectionSource == (int)CollectionSources.Date)
                            {
                                DateTime dDate;
                                var variablevalueformat = x.VariableNameValue;
                                string dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture) : variablevalueformat : "";
                                worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                            }
                            else if (collectionSource == (int)CollectionSources.Time)
                            {
                                var variablevalueformat = x.VariableNameValue;
                                var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.Parse(variablevalueformat).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";
                                worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                            }
                            else
                            {
                                worksheet.Cell(rownumber, cellnumber).SetValue(x.VariableNameValue);
                            }
                        }
                    });
                }

                if (filters.FilterId == DBDSReportFilter.MedDRA || filters.FilterId == null)
                {
                    worksheet = workbook.Worksheets.Add("MedDRA");

                    worksheet.Rows(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(1, 1).Value = "STUDY CODE";
                    worksheet.Cell(1, 2).Value = "SITE CODE";
                    worksheet.Cell(1, 3).Value = "Domain Code";
                    worksheet.Cell(1, 4).Value = "SCRNUM";
                    worksheet.Cell(1, 5).Value = "RANDNUM";
                    worksheet.Cell(1, 6).Value = "INITIAL";
                    worksheet.Cell(1, 7).Value = "VISIT";
                    worksheet.Cell(1, 8).Value = "Template";
                    worksheet.Cell(1, 9).Value = "Var_Anno";
                    worksheet.Cell(1, 10).Value = "Rep_Term";
                    worksheet.Cell(1, 11).Value = "Llt_code";
                    worksheet.Cell(1, 12).Value = "Llt_name";
                    worksheet.Cell(1, 13).Value = "Llt_curr";
                    worksheet.Cell(1, 14).Value = "Pt_code";
                    worksheet.Cell(1, 15).Value = "Pt_name";
                    worksheet.Cell(1, 16).Value = "Pt_soc_c";
                    worksheet.Cell(1, 17).Value = "Hlt_code";
                    worksheet.Cell(1, 18).Value = "Hlt_name";
                    worksheet.Cell(1, 19).Value = "Hlgt_code";
                    worksheet.Cell(1, 20).Value = "Hlgt_name";
                    worksheet.Cell(1, 21).Value = "Soc_code";
                    worksheet.Cell(1, 22).Value = "Soc_Name";
                    worksheet.Cell(1, 23).Value = "Soc_abbr";
                    worksheet.Cell(1, 24).Value = "Soc_ind";
                    worksheet.Cell(1, 25).Value = "Cod_user";
                    worksheet.Cell(1, 26).Value = "Cod_date";
                    worksheet.Cell(1, 27).Value = "Dict_ver";
                    worksheet.Cell(1, 28).Value = "Dict_lan";


                    worksheet.Cell(2, 1).Value = "Study Code";
                    worksheet.Cell(2, 2).Value = "Site Code";
                    worksheet.Cell(2, 3).Value = "Panel Name";
                    worksheet.Cell(2, 4).Value = "Screening No";
                    worksheet.Cell(2, 5).Value = "Enrollment No";
                    worksheet.Cell(2, 6).Value = "Patient Initial";
                    worksheet.Cell(2, 7).Value = "Visit";
                    worksheet.Cell(2, 8).Value = "Template";
                    worksheet.Cell(2, 9).Value = "Variable Annotation";
                    worksheet.Cell(2, 10).Value = "Reported Term";
                    worksheet.Cell(2, 11).Value = "llt_code";
                    worksheet.Cell(2, 12).Value = "llt_name";
                    worksheet.Cell(2, 13).Value = "llt_currency";
                    worksheet.Cell(2, 14).Value = "pt_code";
                    worksheet.Cell(2, 15).Value = "pt_name";
                    worksheet.Cell(2, 16).Value = "pt_soc_code";
                    worksheet.Cell(2, 17).Value = "hlt_code";
                    worksheet.Cell(2, 18).Value = "hlt_name";
                    worksheet.Cell(2, 19).Value = "hlgt_code";
                    worksheet.Cell(2, 20).Value = "hlgt_name";
                    worksheet.Cell(2, 21).Value = "soc_Code";
                    worksheet.Cell(2, 22).Value = "soc_name";
                    worksheet.Cell(2, 23).Value = "soc_abbrev";
                    worksheet.Cell(2, 24).Value = "Soc Indicator";
                    worksheet.Cell(2, 25).Value = "Coded by user";
                    worksheet.Cell(2, 26).Value = "Coded on date (UTC)";
                    worksheet.Cell(2, 27).Value = "Version";
                    worksheet.Cell(2, 28).Value = "Language";

                    var mrownumber = 3;
                    MainData.Meddra.ForEach(m =>
                    {
                        worksheet.Cell(mrownumber, 1).SetValue(m.ProjectCode);
                        worksheet.Cell(mrownumber, 2).SetValue(m.SiteCode);
                        worksheet.Cell(mrownumber, 3).SetValue(m.DomainCode);
                        worksheet.Cell(mrownumber, 4).SetValue(m.ScreeningNumber);
                        worksheet.Cell(mrownumber, 5).SetValue(m.RandomizationNumber);
                        worksheet.Cell(mrownumber, 6).SetValue(m.Initial);
                        worksheet.Cell(mrownumber, 7).SetValue(m.Visit);
                        worksheet.Cell(mrownumber, 8).SetValue(m.TemplateName);
                        worksheet.Cell(mrownumber, 9).SetValue(m.VariableAnnotation);
                        worksheet.Cell(mrownumber, 10).SetValue(m.VariableTerm);
                        worksheet.Cell(mrownumber, 11).SetValue(m.LltCode);
                        worksheet.Cell(mrownumber, 12).SetValue(m.LltName);
                        worksheet.Cell(mrownumber, 13).SetValue(m.LltCurrency);
                        worksheet.Cell(mrownumber, 14).SetValue(m.PtCode);
                        worksheet.Cell(mrownumber, 15).SetValue(m.PtName);
                        worksheet.Cell(mrownumber, 16).SetValue(m.PtSocCode);
                        worksheet.Cell(mrownumber, 17).SetValue(m.HltCode);
                        worksheet.Cell(mrownumber, 18).SetValue(m.HltName);
                        worksheet.Cell(mrownumber, 19).SetValue(m.HlgtCode);
                        worksheet.Cell(mrownumber, 20).SetValue(m.HlgtName);
                        worksheet.Cell(mrownumber, 21).SetValue(m.SocCode);
                        worksheet.Cell(mrownumber, 22).SetValue(m.SocName);
                        worksheet.Cell(mrownumber, 23).SetValue(m.SocAbbrev);
                        worksheet.Cell(mrownumber, 24).SetValue(m.PrimaryIndicator);
                        worksheet.Cell(mrownumber, 25).SetValue(m.CodedBy);

                        DateTime dDate;
                        var dt = !string.IsNullOrEmpty(m.CodedOn.ToString()) ? DateTime.TryParse(m.CodedOn.ToString(), out dDate) ? DateTime.Parse(m.CodedOn.ToString()).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : m.CodedOn.ToString() : "";
                        //var dt = DateTime.Parse(m.CodedOn.ToString()).AddHours(5).AddMinutes(30).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat);

                        worksheet.Cell(mrownumber, 26).Value = dt;
                        worksheet.Cell(mrownumber, 27).SetValue(m.Version);
                        worksheet.Cell(mrownumber, 28).Value = m.Language;
                        //worksheet.Cell(mrownumber, 28).Value = this.datetimeformat.transform(new Date(m.codedOn).toLocaleString());

                        mrownumber++;
                    });
                }

                string path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.DBDSReport.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    stream.Position = 0;
                    var FileName = filters.ExcelFormat ? "DBDS_" + DateTime.Now.Ticks + ".xlsx" : "DBDS_" + DateTime.Now.Ticks + ".csv";
                    var FilePath = Path.Combine(path, FileName);
                    workbook.SaveAs(FilePath);

                    #region Update Job Status
                    var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                    string savepath = Path.Combine(documentUrl, FolderType.DBDSReport.ToString());
                    jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
                    jobMonitoring.JobStatus = JobStatusType.Completed;
                    jobMonitoring.FolderPath = savepath;
                    jobMonitoring.FolderName = FileName;
                    _jobMonitoringRepository.Update(jobMonitoring);
                    _context.Save();
                    #endregion


                    #region EmailSend
                    var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                    var ProjectName = _context.Project.Find(filters.SelectedProject).ProjectCode + "-" + _context.Project.Find(filters.SelectedProject).ProjectName;
                    string pathofdoc = Path.Combine(savepath, FileName);
                    var linkOfDoc = "<a href='" + pathofdoc + "'>Click Here</a>";
                    _emailSenderRespository.SendDBDSGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfDoc);
                    #endregion
                }
            }
            #endregion

        }

        public List<ScreeningVariableValueDto> GetScreeningRelation(int projectDesignVariableId, int screeningEntryId)
        {
            var result = All.Where(x => x.DeletedDate == null &&
                                   x.ScreeningTemplate.DeletedDate == null &&
                                   x.ScreeningTemplate.ScreeningVisit.DeletedDate == null &&
                                   x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == screeningEntryId &&
                                   x.ProjectDesignVariableId == projectDesignVariableId).OrderBy(o => o.CreatedDate)
                .Select(c => new
                {
                    c.Id,
                    c.ProjectDesignVariableId,
                    ValueName = c.Value,
                    c.ProjectDesignVariable.CollectionSource,
                    SeqNo = c.ScreeningTemplate.RepeatSeqNo == null ? c.ScreeningTemplate.ProjectDesignTemplate.DesignOrder.ToString() + ".0" : c.ScreeningTemplate.ProjectDesignTemplate.DesignOrder.ToString() + "." + c.ScreeningTemplate.RepeatSeqNo.Value.ToString()
                }).ToList();

            var relations = new List<ScreeningVariableValueDto>();

            var collectionSource = result.FirstOrDefault()?.CollectionSource;
            var timeformat = "";
            var dateformat = "";
            if (collectionSource != null && (collectionSource == CollectionSources.DateTime || collectionSource == CollectionSources.Date || collectionSource == CollectionSources.Time))
            {

                timeformat = _context.AppSetting.Where(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.KeyName == "GeneralSettingsDto.TimeFormat").Select(t => t.KeyValue).FirstOrDefault();
                dateformat = _context.AppSetting.Where(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.KeyName == "GeneralSettingsDto.DateFormat").Select(t => t.KeyValue).FirstOrDefault();

            }

            result.ForEach(x =>
            {
                var relationValue = new ScreeningVariableValueDto();
                relationValue.Id = x.Id;
                relationValue.ProjectDesignVariableId = x.ProjectDesignVariableId;
                relationValue.ValueName = x.SeqNo + "- " + x.ValueName;
                if (x.CollectionSource.IsDropDownCollection())
                {
                    int.TryParse(x.ValueName, out int projectDesignVariableValueId);
                    relationValue.ValueName = x.SeqNo + "- " + _context.ProjectDesignVariableValue.Where(b => b.Id == projectDesignVariableValueId).Select(
                        t => _jwtTokenAccesser.Language != 1 ? t.VariableValueLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && c.DeletedDate == null && c.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : t.ValueName).FirstOrDefault();

                }
                else if (collectionSource == CollectionSources.Date && !string.IsNullOrEmpty(x.ValueName) && !string.IsNullOrEmpty(dateformat))
                {
                    relationValue.ValueName = x.SeqNo + "- " + Convert.ToDateTime(x.ValueName).ToString(dateformat);
                }
                else if ((collectionSource == CollectionSources.Time || collectionSource == CollectionSources.DateTime) && !string.IsNullOrEmpty(x.ValueName) && !string.IsNullOrEmpty(dateformat))
                {
                    relationValue.ValueName = x.SeqNo + "- " + Convert.ToDateTime(x.ValueName).ToString(dateformat + " " + timeformat);
                }
                relations.Add(relationValue);
            });


            return relations;
        }


        public DesignScreeningVariableDto GetQueryVariableDetail(int id, int screeningEntryId)
        {
            DesignScreeningVariableDto variableDetail = null;

            var projectDesignId = _context.ScreeningEntry.Find(screeningEntryId).ProjectDesignId;
            var sequenseDeatils = _templateVariableSequenceNoSettingRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();


            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            var screeningValue = All.AsNoTracking().Where(t => t.Id == id)
                   .ProjectTo<Data.Dto.Screening.ScreeningTemplateValueBasic>(_mapper.ConfigurationProvider).FirstOrDefault();
            if (screeningValue != null)
            {
                variableDetail = _context.ProjectDesignVariable.Where(t => t.Id == screeningValue.ProjectDesignVariableId)
                    .Select(x => new DesignScreeningVariableDto
                    {
                        ProjectDesignTemplateId = x.ProjectDesignTemplateId,
                        ProjectDesignVariableId = x.Id,
                        Id = x.Id,
                        VariableName = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && c.DeletedDate == null && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.VariableName),
                        VariableCode = x.VariableCode,
                        CollectionSource = x.CollectionSource,
                        ValidationType = x.ValidationType,
                        DataType = x.DataType,
                        Length = x.Length,
                        DefaultValue = string.IsNullOrEmpty(x.DefaultValue) && x.CollectionSource == CollectionSources.HorizontalScale ? "1" : x.DefaultValue,
                        LargeStep = x.LargeStep,
                        LowRangeValue = x.LowRangeValue,
                        HighRangeValue = x.HighRangeValue,
                        RelationProjectDesignVariableId = x.RelationProjectDesignVariableId,
                        PrintType = x.PrintType,
                        UnitName = x.Unit.UnitName,
                        DesignOrder = sequenseDeatils.IsVariableSeqNo == true ? x.DesignOrder.ToString() : "",
                        IsDocument = x.IsDocument,
                        VariableCategoryName = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableCategory.VariableCategoryLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null && c.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.VariableCategory.CategoryName) ?? "",
                        SystemType = x.SystemType,
                        IsNa = x.IsNa,
                        DateValidate = x.DateValidate,
                        Alignment = x.Alignment ?? Alignment.Right,
                        Note = (_jwtTokenAccesser.Language != 1 ?
                        x.VariableNoteLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null && c.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : x.Note),
                        ValidationMessage = x.ValidationType == ValidationType.Required ? "This field is required" : "",
                        Values = x.Values.Where(x => x.DeletedDate == null).Select(c => new ScreeningVariableValueDto
                        {
                            Id = c.Id,
                            ProjectDesignVariableId = c.ProjectDesignVariableId,
                            ValueName = _jwtTokenAccesser.Language != 1 ? c.VariableValueLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && c.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : c.ValueName,
                            SeqNo = c.SeqNo,
                            Label = _jwtTokenAccesser.Language != 1 ? c.VariableValueLanguage.Where(c => c.LanguageId == _jwtTokenAccesser.Language && c.DeletedDate == null).Select(a => a.LabelName).FirstOrDefault() : c.Label,
                        }).ToList()

                    }).FirstOrDefault();
            }

            if (variableDetail != null)
            {
                if (variableDetail.CollectionSource == CollectionSources.Relation && variableDetail.RelationProjectDesignVariableId > 0)
                    variableDetail.Values = GetScreeningRelation(variableDetail.RelationProjectDesignVariableId ?? 0, screeningEntryId);

                variableDetail.ScreeningValue = screeningValue.Value;
                variableDetail.ScreeningValueOld = screeningValue.IsNa ? "N/A" : screeningValue.Value;
                variableDetail.ScreeningTemplateValueId = screeningValue.Id;
                variableDetail.ScheduleDate = screeningValue.ScheduleDate;
                variableDetail.QueryStatus = screeningValue.QueryStatus;
                variableDetail.IsNaValue = screeningValue.IsNa;
                variableDetail.IsSystem = screeningValue.QueryStatus == QueryStatus.Closed ? false : screeningValue.IsSystem;


                variableDetail.DocPath = screeningValue.DocPath != null ? screeningValue.DocPath : null;
                variableDetail.DocFullPath = screeningValue.DocPath != null ? documentUrl + screeningValue.DocPath : null;


                if (variableDetail.Values != null && (variableDetail.CollectionSource == CollectionSources.CheckBox || variableDetail.CollectionSource == CollectionSources.MultiCheckBox))
                    variableDetail.Values.ToList().ForEach(val =>
                    {
                        var childValue = screeningValue.Children.FirstOrDefault(v => v.ProjectDesignVariableValueId == val.Id);
                        if (childValue != null)
                        {
                            val.ScreeningValue = childValue.Value;
                            val.ScreeningValueOld = childValue.Value;
                            val.ScreeningTemplateValueChildId = childValue.Id;
                        }
                    });


            }

            return variableDetail;
        }

        public void DeleteRepeatTemplateValue(int Id)
        {
            var values = All.Where(t => t.ScreeningTemplateId == Id).ToList();
            if (values != null)
            {
                values.ForEach(x =>
                {
                    var record = Find(x.Id);
                    Delete(record);
                });
            }
        }

        public bool IsEligible(int VolunteerId)
        {
            return All.Any(x => x.DeletedDate == null &&
                                x.ProjectDesignVariable.Annotation == "DCP" &&
                                x.ProjectDesignVariable.Values != null &&
                                x.ProjectDesignVariable.ProjectDesignTemplate.Domain.DomainCode == "EC01" &&
                                x.ProjectDesignVariable.Values.Any(r => r.ValueCode == "01")
                                && x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == VolunteerId);
        }

        public void UpdateDefaultValueForDosing(IList<DesignScreeningVariableDto> variableList, int screeningTemplateId, bool IsDosing)
        {
            var screeningDesignVariableId = All.Where(x => x.ScreeningTemplateId == screeningTemplateId).Select(r => r.ProjectDesignVariableId).ToList();
            //if (screeningDesignVariableId != null && screeningDesignVariableId.Count > 0)
            //    return;

            var templateVariable = variableList.ToList();

            // Save date for the default
            if (templateVariable.Count != 0)
            {
                //var screeningDesignVariable = All.Where(x => x.ScreeningTemplateId == screeningTemplateId && x.ProjectDesignVariableId == variableList[0].Id)
                //    .Select(r => r.Id).FirstOrDefault();

                //if (variableList[0].CollectionSource == CollectionSources.DateTime || variableList[0].CollectionSource == CollectionSources.Date || variableList[0].CollectionSource == CollectionSources.Time)
                //{
                //    var screeningTemplateValue = new ScreeningTemplateValue
                //    {
                //        ScreeningTemplateId = screeningTemplateId,
                //        ProjectDesignVariableId = variableList[0].Id,
                //        Value = _jwtTokenAccesser.GetClientDate().ToString("MM/dd/yyyy HH:mm:ss"),
                //    };

                //    if (screeningDesignVariable == 0)
                //        Add(screeningTemplateValue);
                //    else
                //    {
                //        screeningTemplateValue.Id = screeningDesignVariable;
                //        Update(screeningTemplateValue);
                //    }
                //    //Add(screeningTemplateValue);

                //    var audit = new ScreeningTemplateValueAudit
                //    {
                //        ScreeningTemplateValue = screeningTemplateValue,
                //        Value = null,
                //        OldValue = null,
                //        Note = "Submitted with actual date by scan barcode."
                //    };
                //    _screeningTemplateValueAuditRepository.Save(audit);
                //}
                if (IsDosing)
                {
                    var TemplateVariableProductType = variableList.Where(r => r.VariableCode == "PT").FirstOrDefault();

                    if (TemplateVariableProductType != null)
                    {
                        var visitID = _context.ScreeningTemplate
                            .Include(c => c.ScreeningVisit)
                            .Where(x => x.Id == screeningTemplateId).Select(r => r.ScreeningVisit.ProjectDesignVisitId).FirstOrDefault();
                        if (visitID != null)
                        {
                            var RandomizationNumber = _context.ScreeningTemplate
                                .Include(c => c.ScreeningVisit)
                                .ThenInclude(c => c.ScreeningEntry)
                                .ThenInclude(c => c.Attendance)
                                .ThenInclude(c => c.Volunteer)
                                .Where(x => x.Id == screeningTemplateId)
                                .Select(r => r.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.RandomizationNumber).FirstOrDefault();
                            if (RandomizationNumber != null)
                            {
                                var randomization = _context.SupplyManagementUploadFileVisit.Include(x => x.SupplyManagementUploadFileDetail)
                                .Where(q => q.ProjectDesignVisitId == visitID &&
                                    q.SupplyManagementUploadFileDetail.RandomizationNo == Convert.ToInt32(RandomizationNumber)).FirstOrDefault();

                                var content = randomization.SupplyManagementUploadFileDetail.TreatmentType;

                                var screeningDesignVariablePT = All.Where(x => x.ScreeningTemplateId == screeningTemplateId && x.ProjectDesignVariableId == TemplateVariableProductType.Id)
                    .Select(r => r.Id).FirstOrDefault();

                                var screeningTValue = new ScreeningTemplateValue
                                {
                                    ScreeningTemplateId = screeningTemplateId,
                                    ProjectDesignVariableId = TemplateVariableProductType.Id,
                                    Value = content,
                                };

                                if (screeningDesignVariablePT == 0)
                                    Add(screeningTValue);
                                else
                                {
                                    screeningTValue.Id = screeningDesignVariablePT;
                                    Update(screeningTValue);
                                }
                                //  Add(screeningTValue);

                                var saudit = new ScreeningTemplateValueAudit
                                {
                                    ScreeningTemplateValue = screeningTValue,
                                    Value = null,
                                    OldValue = null,
                                    Note = "Submitted with product type by scan barcode."
                                };
                                _screeningTemplateValueAuditRepository.Save(saudit);
                            }
                        }
                    }
                }
            }
            _context.Save();
        }
    }
}