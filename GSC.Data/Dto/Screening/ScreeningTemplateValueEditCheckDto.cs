using GSC.Helper;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueEditCheckDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningEntryId { get; set; }
        public int EditCheckDetailId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public EditCheckValidateType ValidateType { get; set; }
        public CollectionSources? CollectionSource { get; set; }
    }


    public class VariableEditCheckDto
    {
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningEntryId { get; set; }
        public CollectionSources CollectionSource { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public bool IsFromQuery { get; set; }
    }

    public class ValidateResult
    {
        public int EditChekId { get; set; }
        public string Result { get; set; }
        public string LogicalOperator { get; set; }
        public string Value { get; set; }
        public bool IsReference { get; set; }
    }
}