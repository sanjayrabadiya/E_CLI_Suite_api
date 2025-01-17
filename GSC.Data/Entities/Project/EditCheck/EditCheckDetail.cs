﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Entities.Project.EditCheck
{
    public class EditCheckDetail : BaseEntity, ICommonAduit
    {
        public int EditCheckId { get; set; }
        public EditCheckRuleBy CheckBy { get; set; }
        public bool ByAnnotation { get; set; }
        public int? ProjectDesignTemplateId { get; set; }
        public int? ProjectDesignVariableId { get; set; }
        public string VariableAnnotation { get; set; }
        public int? DomainId { get; set; }
        public Operator? Operator { get; set; }
        public string CollectionValue { get; set; }
        public bool IsTarget { get; set; }
        public string Message { get; set; }
        public string QueryFormula { get; set; }
        public EditCheck EditCheck { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public Domain Domain { get; set; }
        public bool IsSameTemplate { get; set; }
        public bool IsReferenceValue { get; set; }
        public string CollectionValue2 { get; set; }
        public string LogicalOperator { get; set; }
        public string StartParens { get; set; }
        public string EndParens { get; set; }
        public int? FetchingProjectDesignTemplateId { get; set; }
        public int? FetchingProjectDesignVariableId { get; set; }

        public int? ProjectDesignVisitId { get; set; }

        public ProjectDesignVisit ProjectDesignVisit { get; set; }

        public bool? IsAdditionalTarget { get; set; }
        public Operator? AdditionalTargetOperator { get; set; }
        public string AdditionalTargetValue { get; set; }


    }
}