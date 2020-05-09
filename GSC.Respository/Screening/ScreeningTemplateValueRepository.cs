﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                                        && x.ScreeningTemplateId == screeningTemplateId).ToList();
            if (result != null) return GetQueryStatusByModel(result, screeningTemplateId);

            return null;
        }

        public QueryStatusDto GetQueryStatusByModel(List<ScreeningTemplateValue> screeningTemplateValue,
            int screeningTemplateId)
        {
            if (screeningTemplateValue == null) return null;

            var result = screeningTemplateValue.Where(x => x.ScreeningTemplateId == screeningTemplateId &&
                                                           x.QueryStatus != QueryStatus.Closed && x.QueryStatus != null)
                .ToList();
            if (result != null && result.Count > 0)
            {
                var queryStatusDto = new QueryStatusDto();
                queryStatusDto.Items = result.GroupBy(r => r.QueryStatus).Select(t => new QueryStatusCount
                    {QueryStatus = ((QueryStatus) t.Key).GetDescription(), Total = t.Count()}).ToList();
                queryStatusDto.TotalQuery = queryStatusDto.Items.Sum(x => x.Total);
                return queryStatusDto;
            }

            return null;
        }

        public List<PeriodQueryStatusDto> GetQueryStatusByPeridId(int projectDesignPeriodId)
        {
            return All.Where(x => x.DeletedDate == null &&
                                  x.ScreeningTemplate.ScreeningEntry.ProjectDesignPeriodId == projectDesignPeriodId
                                  && x.QueryStatus != null
                                  && x.ScreeningTemplate.ProjectDesignVisitId != null).Select(r =>
                new PeriodQueryStatusDto
                {
                    ScreeningEntryId = r.ScreeningTemplate.ScreeningEntryId,
                    AcknowledgeLevel = r.AcknowledgeLevel,
                    QueryStatus = r.QueryStatus
                }).ToList();
        }

        //Chnages by Vipul 09/03/2020 get extra param screeningEntryId
        public int GetQueryCountByVisitId(int projectDesignVisitId, int screeningEntryid)
        {
            return All.Count(x => x.DeletedDate == null &&
                                  x.ScreeningTemplate.ProjectDesignVisitId == projectDesignVisitId
                                  && x.ScreeningTemplate.ScreeningEntryId == screeningEntryid
                                  && x.QueryStatus != null && x.QueryStatus != QueryStatus.Closed
                                  && x.ScreeningTemplate.ProjectDesignVisitId != null);
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

        public string CheckCloseQueries(List<ScreeningTemplateValue> screeningTemplateValues)
        {
            var validateMsg = "";

            if (screeningTemplateValues.Any(x => x.QueryStatus != null && x.QueryStatus != QueryStatus.Closed))
                validateMsg = "Please close all queries! \n";

            //screeningTemplateValues.Where(x => x.QueryStatus != null && x.QueryStatus != QueryStatus.Closed)
            //    .ForEach(x =>
            //    {
            //        validateMsg = validateMsg + x.ProjectDesignVariable.DesignOrder + " " + x.ProjectDesignVariable.VariableName + " Level " + x.AcknowledgeLevel.ToString() + " Status " + x.QueryStatus.ToString() + "\n";
            //    });

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
            // public List<DashboardQueryStatusDto> GetQueryByProjectDesignId(int projectDesignId,int screeningEntryId)
        {
            //return All.Where(x => x.DeletedDate == null &&
            // x.ScreeningTemplate.ScreeningEntry.ProjectDesignId == projectDesignId
            // && x.ScreeningTemplate.ScreeningEntryId == screeningEntryId
            //  && x.QueryStatus != null).
            // GroupBy(c => new { c.ScreeningTemplate.ScreeningEntryId,c.QueryStatus }).
            // Select(g => new DashboardQueryStatusDto
            // {
            //     ScreeningEntryId = g.Key.ScreeningEntryId,
            //     Open = g.Count(x => x.QueryStatus == QueryStatus.Open),
            //     Answered = g.Count(x => x.QueryStatus == QueryStatus.Answered),
            //     Resolved = g.Count(x => x.QueryStatus == QueryStatus.Resolved),
            //     ReOpened = g.Count(x => x.QueryStatus == QueryStatus.Reopened),
            //     Closed = g.Count(x => x.QueryStatus == QueryStatus.Closed),
            //     SelfCorrection = g.Count(x => x.QueryStatus == QueryStatus.SelfCorrection),
            //     Acknowledge = g.Count(x => x.QueryStatus == QueryStatus.Acknowledge)
            // }).ToList();

            return All.Where(x => x.DeletedDate == null &&
                                  x.ScreeningTemplate.ScreeningEntry.ProjectDesignId == projectDesignId
                                  //&& x.ScreeningTemplate.ScreeningEntryId == screeningEntryId
                                  && x.QueryStatus != null)
                .GroupBy(c => new {c.ScreeningTemplate.ScreeningEntryId, c.QueryStatus}).Select(r =>
                    new DashboardQueryStatusDto
                    {
                        ScreeningEntryId = r.Key.ScreeningEntryId,
                        Status = r.Key.QueryStatus,
                        Total = r.Count()
                    }).ToList();
        }

        public IList<ProjectDatabaseDto> GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters)
        {
            try
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
                                filters.VisitIds.Contains(u.ProjectDesignTemplate.ProjectDesignVisitId)) &&
                            (filters.DomainIds == null ||
                             filters.DomainIds.Contains(u.ProjectDesignTemplate.DomainId)) && u.DeletedDate == null)
                        on screening.Id equals template.ScreeningEntryId
                                 //join value in Context.ScreeningTemplateValue.Where(val => val.DeletedDate == null
                                 //                                                                      && val.ProjectDesignVariable
                                 //                                                                          .DeletedDate == null) on new
                                 //                                                                          { template.Id, template.ProjectDesignTemplateId } equals new
                                 //                                                                          { Id = value.ScreeningTemplateId, value.ProjectDesignVariable.ProjectDesignTemplateId }
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
                    join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId
                        into noneregisterDto
                    from nonregister in noneregisterDto.DefaultIfEmpty()
                    join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals
                        projectSubjectTemp.Id into projectsubjectDto
                    from projectsubject in projectsubjectDto.DefaultIfEmpty()
                    //where !(Context.EditCheckDetail.Where(o => o.Operator == Operator.BMI || o.Operator == Operator.Percentage)
                    //.SelectMany(m => new int?[] { m.ProjectDesignVariableId }).Contains(value.ProjectDesignVariableId))
                    select new ProjectDatabaseDto
                    {
                       // Id = value.Id,
                        ScreeningEntryId = screening.Id,
                        ScreeningTemplateId = template.Id,
                        ScreeningTemplateParentId = template.ParentId,
                        ProjectId = screening.ProjectId,
                        ProjectCode =ProjectCode,
                        ParentProjectId = screening.Project.ParentProjectId,
                        ProjectName = screening.Project.ProjectCode,
                        DesignOrder = template.ProjectDesignTemplate.DesignOrder,
                        DesignOrderOfVariable = value == null ? 0 : value.ProjectDesignVariable.DesignOrder,
                        TemplateId = template.ProjectDesignTemplateId,
                        TemplateName = template.ProjectDesignTemplate.TemplateName,
                        DomainName = template.ProjectDesignTemplate.Domain.DomainName,
                        DomainId = template.ProjectDesignTemplate.DomainId,
                        VisitId = template.ProjectDesignVisitId,
                        RepeatedVisit = template.RepeatedVisit,
                        Visit = template.ProjectDesignVisit.DisplayName +
                                Convert.ToString(template.RepeatedVisit == null ? "" : "_" + template.RepeatedVisit),
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
                        Initial = volunteer.FullName == null ? nonregister.Initial : volunteer.AliasName,
                        SubjectNo = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                        RandomizationNumber = volunteer.FullName == null
                            ? nonregister.RandomizationNumber
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
                        ParentProjectId =s.FirstOrDefault().ParentProjectId,
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

                return grpquery;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}