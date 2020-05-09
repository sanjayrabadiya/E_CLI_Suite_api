﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.ProjectRight;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.EditCheck
{
    public class EditCheckRepository : GenericRespository<Data.Entities.Project.EditCheck.EditCheck, GscContext>,
        IEditCheckRepository
    {
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IMapper _mapper;
        private readonly IEditCheckImpactRepository _editCheckImpactRepository;
        public EditCheckRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            IMapper mapper,
            IEditCheckDetailRepository editCheckDetailRepository,
            IEditCheckImpactRepository editCheckImpactRepository,
            INumberFormatRepository numberFormatRepository) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _numberFormatRepository = numberFormatRepository;
            _editCheckDetailRepository = editCheckDetailRepository;
            _editCheckImpactRepository = editCheckImpactRepository;
            _mapper = mapper;
        }

        public List<EditCheckDto> GetAll(int projectDesignId, bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0)
                return new List<EditCheckDto>();

            return All.Where(t => (t.CompanyId == null
                                   || t.CompanyId == _jwtTokenAccesser.CompanyId)
                                  && t.IsDeleted == isDeleted
                                  && t.ProjectDesignId == projectDesignId
                                  && projectList.Any(c => c == t.ProjectDesign.ProjectId)
            ).Select(r => new EditCheckDto
            {
                Id = r.Id,
                ProjectDesignId = r.ProjectDesignId,
                AutoNumber = r.AutoNumber,
                CheckFormula = r.CheckFormula,
                TargetFormula = r.TargetFormula,
                SampleResult = r.SampleResult,
                ErrorMessage = r.ErrorMessage,
                SourceFormula = r.SourceFormula,
                StatusName = !string.IsNullOrEmpty(r.TargetFormula) && !string.IsNullOrEmpty(r.SourceFormula) ? "Completed" :
                r.IsOnlyTarget ? "Only Target" : "In-Complete",
                IsLock = !r.ProjectDesign.IsUnderTesting,
                IsFormula = r.IsFormula,
                IsReferenceVerify = r.IsReferenceVerify,
                IsDeleted = r.IsDeleted
            })
            .OrderByDescending(x => x.Id).ToList();
        }

        public void SaveEditCheck(Data.Entities.Project.EditCheck.EditCheck editCheck)
        {
            var number = All.Count(x => x.ProjectDesignId == editCheck.ProjectDesignId) + 1;
            editCheck.AutoNumber = _numberFormatRepository.GetNumberFormat("EditCheck", number);
            Add(editCheck);
        }

        public Data.Entities.Project.EditCheck.EditCheck CopyTo(int id)
        {
            var editCheck = All.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();
            var number = All.Count(x => x.ProjectDesignId == editCheck.ProjectDesignId) + 1;
            editCheck.Id = 0;
            editCheck.AutoNumber = _numberFormatRepository.GetNumberFormat("EditCheck", number);
            var details = _editCheckDetailRepository.All.AsNoTracking().Where(r => r.EditCheckId == id && r.DeletedDate == null).ToList();
            details.ForEach(r =>
            {
                r.Id = 0;
                r.EditCheckId = 0;
                _editCheckDetailRepository.Add(r);
            });
            editCheck.EditCheckDetails = details;
            Add(editCheck);
            return editCheck;
        }
        public EditCheckDto GetEditCheckDetail(int id, bool isDeleted)
        {
            var resut = All.Where(x => x.Id == id).Select(x => new EditCheckDto
            {
                Id = x.Id,
                ProjectDesignId = x.ProjectDesignId,
                AutoNumber = x.AutoNumber,
                CheckFormula = x.CheckFormula,
                TargetFormula = x.TargetFormula,
                SampleResult = x.SampleResult,
                ErrorMessage = x.ErrorMessage,
                SourceFormula = x.SourceFormula,
                IsFormula = x.IsFormula,
                IsOnlyTarget = x.IsOnlyTarget,
                IsReferenceVerify = x.IsReferenceVerify,
                CompanyId = x.CompanyId,
                EditCheckDetails = x.EditCheckDetails.Where(r => r.IsDeleted == isDeleted).Select
                 (c => new EditCheckDetailDto
                 {
                     EditCheckId = c.EditCheckId,
                     Id = c.Id,
                     CheckBy = c.CheckBy,
                     CheckByName = c.CheckBy.GetDescription(),
                     ByAnnotation = c.ByAnnotation,
                     ProjectDesignTemplateId = c.ProjectDesignTemplateId,
                     ProjectDesignVisitId = c.ProjectDesignVariable != null
                         ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.Id
                         : c.ProjectDesignTemplate.ProjectDesignVisit.Id,
                     ProjectDesignPeriodId = c.ProjectDesignVariable != null
                         ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId
                         : c.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriodId,
                     ProjectDesignVariableId = c.ProjectDesignVariableId,
                     VariableAnnotation = c.VariableAnnotation,
                     DomainId = c.DomainId,
                     Operator = c.Operator,
                     OperatorName = c.Operator.GetDescription(),
                     CollectionSource = c.ProjectDesignVariable.CollectionSource,
                     DataType = c.ProjectDesignVariable.DataType,
                     IsTarget = c.IsTarget,
                     TargetName = c.IsTarget ? "Yes" : "No",
                     LogicalOperator = c.LogicalOperator ?? "",
                     CollectionValue2 = c.CollectionValue2 ?? "",
                     CollectionValue = c.CollectionValue ?? "",
                     IsReferenceValue = c.IsReferenceValue,
                     StartParens = c.StartParens,
                     IsFormula = x.IsFormula,
                     EndParens = c.EndParens,
                     IsSameTemplate = c.IsSameTemplate,
                     ExtraData = _mapper.Map<List<ProjectDesignVariableValueDropDown>>(c.ProjectDesignVariable.Values.Where(b => b.DeletedDate == null).ToList()),
                     Message = c.Message,
                     QueryFormula = c.QueryFormula,
                     PeriodName = c.ProjectDesignVariable != null
                         ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName
                         : "",
                     TemplateName = c.ProjectDesignTemplate.TemplateName,
                     VariableName = c.ProjectDesignVariable.VariableName,
                     VisitName = c.ProjectDesignVariable != null
                         ? c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName
                         : "",
                     FieldName = c.CheckBy == EditCheckRuleBy.ByTemplate
                         ?
                         c.ProjectDesignTemplate.ProjectDesignVisit.DisplayName + "." +
                         c.ProjectDesignTemplate.TemplateName
                         : c.CheckBy == EditCheckRuleBy.ByTemplateAnnotation
                             ? c.Domain.DomainName
                             : c.CheckBy == EditCheckRuleBy.ByVariableAnnotation
                                 ? c.VariableAnnotation
                                 :
                                 c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                                     .DisplayName + "." +
                                 c.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName + "." +
                                 c.ProjectDesignVariable.ProjectDesignTemplate.TemplateName + "." +
                                 (c.ByAnnotation
                                     ? c.ProjectDesignVariable.Annotation
                                     : c.ProjectDesignVariable.VariableName)
                 }).OrderBy(v => v.Id).ToList()
            }).FirstOrDefault();

            resut.EditCheckDetails.ForEach(x =>
            {

                if (x.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                    x.CollectionSource = GetCollectionSources(x.VariableAnnotation, resut.ProjectDesignId);

                x.CollectionValue =
                         string.IsNullOrEmpty(x.CollectionValue)
                             ? ""
                             :
                               IsMultiCollection(x.CollectionSource)
                                       ? string.Join(", ", Context.ProjectDesignVariableValue
                                                   .Where(t => ProjectDesignVariableId(x.CollectionValue).Contains(t.Id)).
                                                   Select(a => a.ValueName).ToList())
                                       : x.CollectionValue;
            });
            return resut;
        }

        CollectionSources? GetCollectionSources(string annotation, int projectDesignId)
        {
            if (string.IsNullOrEmpty(annotation)) return null;

            var annotationVariable = Context.ProjectDesignVariable.Where(a => a.Annotation == annotation
                       && a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId).FirstOrDefault();

            if (annotationVariable != null)
                return annotationVariable.CollectionSource;

            return null;
        }

        public Data.Entities.Project.EditCheck.EditCheck UpdateFormula(int id)
        {
            var editCheck = Find(id);
            editCheck.TargetFormula = GetFormula(id, true);
            editCheck.SourceFormula = GetFormula(id, false);
            editCheck.CheckFormula = editCheck.TargetFormula + " -> " + editCheck.SourceFormula;
            var verifyResult = CheckParens(editCheck.Id, editCheck.IsFormula);
            editCheck.IsReferenceVerify = verifyResult.IsValid;
            editCheck.SampleResult = verifyResult.ResultMessage;
            editCheck.ErrorMessage = verifyResult.ErrorMessage;
            editCheck.IsOnlyTarget = !_editCheckDetailRepository.All.
                Any(t => t.EditCheckId == editCheck.Id &&
                t.DeletedDate == null && !t.IsTarget);

            Update(editCheck);

            Context.SaveChanges(_jwtTokenAccesser);

            return editCheck;
        }

        EditCheckResult CheckParens(int editCheckId, bool isFormula)
        {
            var data = _editCheckDetailRepository.All.AsNoTracking().
                Where(x => x.DeletedDate == null &&
                !x.IsTarget && x.EditCheckId == editCheckId).Select(r => new EditCheckValidate
                {
                    IsTarget = r.IsTarget,
                    StartParens = r.StartParens,
                    Input1 = "1",
                    IsFormula = r.EditCheck.IsFormula,
                    Operator = r.Operator,
                    OperatorName = r.Operator == null ? "" : r.Operator.GetDescription(),
                    EndParens = r.EndParens,
                    CollectionValue2 = r.CollectionValue2,
                    CollectionValue = r.CollectionValue
                }).ToList();

            if (isFormula)
            {
                return _editCheckImpactRepository.ValidateEditCheckReference(data);
            }

            var startParens = string.Join(" ", data.Select(r => r.StartParens ?? ""));
            var endParens = string.Join(" ", data.Select(r => r.EndParens ?? ""));

            return new EditCheckResult { IsValid = (startParens.Trim().Length == endParens.Trim().Length) };
        }

        List<int> ProjectDesignVariableId(string collectionValue)
        {
            List<int> result = new List<int>();
            if (!string.IsNullOrEmpty(collectionValue))
            {
                collectionValue.Split(",").ForEach(x => { result.Add(Convert.ToInt32(x)); });
            }
            return result;
        }

        bool IsMultiCollection(CollectionSources? collectionSources)
        {
            return collectionSources != null & (collectionSources == CollectionSources.ComboBox ||
                                   collectionSources == CollectionSources.RadioButton ||
                                   collectionSources == CollectionSources.CheckBox ||
                                   collectionSources == CollectionSources.MultiCheckBox);
        }


        bool IsInFilter(Operator? operatores)
        {
            if (operatores == null) return false;

            return operatores == Operator.In ||
                                   operatores == Operator.NotIn ||
                                   operatores == Operator.Filter;
        }

        private string GetFormula(int id, bool isTarget)
        {
            var variableValues = Context.EditCheckDetail.
                Where(x => x.EditCheckId == id
                && x.IsTarget == isTarget
                && x.DeletedDate == null).Select(r => r.CollectionValue).ToList();

            var result = Context.EditCheckDetail.Where(x => x.EditCheckId == id
                                                            && x.IsTarget == isTarget
                                                            && x.DeletedDate == null).
                OrderByDescending(v => v.LogicalOperator).Select(r => new EditCheckDetailDto
                {
                    PeriodName = r.ProjectDesignVariable != null
                         ? r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName
                         : r.ProjectDesignTemplate != null ? r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName : "",
                    TemplateName = r.ProjectDesignTemplate.TemplateName,
                    VariableName = r.ProjectDesignVariable.Annotation ?? r.ProjectDesignVariable.VariableName,
                    VisitName = r.ProjectDesignVariable != null
                         ? r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName
                         : r.ProjectDesignTemplate != null ? r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName : "",
                    Operator = r.Operator,
                    LogicalOperator = r.LogicalOperator,
                    DomainName = r.Domain.DomainName,
                    VariableAnnotation = r.VariableAnnotation,
                    CheckBy = r.CheckBy,
                    StartParens = r.StartParens,
                    EndParens = r.EndParens,
                    CollectionValue = r.CollectionValue,
                    ProjectDesignId = r.EditCheck.ProjectDesignId,
                    CollectionValue2 = r.CollectionValue2,
                    CollectionSource = r.ProjectDesignVariable.CollectionSource
                }).OrderBy(r => r.Id).ToList();

            var last = result.LastOrDefault();
            result.ForEach(x =>
           {
               if (x.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                   x.CollectionSource = GetCollectionSources(x.VariableAnnotation, x.ProjectDesignId);


               var name = (x.CheckBy == EditCheckRuleBy.ByTemplate ?
                    x.PeriodName + "." + x.VisitName + "." + x.TemplateName :
                    x.CheckBy == EditCheckRuleBy.ByTemplateAnnotation ?
                        x.DomainName :
                     x.CheckBy == EditCheckRuleBy.ByVariableAnnotation
                           ? x.VariableAnnotation :
                            x.PeriodName + "." + x.VisitName + "." +
                            x.TemplateName + "." + x.VariableName);

               var operatorName = x.Operator.GetDescription();

               var collectionValue = (string.IsNullOrEmpty(x.CollectionValue) ? ""
                        : IsMultiCollection(x.CollectionSource) ?
                        Convert.ToString(IsInFilter(x.Operator) ? "(" : "") +
                        string.Join(", ", Context.ProjectDesignVariableValue
                        .Where(t => ProjectDesignVariableId(x.CollectionValue).Contains(t.Id)).
                        Select(a => a.ValueName).ToList()) +
                        Convert.ToString(IsInFilter(x.Operator) ? ")" : "")
                        : x.CollectionValue);


               if (x.Operator != null && ((Operator)x.Operator).CheckMathOperator())
               {
                   if (x.Equals(last))
                       name = $"{x.StartParens}{"{"}{name.Trim()}{"}"}{x.EndParens ?? ""} {collectionValue}";
                   else
                       name = $"{x.StartParens}{"{"}{name.Trim()}{"}"} {operatorName}{x.EndParens ?? ""} {collectionValue}";
               }

               else
               {
                   name = $"{x.StartParens}{"{"}{name.Trim()} {operatorName}{x.EndParens ?? ""} {collectionValue}";

                   if (!string.IsNullOrEmpty(x.CollectionValue2) && (x.Operator == Operator.Between || x.Operator == Operator.NotBetween))
                       name = name + " AND " + x.CollectionValue2;

                   if (x.Equals(last))
                       name = $"{name}{"}"}";
                   else
                       name = $"{name}{"}"} {x.LogicalOperator}";
               }

               x.QueryFormula = name;

           });

            return string.Join(" ", result.Select(r => r.QueryFormula));
        }



    }
}