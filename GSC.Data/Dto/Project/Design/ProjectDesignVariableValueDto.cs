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

        public int SeqNo { get; set; }
        public string Label { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
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
    }

    public class ProjectDesignVariableValueDropDown 
    {
        public int Id { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
    }

    public class ProjectDesignReportDto
    {
        public string StudyCode { get; set; }
        public string Visit { get; set; }
        public string Template { get; set; }
        public bool IsRepeated { get; set; }
        public bool IsParticipantView { get; set; }
        public string DomainName { get; set; }
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
    }
}