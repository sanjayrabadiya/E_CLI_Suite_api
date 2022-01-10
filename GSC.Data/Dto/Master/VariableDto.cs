using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using DataType = GSC.Helper.DataType;

namespace GSC.Data.Dto.Master
{
    public class VariableDto : BaseDto
    {
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

        //public int? CompanyId { get; set; }

        public ValidationType ValidationType { get; set; }

        public DataType? DataType { get; set; }
        public int? Length { get; set; }

        public string DefaultValue { get; set; }

        public string LowRangeValue { get; set; }

        public string HighRangeValue { get; set; }

        public int? UnitId { get; set; }
        public string UnitAnnotation { get; set; }
        public PrintType? PrintType { get; set; }
        public bool IsDocument { get; set; }
        public IList<VariableValue> Values { get; set; } = null;
        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }

        public int? CompanyId { get; set; }
        public int? LargeStep { get; set; }
        //   public IList<VariableRemarks> Remarks { get; set; } = null;
        public Alignment? Alignment { get; set; }
        public bool CollectionValueDisable { get; set; }
    }

    public class VariableGridDto : BaseAuditDto
    {
        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public string DomainName { get; set; }
        public string CategoryName { get; set; }
        public string VariableAlias { get; set; }
        public string AnnotationType { get; set; }
        public string RoleVariableType { get; set; }
        public string CoreVariableType { get; set; }
        public string Unit { get; set; }
        public string UnitAnnotation { get; set; }
        public string DataType { get; set; }
        public string CollectionSource { get; set; }
        public int? Length { get; set; }
        public int? AnnotationTypeId { get; set; }
        public string Annotation { get; set; }
        public string CollectionAnnotation { get; set; }
        public string ValidationType { get; set; }
        public string DateValidate { get; set; }
        public string DefaultValue { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
        public string CollectionValue { get; set; }
        public int? LargeStep { get; set; }
        public VariableCategoryType? SystemType { get; set; }
    }


    public class VerificationApprovalVariableDto
    {
        public int Id { get; set; }
        public int VariableTemplateId { get; set; }
        public int? VariableId { get; set; }
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
        public PrintType? PrintType { get; set; }
        public IList<VerificationApprovalVariableValueDto> Values { get; set; } = null;
        // public IList<ScreeningVariableRemarksDto> Remarks { get; set; } = null;
        public string UnitName { get; set; }
        public int? DesignOrder { get; set; }
        public string VerificationApprovalValue { get; set; }
        public int VerificationApprovalTemplateValueId { get; set; }
        public string VerificationApprovalValueOld { get; set; }
        public string VariableCategoryName { get; set; }
        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public bool IsNaValue { get; set; }
        public bool IsSystem { get; set; }
        public string Note { get; set; }
        public string ValidationMessage { get; set; }
        public Alignment? Alignment { get; set; }
        public int? LargeStep { get; set; }

    }

    public class ManageMonitoringVariableDto : BaseAuditDto
    {
        public int Id { get; set; }
        public int VariableTemplateId { get; set; }
        public int? VariableId { get; set; }
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
        public PrintType? PrintType { get; set; }
        public IList<ManageMonitoringReportVariableValueDto> Values { get; set; } = null;
        public string UnitName { get; set; }
        public int? DesignOrder { get; set; }
        public string VariableValue { get; set; }
        public int ManageMonitoringReportVariableId { get; set; }
        public string VariableValueOld { get; set; }
        public string VariableCategoryName { get; set; }
        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public bool IsNaValue { get; set; }
        public bool IsSystem { get; set; }
        public string Note { get; set; }
        public string ValidationMessage { get; set; }
        public Alignment? Alignment { get; set; }
        public int? LargeStep { get; set; }
        public bool HasComments { get; set; }
        public bool IsReviewPerson { get; set; }
        public CtmsCommentStatus QueryStatus { get; set; }
    }
}