using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        //public int? CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
    }

    public class VariableGridDto : BaseAuditDto
    {
        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public string VariableAlias { get; set; }
        public int? AnnotationTypeId { get; set; }
        public string Annotation { get; set; }
        public string CollectionAnnotation { get; set; }
        public string DefaultValue { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
    }
}