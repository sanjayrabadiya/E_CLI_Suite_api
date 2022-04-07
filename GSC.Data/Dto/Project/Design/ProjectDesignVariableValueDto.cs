using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Helper;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVariableValueDto : BaseDto
    {
        public int ProjectDesignVariableId { get; set; }

        [Required(ErrorMessage = "Value Code is required.")]
        public string ValueCode { get; set; }

        [Required(ErrorMessage = "Value Name is required.")]
        public string ValueName { get; set; }
        public double? InActiveVersion { get; set; }
        public double? StudyVersion { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
        public bool AllowActive { get; set; }
        public string DisplayVersion { get; set; }
    }

    public class ScreeningVariableValueDto
    {
        public int Id { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string ValueName { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
        public string Label { get; set; }
        public int SeqNo { get; set; }
        public double? StudyVersion { get; set; }
        public double? InActiveVersion { get; set; }
        public short? LevelNo { get; set; }
    }


    public class ProjectDesignVariableValueDropDown
    {
        public int Id { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public bool InActive { get; set; }
    }

    public class ProjectDesignReportDto
    {
        public string StudyCode { get; set; }
        public string Period { get; set; }
        public string Visit { get; set; }
        public bool IsVisitRepeated { get; set; }
        public bool IsNonCRF { get; set; }
        public int? VisitOrderId { get; set; }
        public int? TemplateOrderId { get; set; }

        public string Template { get; set; }
        public bool IsRepeated { get; set; }
        public bool IsParticipantView { get; set; }
        public string DomainName { get; set; }
        public int? VariableOrderId { get; set; }

        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public string VariableAlias { get; set; }
        public string VariableAnnotation { get; set; }
        public string VariableCategoryName { get; set; }
        public string Role { get; set; }
        public string CoreType { get; set; }
        public string CollectionSource { get; set; }
        public string DataType { get; set; }
        public bool IsNa { get; set; }
        public string DateValidate { get; set; }
        public string UnitName { get; set; }
        public string UnitAnnotation { get; set; }
        public string CollectionAnnotation { get; set; }
        public string ValidationType { get; set; }
        public int? Length { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
        public string DefaultValue { get; set; }
        public bool? IsDocument { get; set; }
        public string Note { get; set; }
        public bool? IsEncrypt { get; set; }
        public string EncryptRole { get; set; }
        public string CollectionValue { get; set; }
        public int? DisplayValue { get; set; }
        public string AnnotationType { get; set; }
    }

    public class ProjectDesignLanguageReportDto : BaseAuditDto
    {
        public string PeriodName { get; set; }
        public string VisitName { get; set; }
        public string TemplateName { get; set; }
        public string VariableName { get; set; }
        public string Note { get; set; }
        public string VariableValue { get; set; }
        public string Language { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
    }
}