using GSC.Helper;
using System;

namespace GSC.Data.Dto.Screening
{
    public class EditCheckValidateDto
    {
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningEntryId { get; set; }
        public int? RepeatedVisit { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public EditCheckRuleBy CheckBy { get; set; }
        public DataType? DataType { get; set; }
        public int EditCheckDetailId { get; set; }
        public int EditCheckId { get; set; }
        public Operator? Operator { get; set; }
        public int? ProjectDesignTemplateId { get; set; }
        public int? DomainId { get; set; }
        public string CollectionValue { get; set; }
        public string CollectionValue2 { get; set; }
        public bool IsReferenceValue { get; set; }
        public string LogicalOperator { get; set; }
        public string Message { get; set; }
        public bool IsSkip { get; set; }
        public string AutoNumber { get; set; }
        public string VariableAnnotation { get; set; }
        public bool IsSameTemplate { get; set; }
        public bool IsTarget { get; set; }
        public string EndParens { get; set; }
        public string StartParens { get; set; }
        public bool IsFormula { get; set; }
        public bool IsOnlyTarget { get; set; }
        public string ScreeningTemplateValue { get; set; }
        public EditCheckValidateType ValidateType { get; set; }
        public string SampleResult { get; set; }
    }
    public class ScheduleTemplateDto
    {
        public ScreeningTemplateStatus Status { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ScreeningVisitId { get; set; }
    }
    public class ScheduleCheckValidateDto
    {
        public int ProjectScheduleId { get; set; }
        public int ProjectScheduleTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Message { get; set; }
        public string Value { get; set; }
        public string AutoNumber { get; set; }
        public int ScreeningTemplateId { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }
        public int? HH { get; set; }
        public int? MM { get; set; }
        public int PositiveDeviation { get; set; }
        public int NegativeDeviation { get; set; }
        public int? NoOfDay { get; set; }
        public ProjectScheduleOperator? Operator { get; set; }
        public bool IsTarget { get; set; }
        public bool HasQueries { get; set; }
        public ScreeningTemplateStatus? Status { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public EditCheckValidateType ValidateType { get; set; }
    }
}