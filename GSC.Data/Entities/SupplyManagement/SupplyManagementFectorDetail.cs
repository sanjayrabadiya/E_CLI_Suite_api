using GSC.Common.Base;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Helper;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementFectorDetail : BaseEntity
    {
        public int SupplyManagementFectorId { get; set; }
        public Fector Fector { get; set; }
        public FectoreType? Type { get; set; }
        public FectorOperator Operator { get; set; }
        public string ProductTypeCode { get; set; }
        public string CollectionValue { get; set; }
        public string LogicalOperator { get; set; }
        public int? Ratio { get; set; }
        public SupplyManagementFector SupplyManagementFector { get; set; }

        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public string StartParens { get; set; }
        public string EndParens { get; set; }
    }
}
