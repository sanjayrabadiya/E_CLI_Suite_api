using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementFactorMapping : BaseEntity
    {
        public int ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public Fector Factor { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public GSC.Data.Entities.Master.Project Project { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }

        public AuditReason AuditReason { get; set; }

    }
}
