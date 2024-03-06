using GSC.Common.Base;
using System.Collections.Generic;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementFector : BaseEntity
    {
        public int ProjectId { get; set; }
        public string Formula { get; set; }
        public GSC.Data.Entities.Master.Project Project { get; set; }
        public List<SupplyManagementFectorDetail> FectorDetailList { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public string CheckFormula { get; set; }

        public string SourceFormula { get; set; }

        public string SampleResult { get; set; }

        public string ErrorMessage { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
