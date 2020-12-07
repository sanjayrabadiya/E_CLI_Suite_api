using ClosedXML.Excel;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Project.Design;
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
        public ScreeningTemplateValueRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignVariableRepository projectDesignVariableRepository, IAppSettingRepository appSettingRepository,
            IJobMonitoringRepository jobMonitoringRepository,
            IUserRepository userRepository, IEmailSenderRespository emailSenderRespository,
            IUploadSettingRepository uploadSettingRepository,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository)
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
        }

        public void UpdateVariableOnSubmit(int projectDesignTemplateId, int screeningTemplateId,
            List<int> projectDesignVariableId)
        {
            var screeningVariable =
                All.Where(x => x.ScreeningTemplateId == screeningTemplateId).AsNoTracking().ToList();

            var templateVariable = _projectDesignVariableRepository
                .FindBy(t => t.ProjectDesignTemplateId == projectDesignTemplateId).ToList()
                .Where(x => !screeningVariable.Any(a => a.ProjectDesignVariableId == x.Id)).ToList();

            foreach (var variable in templateVariable)
            {
                if (projectDesignVariableId != null && projectDesignVariableId.Any(c => c == variable.Id))
                    continue;

                var screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplateId,
                    ProjectDesignVariableId = variable.Id,
                    Value = variable.DefaultValue,

                };
                Add(screeningTemplateValue);


                var audit = new ScreeningTemplateValueAudit
                {
                    ScreeningTemplateValue= screeningTemplateValue,
                    Value = string.IsNullOrEmpty(variable.DefaultValue) ? "" : variable.DefaultValue,
                    OldValue = null,
                    Note = "Submitted with default data",
                    UserId = _jwtTokenAccesser.UserId,
                    UserRoleId = _jwtTokenAccesser.RoleId
                };
                _screeningTemplateValueAuditRepository.Add(audit);
            }
        }

        public QueryStatusDto GetQueryStatusCount(int screeningTemplateId)
        {
            var result = All.Where(x => x.DeletedDate == null
                                        && x.ProjectDesignVariable.DeletedDate == null
                                        && x.ScreeningTemplateId == screeningTemplateId).
                                        Select(r => new
                                        Data.Dto.Screening.ScreeningTemplateValueBasic
                                        {
                                            ScreeningTemplateId = r.ScreeningTemplateId,
                                            QueryStatus = r.QueryStatus,
                                            AcknowledgeLevel = r.AcknowledgeLevel
                                        }).ToList();
            if (result != null) return GetQueryStatusByModel(result, screeningTemplateId);

            return null;
        }

        public QueryStatusDto GetQueryStatusByModel(List<Data.Dto.Screening.ScreeningTemplateValueBasic> screeningTemplateValue,
            int screeningTemplateId)
        {
            if (screeningTemplateValue == null) return null;

            var result = screeningTemplateValue.Where(x => x.ScreeningTemplateId == screeningTemplateId
            && x.QueryStatus != QueryStatus.Closed && x.QueryStatus != null).ToList();
            if (result != null && result.Count > 0)
            {
                var queryStatusDto = new QueryStatusDto();
                queryStatusDto.Items = result.GroupBy(r => new { r.QueryStatus, r.AcknowledgeLevel }).Select(t => new QueryStatusCount
                {
                    QueryStatus = ((QueryStatus)t.Key.QueryStatus).GetDescription(),
                    ReviewLevel = t.Key.AcknowledgeLevel == null ? -1 : t.Key.AcknowledgeLevel,
                    Total = t.Count()
                }).ToList();
                queryStatusDto.TotalQuery = queryStatusDto.Items.Sum(x => x.Total);
                return queryStatusDto;
            }

            return null;
        }

        public List<QueryStatusDto> GetQueryStatusBySubject(int screeningEntryId)
        {
            var result = All.Where(x => x.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == screeningEntryId &&
                                                           x.QueryStatus != QueryStatus.Closed && x.QueryStatus != null)
                .Select(r => new { r.ScreeningTemplateId, r.QueryStatus, r.AcknowledgeLevel }).ToList();

            if (result != null && result.Count > 0)
            {
                var queryStatusDto = result.GroupBy(x => x.ScreeningTemplateId).
                    Select(a => new QueryStatusDto
                    {
                        ScreeningTemplateId = a.Key,
                        TotalQuery = a.Count(),
                        Items = result.Where(r => r.ScreeningTemplateId == a.Key).GroupBy(r => new { r.AcknowledgeLevel, r.QueryStatus }).
                        Select(t => new QueryStatusCount
                        {
                            QueryStatus = ((QueryStatus)t.Key.QueryStatus).GetDescription(),
                            ReviewLevel = t.Key.AcknowledgeLevel == null ? -1 : t.Key.AcknowledgeLevel,
                            Total = t.Count()
                        }).ToList()
                    }).ToList();

                return queryStatusDto;
            }

            return null;
        }

        public List<PeriodQueryStatusDto> GetQueryStatusByPeridId(int projectDesignPeriodId)
        {
            return All.Where(x => x.DeletedDate == null &&
                                  x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectDesignPeriodId == projectDesignPeriodId
                                  && x.QueryStatus != null).Select(r =>
            new PeriodQueryStatusDto
            {
                ScreeningEntryId = r.ScreeningTemplate.ScreeningVisit.ScreeningEntryId,
                AcknowledgeLevel = r.AcknowledgeLevel,
                QueryStatus = r.QueryStatus
            }).ToList();
        }


        public int GetQueryCountByVisitId(int screeningVisitId)
        {
            return All.Count(x => x.DeletedDate == null &&
                                  x.ScreeningTemplate.ScreeningVisitId == screeningVisitId
                                  && x.QueryStatus != null && x.QueryStatus != QueryStatus.Closed);
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

        public List<DashboardQueryStatusDto> GetQueryByProjectDesignId(int projectDesignId)
        {


            return All.Where(x => x.DeletedDate == null &&
                                  x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectDesignId == projectDesignId
                                  && x.QueryStatus != null)
                .GroupBy(c => new { c.ScreeningTemplate.ScreeningVisit.ScreeningEntryId, c.QueryStatus }).Select(r =>
                      new DashboardQueryStatusDto
                      {
                          ScreeningEntryId = r.Key.ScreeningEntryId,
                          Status = r.Key.QueryStatus,
                          Total = r.Count()
                      }).ToList();
        }

        public void GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters)
        {

            var ProjectCode = _context.Project.Find(filters.ParentProjectId).ProjectCode;
            var GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(1);
            GeneralSettings.TimeFormat = GeneralSettings.TimeFormat.Replace("a", "tt");

            CommonDto MainData = new CommonDto();

            #region Job Monitoring Save - Inprocess Status
            JobMonitoring jobMonitoring = new JobMonitoring();
            jobMonitoring.JobName = JobNameType.DBDSReport;
            jobMonitoring.JobDescription = filters.SelectedProject;
            jobMonitoring.JobType = filters.ExcelFormat ? JobTypeEnum.Excel : JobTypeEnum.Csv;
            jobMonitoring.JobStatus = JobStatusType.InProcess;
            jobMonitoring.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoring.SubmittedTime = DateTime.Now.UtcDateTime();
            _jobMonitoringRepository.Add(jobMonitoring);

            _context.Save();
            #endregion

            if (filters.FilterId == DBDSReportFilter.DBDS || filters.FilterId == null)
            {

                #region Main Query
                var queryDtos = (from screening in _context.ScreeningEntry.Where(t => filters.ProjectId.Contains(t.ProjectId)
                                 && (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId))
                                 && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.Id))
                                 && t.DeletedDate == null)
                                 join template in _context.ScreeningTemplate.Where(u => (filters.TemplateIds == null
                                     || filters.TemplateIds.Contains(u.ProjectDesignTemplateId))
                                     && (filters.VisitIds == null || filters.VisitIds.Contains(u.ProjectDesignTemplate.ProjectDesignVisitId))
                                     && (filters.DomainIds == null || filters.DomainIds.Contains(u.ProjectDesignTemplate.DomainId))
                                     && u.DeletedDate == null)
                                 on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                                 join valueTemp in _context.ScreeningTemplateValue.Where(val => val.DeletedDate == null
                                     && val.ProjectDesignVariable.DeletedDate == null)
                                 on new { template.Id, template.ProjectDesignTemplateId } equals new { Id = valueTemp.ScreeningTemplateId, valueTemp.ProjectDesignVariable.ProjectDesignTemplateId }
                                 into valueDto
                                 from value in valueDto.DefaultIfEmpty()
                                 join randomizationTemp in _context.Randomization on screening.RandomizationId equals randomizationTemp.Id into randomizationDto
                                 from randomization in randomizationDto.DefaultIfEmpty()
                                 select new ProjectDatabaseDto
                                 {
                                     ScreeningEntryId = screening.Id,
                                     ScreeningTemplateId = template.Id,
                                     ScreeningTemplateParentId = template.ParentId,
                                     ProjectId = screening.ProjectId,
                                     ProjectCode = ProjectCode,
                                     ParentProjectId = screening.Project.ParentProjectId,
                                     ProjectName = screening.Project.ProjectCode,
                                     DesignOrder = template.ProjectDesignTemplate.DesignOrder,
                                     DesignOrderOfVariable = value == null ? 0 : value.ProjectDesignVariable.DesignOrder,
                                     TemplateId = template.ProjectDesignTemplateId,
                                     TemplateName = template.ProjectDesignTemplate.TemplateName,
                                     DomainName = template.ProjectDesignTemplate.Domain.DomainName,
                                     DomainId = template.ProjectDesignTemplate.DomainId,
                                     VisitId = template.ScreeningVisit.ProjectDesignVisitId,
                                     RepeatedVisit = template.ScreeningVisit.RepeatedVisitNumber,
                                     Visit = template.ScreeningVisit.ProjectDesignVisit.DisplayName +
                                          Convert.ToString(template.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + template.ScreeningVisit.RepeatedVisitNumber),
                                     VariableName = value == null ? null : value.ProjectDesignVariable.VariableName,
                                     VariableId = value == null ? 0 : value.ProjectDesignVariableId,
                                     Annotation = value == null ? null : value.ProjectDesignVariable.Annotation,
                                     UnitId = value == null ? 0 : value.ProjectDesignVariable.UnitId,
                                     Unit = value == null ? null : value.ProjectDesignVariable.Unit.UnitName,
                                     UnitAnnotation = value == null ? null : value.ProjectDesignVariable.UnitAnnotation,
                                     VariableUnit = value == null ? null : value.ProjectDesignVariable.Unit.UnitName == null ? "" : value.ProjectDesignVariable.Unit.UnitName,
                                     CollectionSource = value == null ? 0 : (int)value.ProjectDesignVariable.CollectionSource,
                                     VariableNameValue = value == null ? null
                                          : value.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox
                                          ? string.Join(";", from stvc in _context.ScreeningTemplateValueChild.Where(x =>
                                                  x.DeletedDate == null && x.ScreeningTemplateValueId == value.Id && x.Value == "true")
                                                             join prpjectdesignvalueTemp in _context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null)
                                                             on stvc.ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into prpjectdesignvalueDto
                                                             from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                                                             select prpjectdesignvalue.ValueName)
                                          : value.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox &&
                                            !string.IsNullOrEmpty(value.Value)
                                              ? _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                                  b.ProjectDesignVariableId == value.ProjectDesignVariable.Id).ValueName
                                          : value.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox &&
                                            value.IsNa && string.IsNullOrEmpty(value.Value)
                                              ? "NA"
                                           : value.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                                                value.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton
                                                  ? _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                                      b.ProjectDesignVariableId == value.ProjectDesignVariable.Id &&
                                                      b.Id == Convert.ToInt32(value.Value)).ValueName
                                           : value.Value,
                                     Initial = screening.RandomizationId != null ? randomization.Initial : screening.Attendance.Volunteer.AliasName,
                                     SubjectNo = screening.RandomizationId != null ? randomization.ScreeningNumber : screening.Attendance.Volunteer.VolunteerNo,
                                     RandomizationNumber = screening.RandomizationId != null ? randomization.RandomizationNumber : "",
                                 }).ToList();
                #endregion

                var grpquery = queryDtos.OrderBy(d => d.VisitId).ThenBy(x => x.DesignOrder).GroupBy(x => new { x.DomainName, x.DomainId }).Select(y => new ProjectDatabaseDomainDto
                {
                    DomainName = y.Key.DomainName,
                    TemplateId = y.FirstOrDefault().TemplateId,
                    DesignOrder = y.FirstOrDefault().DesignOrder,
                    LstVariable = y.Where(v => v.VariableName != null).ToList().Count > 0 ? y.Where(q => q.DomainId == y.Key.DomainId && q.VariableName != null).GroupBy(vari => vari.VariableName).Select(v =>
                        new ProjectDatabaseVariableDto
                        {
                            VariableName = v.Key,
                            Annotation = v.FirstOrDefault().Annotation,
                            UnitId = v.FirstOrDefault().UnitId,
                            Unit = v.FirstOrDefault().Unit,
                            UnitAnnotation = v.FirstOrDefault().UnitAnnotation,
                            DesignOrderOfVariable = v.FirstOrDefault().DesignOrderOfVariable,
                            TemplateId = v.FirstOrDefault().TemplateId
                        }).OrderBy(o => o.TemplateId).ThenBy(d => d.DesignOrderOfVariable).ToList()
                        : _context.ProjectDesignVariable.Where(v => v.DeletedDate == null
                        && v.ProjectDesignTemplateId == y.FirstOrDefault().TemplateId).Select(x => new ProjectDatabaseVariableDto
                        {
                            VariableName = x.VariableName,
                            Annotation = x.Annotation,
                            UnitId = x.UnitId,
                            Unit = x.Unit.UnitName,
                            UnitAnnotation = x.UnitAnnotation,
                            DesignOrderOfVariable = x.DesignOrder,
                            TemplateId = x.ProjectDesignTemplateId
                        }).OrderBy(o => o.TemplateId).ThenBy(d => d.DesignOrderOfVariable).ToList(),

                    LstProjectDataBase = y.Where(v => v.VariableName != null).GroupBy(x => new { x.Initial, x.SubjectNo }).Select(s => new ProjectDatabaseInitialDto
                    {
                        Initial = s.Key.Initial,
                        ProjectId = s.FirstOrDefault().ProjectId,
                        ProjectCode = s.FirstOrDefault().ProjectCode,
                        ParentProjectId = s.FirstOrDefault().ParentProjectId,
                        ProjectName = s.FirstOrDefault().ProjectName,
                        SubjectNo = s.Key.SubjectNo,
                        RandomizationNumber = s.FirstOrDefault().RandomizationNumber,
                        LstProjectDataBaseVisit = s.GroupBy(vst => vst.Visit).Select(n => new ProjectDatabaseVisitDto
                        {
                            Visit = n.Key,
                            DesignOrder = n.FirstOrDefault().DesignOrder,
                            TemplateName = n.FirstOrDefault().TemplateName,
                            LstProjectDataBaseitems = n.OrderBy(o => o.ScreeningTemplateId).Select(i => new ProjectDatabaseItemDto
                            {
                                ScreeningTemplateParentId = i.ScreeningTemplateParentId,
                                VariableName = i.VariableName,
                                ScreeningTemplateId = i.ScreeningTemplateId,
                                CollectionSource = i.CollectionSource,
                                VariableNameValue = i.VariableNameValue,
                                UnitId = i.UnitId,
                                Unit = i.Unit,
                            }).ToList()
                        }).ToList()
                    }).OrderBy(p => p.ProjectId).ThenBy(x => x.SubjectNo).ToList()
                }).ToList();

                MainData.Dbds = grpquery;
            }

            if (filters.FilterId == DBDSReportFilter.MedDRA || filters.FilterId == null)
            {
                #region Medra

                var MeddraDetails = (from se in _context.ScreeningEntry.Where(t =>
                //filter from front
                           filters.ProjectId.Contains(t.ProjectId) &&
                           (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId))
                           && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.Id)) &&
                //end filter
                           t.DeletedDate == null)
                                     join project in _context.Project.Where(x => filters.ProjectId.Contains(x.Id)) on se.ProjectId equals project.Id
                                     join st in _context.ScreeningTemplate.Where(t => t.DeletedDate == null &&
                                     //filter from report page
                                         (filters.TemplateIds == null || filters.TemplateIds.Contains(t.ProjectDesignTemplateId))
                                         && (filters.VisitIds == null ||
                                             filters.VisitIds.Contains(t.ProjectDesignTemplate.ProjectDesignVisitId)) &&
                                         (filters.DomainIds == null ||
                                          filters.DomainIds.Contains(t.ProjectDesignTemplate.DomainId))
                                     // end filter
                                     && t.Status != ScreeningTemplateStatus.Pending && t.Status != ScreeningTemplateStatus.InProcess) on se.Id equals st.ScreeningVisit.ScreeningEntryId
                                     join pt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                                     join visit in _context.ProjectDesignVisit on pt.ProjectDesignVisitId equals visit.Id
                                     join pdv in _context.ProjectDesignVariable.Where(val => val.DeletedDate == null) on new { Id1 = pt.Id } equals new { Id1 = pdv.ProjectDesignTemplateId }
                                     join value in _context.ScreeningTemplateValue.Where(val => val.DeletedDate == null) on new
                                     { Id = st.Id, Id1 = pdv.Id } equals new
                                     { Id = value.ScreeningTemplateId, Id1 = value.ProjectDesignVariableId }
                                     join sc in _context.StudyScoping on pdv.Id equals sc.ProjectDesignVariableId
                                     //join attendance in _context.Attendance.Where(t => t.DeletedDate == null)
                                     //on se.AttendanceId equals attendance.Id
                                     //join volunteerTemp in _context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                                     //from volunteer in volunteerDto.DefaultIfEmpty()
                                     //join noneregisterTemp in _context.NoneRegister.Where(t => t.DeletedDate == null && t.RandomizationNumber != null) on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                                     //from nonregister in noneregisterDto.DefaultIfEmpty()
                                     //join projectSubjectTemp in _context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                                     //from projectsubject in projectsubjectDto.DefaultIfEmpty()
                                     join randomizationTemp in _context.Randomization on st.ScreeningVisit.ScreeningEntry.RandomizationId equals randomizationTemp.Id into randomizationDto
                                     from randomization in randomizationDto.DefaultIfEmpty()
                                     join medraCoding in _context.MeddraCoding.Where(t => t.DeletedDate == null) on value.Id equals medraCoding.ScreeningTemplateValueId into medraDto
                                     from meddraCoding in medraDto.DefaultIfEmpty()
                                     join medraConfig in _context.MedraConfig.Where(t => t.DeletedDate == null) on meddraCoding.MeddraConfigId equals medraConfig.Id into meddraConfigdto
                                     from medraConfig in meddraConfigdto.DefaultIfEmpty()
                                     join soc in _context.MeddraSocTerm on meddraCoding.MeddraSocTermId equals soc.Id into socDto
                                     from meddraSoc in socDto.DefaultIfEmpty()
                                     join mllt in _context.MeddraLowLevelTerm on meddraCoding.MeddraLowLevelTermId equals mllt.Id into mlltDto
                                     from meddraLLT in mlltDto.DefaultIfEmpty()
                                     join md in _context.MeddraMdHierarchy.Where(t => t.DeletedDate == null)
                                     on meddraSoc.soc_code equals md.soc_code into mdDto
                                     from meddraMD in mdDto.DefaultIfEmpty()
                                     join users in _context.Users on meddraCoding.ModifiedBy equals users.Id into userDto
                                     from user in userDto.DefaultIfEmpty()
                                     join roles in _context.SecurityRole on meddraCoding.CreatedRole equals roles.Id into roleDto
                                     from role in roleDto.DefaultIfEmpty()
                                     join version in _context.MedraVersion on medraConfig.MedraVersionId equals version.Id into versionDto
                                     from mv in versionDto.DefaultIfEmpty()
                                     join ln in _context.MedraLanguage on medraConfig.MedraVersionId equals ln.Id into lnDto
                                     from ml in lnDto.DefaultIfEmpty()
                                     where meddraLLT.pt_code == meddraMD.pt_code
                                        && meddraSoc.MedraConfigId == medraConfig.Id
                                        && meddraLLT.MedraConfigId == medraConfig.Id
                                        && meddraMD.MedraConfigId == medraConfig.Id
                                        && randomization.RandomizationNumber != null
                                     select new MeddraDetails
                                     {
                                         ProjectCode = ProjectCode,
                                         SiteCode = se.Project.ParentProjectId != null ? se.Project.ProjectCode : "",
                                         DomainCode = pdv.Domain.DomainName,
                                         //ScreeningNumber = nonregister.ScreeningNumber,
                                         //RandomizationNumber = nonregister.RandomizationNumber,
                                         //Initial = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                                         Initial = st.ScreeningVisit.ScreeningEntry.RandomizationId != null ? randomization.Initial : st.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.AliasName,
                                         ScreeningNumber = st.ScreeningVisit.ScreeningEntry.RandomizationId != null ? randomization.ScreeningNumber : st.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                                         RandomizationNumber = st.ScreeningVisit.ScreeningEntry.RandomizationId != null ? randomization.RandomizationNumber : "",
                                         RepeatedVisit = st.ScreeningVisit.RepeatedVisitNumber,
                                         Visit = st.ScreeningVisit.ProjectDesignVisit.DisplayName + Convert.ToString(st.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + st.ScreeningVisit.RepeatedVisitNumber),
                                         TemplateName = st.ProjectDesignTemplate.TemplateName,
                                         VariableAnnotation = pdv.Annotation,
                                         VariableTerm = value.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox ? string.Join(";",
                                        from stvc in _context.ScreeningTemplateValueChild.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == value.Id && x.Value == "true")
                                        join prpjectdesignvalueTemp in _context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null) on stvc.ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into
                                        prpjectdesignvalueDto
                                        from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                                        select prpjectdesignvalue.ValueName)
                                        : value.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox &&
                                        !string.IsNullOrEmpty(value.Value)
                                        ? _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                        b.ProjectDesignVariableId == value.ProjectDesignVariable.Id).ValueName
                                        : value.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox &&
                                        value.IsNa && string.IsNullOrEmpty(value.Value) ? "NA"
                                        : value.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                                        value.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton
                                        ? _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                        b.ProjectDesignVariableId == value.ProjectDesignVariable.Id &&
                                        b.Id == Convert.ToInt32(value.Value)).ValueName
                                        : value.Value,
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
                                         CodedBy = user.UserName,
                                         CodedOn = meddraCoding.ModifiedDate
                                     }).OrderBy(x => x.ScreeningNumber).ToList();


                #endregion

                MainData.Meddra = MeddraDetails;
            }

            #region Report Design
            var repeatdata = new List<RepeatTemplateDto>();
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet;

                if (filters.FilterId == DBDSReportFilter.DBDS || filters.FilterId == null)
                {
                    MainData.Dbds.ForEach(d =>
                    {
                        worksheet = workbook.Worksheets.Add(d.DomainName);

                        worksheet.Rows(1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                        worksheet.Cell(1, 1).Value = "STUDY CODE";
                        worksheet.Cell(1, 2).Value = "SITE CODE";
                        worksheet.Cell(1, 3).Value = d.DomainName;
                        worksheet.Cell(1, 4).Value = "SCRNUM";
                        worksheet.Cell(1, 5).Value = "RANDNUM";
                        worksheet.Cell(1, 6).Value = "INITIAL";
                        worksheet.Cell(1, 7).Value = "VISIT";

                        worksheet.Cell(2, 1).Value = "Study Code";
                        worksheet.Cell(2, 2).Value = "Site Code";
                        worksheet.Cell(2, 3).Value = "Panel Name";
                        worksheet.Cell(2, 4).Value = "Screening No";
                        worksheet.Cell(2, 5).Value = "Enrollment No";
                        worksheet.Cell(2, 6).Value = "Patient Initial";
                        worksheet.Cell(2, 7).Value = "Visit";

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
                                var repeatlength = vst.LstProjectDataBaseitems.Where(m => m.ScreeningTemplateParentId != null).GroupBy(x => x.ScreeningTemplateId).ToList().Count;

                                worksheet.Row(j).Cell(1).SetValue(db.ProjectCode);
                                worksheet.Row(j).Cell(2).SetValue(db.ParentProjectId != null ? db.ProjectName : "");
                                worksheet.Row(j).Cell(3).SetValue(vst.DesignOrder + ". " + vst.TemplateName);
                                worksheet.Row(j).Cell(4).SetValue(db.SubjectNo);
                                worksheet.Row(j).Cell(5).SetValue(db.RandomizationNumber);
                                worksheet.Row(j).Cell(6).SetValue(db.Initial);
                                worksheet.Row(j).Cell(7).SetValue(vst.Visit);

                                var repeatorder = 1;
                                if (repeatlength > 0)
                                {
                                    for (var m = 0; m < repeatlength; m++)
                                    {
                                        j = j + 1;
                                        worksheet.Row(j).Cell(1).SetValue(db.ProjectCode);
                                        worksheet.Row(j).Cell(2).SetValue(db.ParentProjectId != null ? db.ProjectName : "");
                                        worksheet.Row(j).Cell(3).SetValue(vst.DesignOrder + "." + repeatorder + " " + vst.TemplateName);
                                        worksheet.Row(j).Cell(4).SetValue(db.SubjectNo);
                                        worksheet.Row(j).Cell(5).SetValue(db.RandomizationNumber);
                                        worksheet.Row(j).Cell(6).SetValue(db.Initial);
                                        worksheet.Row(j).Cell(7).SetValue(vst.Visit);
                                        repeatorder++;
                                    }
                                }
                                j++;
                            });
                        });

                        var rownumber = 3;
                        var totallen = d.LstProjectDataBase.Count;
                        for (var n = 0; n < totallen; n++)
                        {
                            var totalVariablevisit = d.LstProjectDataBase[n].LstProjectDataBaseVisit.Count;
                            for (var vst = 0; vst < totalVariablevisit; vst++)
                            {
                                var indexrow = 0;
                                var totalVariablelen = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems.Count;
                                for (var m = 7; m < (totalVariablelen + 7); m++)
                                {
                                    var variableName = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].VariableName;
                                    var parent = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].ScreeningTemplateParentId;
                                    var templateID = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].ScreeningTemplateId;
                                    var collectionSource = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].CollectionSource;

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
                                        var variablevalueformat = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].VariableNameValue;
                                        var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).AddHours(5).AddMinutes(30).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : variablevalueformat : "";
                                        worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                                    }
                                    else if (collectionSource == (int)CollectionSources.Date)
                                    {
                                        DateTime dDate;
                                        var variablevalueformat = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].VariableNameValue;
                                        string dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.TryParse(variablevalueformat, out dDate) ? DateTime.Parse(variablevalueformat).AddHours(5).AddMinutes(30).ToString(GeneralSettings.DateFormat, CultureInfo.InvariantCulture) : variablevalueformat : "";
                                        worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                                    }
                                    else if (collectionSource == (int)CollectionSources.Time)
                                    {
                                        var variablevalueformat = d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].VariableNameValue;
                                        var dt = !string.IsNullOrEmpty(variablevalueformat) ? DateTime.Parse(variablevalueformat).AddHours(5).AddMinutes(30).ToString(GeneralSettings.TimeFormat, CultureInfo.InvariantCulture) : "";
                                        worksheet.Cell(rownumber, cellnumber).SetValue(dt);
                                    }
                                    else
                                    {
                                        worksheet.Cell(rownumber, cellnumber).SetValue(
                                        d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].VariableNameValue);
                                    }

                                    if (d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].UnitId != null)
                                    {
                                        cellnumber += 1;
                                        m += 1;
                                        totalVariablelen += 1;
                                        worksheet.Cell(rownumber, cellnumber).SetValue(
                                            d.LstProjectDataBase[n].LstProjectDataBaseVisit[vst].LstProjectDataBaseitems[indexrow].Unit);
                                    }

                                    indexrow++;
                                }

                                rownumber++;
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
                        var dt = !string.IsNullOrEmpty(m.CodedOn.ToString()) ? DateTime.TryParse(m.CodedOn.ToString(), out dDate) ? DateTime.Parse(m.CodedOn.ToString()).AddHours(5).AddMinutes(30).ToString(GeneralSettings.DateFormat + ' ' + GeneralSettings.TimeFormat) : m.CodedOn.ToString() : "";
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
                // string path = "D://Excel//";

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
                    jobMonitoring.CompletedTime = DateTime.Now.UtcDateTime();
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
                    //return content;
                }
            }
            #endregion
        }
    }
}