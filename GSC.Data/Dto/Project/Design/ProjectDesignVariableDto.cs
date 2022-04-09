using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;
using DataType = GSC.Helper.DataType;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVariableDto : BaseDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public int? VariableId { get; set; }

        [Required(ErrorMessage = "Variable Name is required.")]
        public string VariableName { get; set; }

        [Required(ErrorMessage = "Variable Code is required.")]
        public string VariableCode { get; set; }

        public string VariableAlias { get; set; }
        public int? DomainId { get; set; }
        public string CDISCValue { get; set; }
        public string CDISCSubValue { get; set; }
        public CoreVariableType CoreVariableType { get; set; }
        public RoleVariableType RoleVariableType { get; set; }
        public CollectionSources CollectionSource { get; set; }
        public string CollectionAnnotation { get; set; }
        public int? VariableCategoryId { get; set; }
        public int? AnnotationTypeId { get; set; }
        public string Annotation { get; set; }
        public ValidationType ValidationType { get; set; }
        public ValidationType OriginalValidationType { get; set; }
        public DataType? DataType { get; set; }
        public int? Length { get; set; }
        public string DefaultValue { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
        public int? UnitId { get; set; }
        public string UnitAnnotation { get; set; }
        public PrintType? PrintType { get; set; }
        public IList<ProjectDesignVariableValueDto> Values { get; set; } = null;
       
        public UnitDto Unit { get; set; }
        public int DesignOrder { get; set; }
        public bool? IsDocument { get; set; }
        public bool? IsEncrypt { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public string ScreeningValueOld { get; set; }
        public bool? IsValid { get; set; }
        public bool HasComments { get; set; }
        public bool HasQueries { get; set; }
        public string DocPath { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public string VariableCategoryName { get; set; }
        public WorkFlowButton WorkFlowButton { get; set; }
        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public int? RelationProjectDesignVariableId { get; set; }
        public bool IsNaValue { get; set; }
        public bool IsSystem { get; set; }
        public string Note { get; set; }
        public EditCheckTargetValidation EditCheckValidation { get; set; }
        public Alignment? Alignment { get; set; }
        public IList<ProjectDesignVariableEncryptRoleDto> Roles { get; set; } = null;
        public int? LargeStep { get; set; }
        public double? InActiveVersion{ get; set; }
        public double? StudyVersion { get; set; }
        public bool CollectionValueDisable { get; set; }
        public bool? DisplayStepValue { get; set; }
    }

    public class DesignScreeningVariableDto
    {
        public int Id { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int? ProjectDesignVariableId { get; set; }
        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public CollectionSources CollectionSource { get; set; }
        public ValidationType ValidationType { get; set; }
        public ValidationType OriginalValidationType { get; set; }
        public DataType? DataType { get; set; }
        public int? Length { get; set; }
        public string DefaultValue { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
        public PrintType PrintType { get; set; }
        public IList<ScreeningVariableValueDto> Values { get; set; } = null;

        public string UnitName { get; set; }
        public int DesignOrder { get; set; }
        public bool? IsDocument { get; set; }
        public bool? IsEncrypt { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }
        public string ScreeningValueOld { get; set; }
        public bool? IsValid { get; set; }
        public bool? IsSaved { get; set; }
        public bool HasComments { get; set; }
        public bool HasQueries { get; set; }
        public string DocPath { get; set; }
        public string DocFullPath { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public string VariableCategoryName { get; set; }
        public WorkFlowButton WorkFlowButton { get; set; }
        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public bool IsNaValue { get; set; }
        public bool IsSystem { get; set; }
        public string Note { get; set; }
        public List<EditCheckIds> editCheckIds { get; set; }
        public EditCheckTargetValidation EditCheckValidation { get; set; }
        public string ValidationMessage { get; set; }
        public Alignment? Alignment { get; set; }

        public int? RelationProjectDesignVariableId { get; set; }
        public int? LargeStep { get; set; }
        public double? StudyVersion { get; set; }
        public double? InActiveVersion { get; set; }
        public int? LabManagementUploadExcelDataId { get; set; }
        public bool? DisplayStepValue { get; set; }

    }

  

    public class WorkFlowButton
    {
        public bool Generate { get; set; }
        public bool Update { get; set; }
        public bool Review { get; set; }
        public bool Clear { get; set; }
        public bool Acknowledge { get; set; }
        public bool SelfCorrection { get; set; }
        public bool DeleteQuery { get; set; }
    }
}