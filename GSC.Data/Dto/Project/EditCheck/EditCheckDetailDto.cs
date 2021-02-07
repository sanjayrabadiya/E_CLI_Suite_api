using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Dto.Project.EditCheck
{
    public class EditCheckDetailDto : BaseDto
    {
        public int EditCheckId { get; set; }
        public EditCheckRuleBy CheckBy { get; set; }
        public string CheckByName { get; set; }
        public bool ByAnnotation { get; set; }
        public int? ProjectDesignTemplateId { get; set; }
        public int? ProjectDesignVisitId { get; set; }
        public int? ProjectDesignPeriodId { get; set; }
        public int? ProjectDesignVariableId { get; set; }
        public int ProjectDesignId { get; set; }
        public string VariableAnnotation { get; set; }
        public int? DomainId { get; set; }
        public Operator? Operator { get; set; }
        public string OperatorName { get; set; }
        public string CollectionValue { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public DataType? DataType { get; set; }
        public bool IsTarget { get; set; }
        public string TargetName { get; set; }
        public string Message { get; set; }
        public string QueryFormula { get; set; }
        public string FieldName { get; set; }
        public string PeriodName { get; set; }
        public string TemplateName { get; set; }
        public string VariableName { get; set; }
        public string VisitName { get; set; }
        public string DomainName { get; set; }

        public string CheckFormula { get; set; }
        public bool IsFormula { get; set; }
        public int[] VariableIds { get; set; }
        public int Sort { get; set; }
        public bool IsSameTemplate { get; set; }
        public bool IsReferenceValue { get; set; }
        public string CollectionValue2 { get; set; }
        public string LogicalOperator { get; set; }
        public bool IsOnlyTarget { get; set; }
        public List<ProjectDesignVariableValueDropDown> ExtraData { get; set; }
        public string StartParens { get; set; }
        public string EndParens { get; set; }
        public int? FetchingProjectDesignTemplateId { get; set; }
        public int? FetchingProjectDesignVariableId { get; set; }
    }

    public class EditCheckValidate
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public string CollectionValue { get; set; }
        public string OperatorName { get; set; }
        public string CollectionValue2 { get; set; }
        public string LogicalOperator { get; set; }
        public string EndParens { get; set; }
        public string StartParens { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public DataType? DataType { get; set; }
        public string InputValue { get; set; }
        public string RefernceFieldName { get; set; }
        public bool IsReferenceValue { get; set; }
        public Operator? Operator { get; set; }
        public bool IsFormula { get; set; }
        public bool IsSkip { get; set; }
        public bool IsTarget { get; set; }
    }

    public class EditCheckResult
    {
        public int Id { get; set; }
        public bool IsValid { get; set; }
        public string SampleText { get; set; }
        public string Result { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorMessage { get; set; } = "";
        public List<EditCheckResult> Target { get; set; }

    }

}