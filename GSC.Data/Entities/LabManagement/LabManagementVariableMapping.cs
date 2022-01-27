using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementVariableMapping : BaseEntity
    {
        public int LabManagementConfigurationId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string TargetVariable { get; set; }
        public LabManagementConfiguration LabManagementConfiguration { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public decimal? MaleLowRange { get; set; }
        public decimal? MaleHighRange { get; set; }
        public decimal? FemaleLowRange { get; set; }
        public decimal? FemaleHighRange { get; set; }
        public string Unit { get; set; }
    }
}
