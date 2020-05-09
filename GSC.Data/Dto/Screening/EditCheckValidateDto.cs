using GSC.Helper;
using System;

namespace GSC.Data.Dto.Screening
{
    public class EditCheckValidateDto
    {
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningEntryId { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public EditCheckRuleBy CheckBy { get; set; }
        public DataType? DataType { get; set; }
        public int EditCheckDetailId { get; set; }
        public int EditCheckId { get; set; }
        public Operator? Operator { get; set; }
        public int? ProjectDesignTemplateId { get; set; }
        public string CollectionValue { get; set; }
        public string CollectionValue2 { get; set; }
        public bool IsReferenceValue { get; set; }
        public string LogicalOperator { get; set; }
        public string Message { get; set; }
        public string AutoNumber { get; set; }
        public bool IsSameTemplate { get; set; }
        public bool IsTarget { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string ScreeningTemplateValue { get; set; }
    }

    public class TemplateValueListDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningEntryId { get; set; }
        public int EditCheckDetailId { get; set; }
        public int EditCheckId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public bool IsTarget { get; set; }
    }
}