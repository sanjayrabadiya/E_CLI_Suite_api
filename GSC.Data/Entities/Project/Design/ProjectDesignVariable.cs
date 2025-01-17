﻿using System.Collections.Generic;
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
        public PrintType? PrintType { get; set; }
        public bool IsDocument { get; set; }
        public bool IsEncrypt { get; set; }
        public double? InActiveVersion{ get; set; }
        public double? StudyVersion { get; set; }
        public IList<ProjectDesignVariableValue> Values { get; set; } = null;
        public Unit Unit { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public int DesignOrder { get; set; }
        public VariableCategory VariableCategory { get; set; }
        public int? RelationProjectDesignVariableId { get; set; }

        [NotMapped] 
        public string VariableCategoryName { get; set; }

        public VariableCategoryType? SystemType { get; set; }
        public bool IsNa { get; set; }
        public DateValidateType? DateValidate { get; set; }
        public Domain Domain { get; set; }
        public string Note { get; set; }
        public string Label { get; set; }
        public List<VariableLanguage> VariableLanguage { get; set; }
        public List<VariableNoteLanguage> VariableNoteLanguage { get; set; }
        public List<VariableLabelLanguage> VariableLabelLanguage { get; set; }
        public Alignment? Alignment { get; set; }

        public IList<ProjectDesignVariableEncryptRole> Roles { get; set; } = null;
        public double? LargeStep { get; set; }
        public AnnotationType AnnotationType { get; set; }

        public bool? DisplayStepValue { get; set; }
        public bool IsHide { get; set; }
        public bool? IsLevelNo { get; set; }
        public string PreLabel { get; set; }
        public ScaleType? ScaleType { get; set; }
        public bool? DisplayValue { get; set; }
    }
}