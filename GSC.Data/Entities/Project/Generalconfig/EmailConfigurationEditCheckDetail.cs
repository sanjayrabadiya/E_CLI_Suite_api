using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;


namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheckDetail : BaseEntity
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public int? ProjectDesignTemplateId { get; set; }
        public int? ProjectDesignVariableId { get; set; }
        public Operator Operator { get; set; }
        public string LogicalOperator { get; set; }
        public string startParens { get; set; }
        public string endParens { get; set; }
        public string CollectionValue { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public EditCheckRuleBy CheckBy { get; set; }

        public string VariableAnnotation { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public EmailConfigurationEditCheck EmailConfigurationEditCheck { get; set; }
    }
}
