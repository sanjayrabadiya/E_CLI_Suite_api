using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueRepository : GenericRespository<ScreeningTemplateValue, GscContext>,
        IScreeningTemplateValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;

        public ScreeningTemplateValueRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignVariableRepository projectDesignVariableRepository)
            : base(uow, jwtTokenAccesser)
        {
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
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

                Add(new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplateId,
                    ProjectDesignVariableId = variable.Id,
                    Value = variable.DefaultValue,
                    Audits = new List<ScreeningTemplateValueAudit>
                    {
                        new ScreeningTemplateValueAudit
                        {
                            Value = string.IsNullOrEmpty(variable.DefaultValue) ? "" : variable.DefaultValue,
                            OldValue = null,
                            Note = "Submitted with default data",
                            UserId = _jwtTokenAccesser.UserId,
                            UserRoleId = _jwtTokenAccesser.RoleId
                        }
                    }
                });
            }
        }

        public QueryStatusDto GetQueryStatusCount(int screeningTemplateId)
        {
            var result = All.Where(x => x.DeletedDate == null
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

            var result = screeningTemplateValue.Where(x => x.ScreeningTemplateId == screeningTemplateId &&
                                                           x.QueryStatus != QueryStatus.Closed && x.QueryStatus != null)
                .ToList();
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
            var childs = Context.ScreeningTemplateValueChild
                .Where(t => t.ScreeningTemplateValueId == screeningTemplateValueId).ToList();
            Context.ScreeningTemplateValueChild.RemoveRange(childs);
        }

        public void UpdateChild(List<ScreeningTemplateValueChild> children)
        {
            Context.ScreeningTemplateValueChild.UpdateRange(children);
        }

        public string CheckCloseQueries(int screeningTemplateId)
        {
            var validateMsg = "";

            if (All.Any(x => x.ScreeningTemplateId == screeningTemplateId && x.QueryStatus != null && x.QueryStatus != QueryStatus.Closed))
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

                var variableValue = Context.ProjectDesignVariableValue.Find(child.ProjectDesignVariableValueId);
                if (variableValue != null)
                {
                    var valueChild = Context.ScreeningTemplateValueChild.AsNoTracking()
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

        public CommonDto GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters)
        {
            var ProjectCode = Context.Project.Find(filters.ParentProjectId).ProjectCode;

            var queryDtos = (from screening in Context.ScreeningEntry.Where(t =>
                    filters.ProjectId.Contains(t.ProjectId) &&
                    (filters.PeriodIds == null || filters.PeriodIds.Contains(t.ProjectDesignPeriodId))
                    && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.AttendanceId)) &&
                    t.DeletedDate == null)
                             join template in Context.ScreeningTemplate.Where(u =>
                                     (filters.TemplateIds == null || filters.TemplateIds.Contains(u.ProjectDesignTemplateId))
                                     && (filters.VisitIds == null ||
                                         filters.VisitIds.Contains(u.ScreeningVisit.ProjectDesignVisitId)) &&
                                     (filters.DomainIds == null ||
                                      filters.DomainIds.Contains(u.ProjectDesignTemplate.DomainId)) && u.DeletedDate == null)
                                 on screening.Id equals template.ScreeningVisit.ScreeningEntryId
                             join valueTemp in Context.ScreeningTemplateValue.Where(val => val.DeletedDate == null
                                               && val.ProjectDesignVariable.DeletedDate == null) on new { template.Id, template.ProjectDesignTemplateId }
                                             equals new { Id = valueTemp.ScreeningTemplateId, valueTemp.ProjectDesignVariable.ProjectDesignTemplateId }
                                             into valueDto
                             from value in valueDto.DefaultIfEmpty()
                             join attendance in Context.Attendance.Where(t => t.DeletedDate == null)
                                 on screening.AttendanceId equals attendance.Id
                             join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into
                                 volunteerDto
                             from volunteer in volunteerDto.DefaultIfEmpty()
                             join randomizationTemp in Context.Randomization  on attendance.Id equals randomizationTemp.AttendanceId
                                 into randomizationDto
                             from randomization in randomizationDto.DefaultIfEmpty()
                             join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals
                                 projectSubjectTemp.Id into projectsubjectDto
                             from projectsubject in projectsubjectDto.DefaultIfEmpty()
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
                                 VisitId = template.ScreeningVisitId,
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
                                 VariableNameValue = value == null ? null :
                                     value.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox
                                         ? string.Join(";",
                                             from stvc in Context.ScreeningTemplateValueChild.Where(x =>
                                                 x.DeletedDate == null && x.ScreeningTemplateValueId == value.Id &&
                                                 x.Value == "true")
                                             join prpjectdesignvalueTemp in
                                                 Context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null) on stvc
                                                     .ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into
                                                 prpjectdesignvalueDto
                                             from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                                             select prpjectdesignvalue.ValueName)
                                         : value.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox &&
                                           !string.IsNullOrEmpty(value.Value)
                                             ? Context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                                 b.ProjectDesignVariableId == value.ProjectDesignVariable.Id).ValueName
                                             : value.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox &&
                                               value.IsNa && string.IsNullOrEmpty(value.Value)
                                                 ? "NA"
                                                 : value.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                                                   value.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton
                                                     ? Context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                                         b.ProjectDesignVariableId == value.ProjectDesignVariable.Id &&
                                                         b.Id == Convert.ToInt32(value.Value)).ValueName
                                                     : value.Value,
                                 Initial = volunteer.FullName == null ? randomization.Initial : volunteer.AliasName,
                                 SubjectNo = volunteer.FullName == null ? randomization.ScreeningNumber : volunteer.VolunteerNo,
                                 RandomizationNumber = volunteer.FullName == null
                                     ? randomization.RandomizationNumber
                                     : projectsubject.Number,
                             }).ToList();
            var grpquery = queryDtos.OrderBy(d => d.VisitId).ThenBy(x => x.DesignOrder).GroupBy(x => new { x.DomainName, x.DomainId }).Select(y => new ProjectDatabaseDto
            {
                DomainName = y.Key.DomainName,
                DomainId = y.Key.DomainId,
                VisitId = y.FirstOrDefault().VisitId,
                TemplateId = y.FirstOrDefault().TemplateId,
                DesignOrder = y.FirstOrDefault().DesignOrder,
                LstVariable = y.Where(v => v.VariableName != null).ToList().Count > 0 ? y.Where(q => q.DomainId == y.Key.DomainId && q.VariableName != null).GroupBy(vari => vari.VariableName).Select(v =>
                    new ProjectDatabaseDto
                    {
                        VariableName = v.Key,
                        Annotation = v.FirstOrDefault().Annotation,
                        UnitId = v.FirstOrDefault().UnitId,
                        Unit = v.FirstOrDefault().Unit,
                        UnitAnnotation = v.FirstOrDefault().UnitAnnotation,
                        DesignOrderOfVariable = v.FirstOrDefault().DesignOrderOfVariable,
                        TemplateId = v.FirstOrDefault().TemplateId
                    }).OrderBy(o => o.TemplateId).ThenBy(d => d.DesignOrderOfVariable).ToList()
                    : Context.ProjectDesignVariable.Where(v => v.DeletedDate == null
                    && v.ProjectDesignTemplateId == y.FirstOrDefault().TemplateId).Select(x => new ProjectDatabaseDto
                    {
                        VariableName = x.VariableName,
                        Annotation = x.Annotation,
                        UnitId = x.UnitId,
                        Unit = x.Unit.UnitName,
                        UnitAnnotation = x.UnitAnnotation,
                        DesignOrderOfVariable = x.DesignOrder,
                        TemplateId = x.ProjectDesignTemplateId
                    }).OrderBy(o => o.TemplateId).ThenBy(d => d.DesignOrderOfVariable).ToList(),

                LstProjectDataBase = y.Where(v => v.VariableName != null).GroupBy(x => new { x.Initial, x.SubjectNo }).Select(s => new ProjectDatabaseDto
                {
                    Initial = s.Key.Initial,
                    ProjectId = s.FirstOrDefault().ProjectId,
                    ProjectCode = s.FirstOrDefault().ProjectCode,
                    ParentProjectId = s.FirstOrDefault().ParentProjectId,
                    ProjectName = s.FirstOrDefault().ProjectName,
                    SubjectNo = s.Key.SubjectNo,
                    RandomizationNumber = s.FirstOrDefault().RandomizationNumber,
                    LstProjectDataBaseVisit = s.GroupBy(vst => vst.Visit).Select(n => new ProjectDatabaseDto
                    {
                        Visit = n.Key,
                        DesignOrder = n.FirstOrDefault().DesignOrder,
                        TemplateName = n.FirstOrDefault().TemplateName,
                        LstProjectDataBaseitems = n.OrderBy(o => o.ScreeningTemplateId).ToList()
                    }).ToList()
                }).OrderBy(p => p.ProjectId).ToList()
            }).ToList();

            var MeddraDetails = (from meddra in Context.MeddraCoding
                                 join mcf in Context.MedraConfig on meddra.MeddraConfigId equals mcf.Id
                                 join mv in Context.MedraVersion on mcf.MedraVersionId equals mv.Id
                                 join ml in Context.MedraLanguage on mcf.LanguageId equals ml.Id
                                 join dict in Context.Dictionary on mv.DictionaryId equals dict.Id
                                 join stv in Context.ScreeningTemplateValue on meddra.ScreeningTemplateValueId equals stv.Id
                                 join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                                 join D in Context.Domain on pdv.DomainId equals D.Id
                                 join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id
                                 join visit in Context.ProjectDesignVisit on st.ScreeningVisitId equals visit.Id
                                 join se in Context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id
                                 join p in Context.Project on se.ProjectId equals p.Id
                                 join A in Context.Attendance on se.AttendanceId equals A.Id
                                 join lltDto in Context.MeddraLowLevelTerm on meddra.MeddraLowLevelTermId equals lltDto.Id into Mllt
                                 from llt in Mllt.DefaultIfEmpty()
                                 join mstDto in Context.MeddraSocTerm on meddra.MeddraSocTermId equals mstDto.Id into meddraSt
                                 from mst in meddraSt.DefaultIfEmpty()
                                 join mdDto in Context.MeddraMdHierarchy on mst.soc_code equals mdDto.soc_code into meddraMd
                                 from md in meddraMd.DefaultIfEmpty()
                                 join volunteerTemp in Context.Volunteer on A.VolunteerId equals volunteerTemp.Id into volunteerDto
                                 from volunteer in volunteerDto.DefaultIfEmpty()
                                 join randomizationTemp in Context.Randomization on A.Id equals randomizationTemp.AttendanceId into randomizationDto
                                 from randomization in randomizationDto.DefaultIfEmpty()
                                 join userTemp in Context.Users on meddra.ModifiedBy equals userTemp.Id into userDto
                                 from user in userDto.DefaultIfEmpty()
                                 where llt.pt_code == md.pt_code && filters.ProjectId.Contains(se.ProjectId)
                                 select new MeddraDetails
                                 {
                                     ProjectCode = ProjectCode,
                                     SiteCode = se.Project.ParentProjectId != null ? se.Project.ProjectCode : "",
                                     DomainCode = D.DomainCode,
                                     ScreeningNumber = se.ScreeningNo,
                                     RandomizationNumber = randomization.RandomizationNumber,
                                     Initial = volunteer.FullName == null ? randomization.Initial : volunteer.AliasName,
                                     RepeatedVisit = st.ScreeningVisit.RepeatedVisitNumber,
                                     Visit = st.ScreeningVisit.ProjectDesignVisit.DisplayName + Convert.ToString(st.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + st.ScreeningVisit.RepeatedVisitNumber),
                                     TemplateName = st.RepeatSeqNo == null && st.ParentId == null ? st.ProjectDesignTemplate.DesignOrder + " " + st.ProjectDesignTemplate.TemplateName
                                        : st.ProjectDesignTemplate.DesignOrder + "." + st.RepeatSeqNo + " " + st.ProjectDesignTemplate.TemplateName,
                                     VariableAnnotation = pdv.Annotation,
                                     VariableTerm = stv.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox ? string.Join(";",
                                    from stvc in Context.ScreeningTemplateValueChild.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == stv.Id && x.Value == "true")
                                    join prpjectdesignvalueTemp in Context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null) on stvc.ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into
                                    prpjectdesignvalueDto
                                    from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                                    select prpjectdesignvalue.ValueName)
                                    : stv.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox &&
                                    !string.IsNullOrEmpty(stv.Value)
                                    ? Context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                    b.ProjectDesignVariableId == stv.ProjectDesignVariable.Id).ValueName
                                    : stv.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox &&
                                    stv.IsNa && string.IsNullOrEmpty(stv.Value) ? "NA"
                                    : stv.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                                    stv.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton
                                    ? Context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                    b.ProjectDesignVariableId == stv.ProjectDesignVariable.Id &&
                                    b.Id == Convert.ToInt32(stv.Value)).ValueName
                                    : stv.Value,
                                     Version = mv.Version,
                                     Language = ml.LanguageName,
                                     SocCode = mst.soc_code.ToString(),
                                     SocName = mst.soc_name,
                                     SocAbbrev = mst.soc_abbrev,
                                     PrimaryIndicator = md.primary_soc_fg,
                                     HlgtCode = md.hlgt_code.ToString(),
                                     HlgtName = md.hlgt_name,
                                     HltCode = md.hlt_code.ToString(),
                                     HltName = md.hlt_name,
                                     PtCode = md.pt_code.ToString(),
                                     PtName = md.pt_name,
                                     PtSocCode = md.pt_soc_code.ToString(),
                                     LltCode = llt.llt_code.ToString(),
                                     LltName = llt.llt_name,
                                     LltCurrency = llt.llt_currency,
                                     CodedBy = user.UserName,
                                     CodedOn = meddra.ModifiedDate
                                 }).ToList();

            CommonDto MainData = new CommonDto();

            MainData.Meddra = MeddraDetails;
            MainData.Dbds = grpquery;

            return MainData;
        }
    }
}