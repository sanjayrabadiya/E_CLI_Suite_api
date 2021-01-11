using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class Variable : BaseEntity, ICommonAduit
    {
        public string VariableName { get; set; }
        public string VariableCode { get; set; }
        public string VariableAlias { get; set; }
        public int? DomainId { get; set; }
        public Domain Domain { get; set; }
        public string CDISCValue { get; set; }
        public string CDISCSubValue { get; set; }
        public CoreVariableType CoreVariableType { get; set; }
        public RoleVariableType RoleVariableType { get; set; }
        public CollectionSources CollectionSource { get; set; }
        public string CollectionAnnotation { get; set; }
        public int? VariableCategoryId { get; set; }
        public AnnotationType AnnotationType { get; set; }
        public int? AnnotationTypeId { get; set; }
        public string Annotation { get; set; }
        public int? CompanyId { get; set; }
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
        public Unit Unit { get; set; }
        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public IList<VariableRemarks> Remarks { get; set; } = null;
        public Alignment? Alignment { get; set; }
    }
}