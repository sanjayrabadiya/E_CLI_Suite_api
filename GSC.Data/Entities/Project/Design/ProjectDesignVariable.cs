using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Data.Entities.Master;
using GSC.Helper;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariable : BaseEntity, ICommonAduit
    {
        public int ProjectDesignTemplateId { get; set; }
        public int? VariableId { get; set; }
        public string VariableName { get; set; }
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
        public DataType? DataType { get; set; }
        public int? Length { get; set; }
        public string DefaultValue { get; set; }
        public string LowRangeValue { get; set; }
        public string HighRangeValue { get; set; }
        public int? UnitId { get; set; }
        public string UnitAnnotation { get; set; }
        public PrintType PrintType { get; set; }
        public bool IsDocument { get; set; }
        public IList<ProjectDesignVariableValue> Values { get; set; } = null;
        public IList<ProjectDesignVariableRemarks> Remarks { get; set; } = null;
        public Unit Unit { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public int DesignOrder { get; set; }
        public VariableCategory VariableCategory { get; set; }

        [NotMapped] public string VariableCategoryName { get; set; }

        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public Domain Domain { get; set; }
        public string Note { get; set; }
        public List<VariableLanguage> VariableLanguage { get; set; }
    }
}