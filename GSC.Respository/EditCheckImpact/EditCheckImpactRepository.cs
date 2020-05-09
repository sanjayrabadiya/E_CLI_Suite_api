using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Screening;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public class EditCheckImpactRepository : IEditCheckImpactRepository
    {

        private readonly ISchedulerRuleRespository _schedulerRuleRespository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly List<int> _screeningTemplateIdLists = new List<int>();
        private readonly IScreeningTemplateValueEditCheckRepository _screeningTemplateValueEditCheckRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IMapper _mapper;
        private readonly IEditCheckImpactService _editCheckImpactService;
        private readonly IEditCheckFormulaRepository _editCheckFormulaRepository;
        public EditCheckImpactRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IEditCheckImpactService editCheckImpactService,
            IMapper mapper,
            IScreeningTemplateValueEditCheckRepository screeningTemplateValueEditCheckRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            ISchedulerRuleRespository schedulerRuleRespository,
            IEditCheckFormulaRepository editCheckFormulaRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueEditCheckRepository = screeningTemplateValueEditCheckRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _editCheckImpactService = editCheckImpactService;
            _schedulerRuleRespository = schedulerRuleRespository;
            _mapper = mapper;
            _editCheckFormulaRepository = editCheckFormulaRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
        }

        public void CheckValidation(ScreeningTemplate screeningTemplate, List<ScreeningTemplateValue> values, int projectDesignId, int domainId)
        {
            var statusId = (int)screeningTemplate.Status;
            if (statusId > 2) return;

            var result = _editCheckImpactService.GetEditCheck(screeningTemplate, projectDesignId, domainId);

            result.ForEach(r =>
            {
                r.ScreeningEntryId = screeningTemplate.ScreeningEntryId;

                if (r.IsSameTemplate)
                {
                    r.ScreeningTemplateId = screeningTemplate.Id;
                    r.ScreeningTemplateValue = values.FirstOrDefault(c => c.ProjectDesignVariableId == r.ProjectDesignVariableId)?.Value;
                }
                else if (r.ProjectDesignTemplateId != null)
                {
                    var refTemplate = _editCheckImpactService.GetScreeningTemplate((int)r.ProjectDesignTemplateId, screeningTemplate.ScreeningEntryId);
                    if (refTemplate != null)
                    {
                        r.ScreeningTemplateId = refTemplate.Id;
                        r.ScreeningTemplateValue = _editCheckImpactService.GetVariableValue(r);
                        statusId = (int)screeningTemplate.Status;
                        if (statusId > 0)
                        {

                        }
                    }
                }

                if (r.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                {
                    r.CollectionValue = _editCheckImpactService.CollectionValueAnnotation(r.CollectionValue);
                    r.ScreeningTemplateValue = _editCheckImpactService.ScreeningValueAnnotation(r.ScreeningTemplateValue, r.CheckBy);
                }
            });

            string a = "";
        }

        public EditCheckResult ValidateEditCheck(List<EditCheckValidate> editCheck)
        {
            if (editCheck.Any(x => x.IsFormula))
                return _editCheckFormulaRepository.ValidateFormula(editCheck);
            else
                return ValidateRule(editCheck);
        }

        EditCheckResult ValidateRule(List<EditCheckValidate> editCheck)
        {
            var result = ValidateRuleReference(editCheck.Where(r => !r.IsTarget).ToList());
            return null;
        }
        public EditCheckResult ValidateEditCheckReference(List<EditCheckValidate> editCheck)
        {
            if (editCheck.Any(x => x.IsFormula))
                return _editCheckFormulaRepository.ValidateFormulaReference(editCheck);
            else
                return ValidateRuleReference(editCheck);
        }


        EditCheckResult ValidateRuleReference(List<EditCheckValidate> editCheck)
        {
            var result = new EditCheckResult();
            var dt = new DataTable();
            string ruleStr = "";
            editCheck.ForEach(r =>
            {;
            
                if (r.Operator == Operator.In || r.Operator == Operator.NotIn)
                    ruleStr = ruleStr + $"{r.StartParens}{r.FieldName} {r.OperatorName} {"('"}{r.Input1}{"')"}{r.EndParens} {r.LogicalOperator} ";
                else
                    ruleStr = ruleStr + $"{r.StartParens}{r.FieldName} {r.OperatorName} {"'"}{r.Input1}{"'"}{r.EndParens} {r.LogicalOperator} ";

                var col = new DataColumn();
                col.DefaultValue = r.CollectionValue;
                col.ColumnName = r.FieldName;
                dt.Columns.Add(col);
            });
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            try
            {
                var foundDt = dt.Select(ruleStr);
                if (foundDt != null && foundDt.Count() > 0)
                {
                    result.ResultMessage = "Verified Reference!";
                    result.IsValid = true;
                }
                else
                {
                    result.ResultMessage = "Not Verified Reference!";
                    result.IsValid = true;
                }
            }
            catch (Exception ex)
            {
                result.ResultMessage = result.Result + " -> " + ruleStr;
                result.ErrorMessage = ex.Message;
            }
            result.ReferenceString = ruleStr;
            return result;
        }
    }
}
