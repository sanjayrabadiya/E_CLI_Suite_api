using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementVisitKITDetailDto : BaseDto
    {
        public string KitNo { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int RandomizationId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ProductCode { get; set; }

        public string ReasonName { get; set; }

        public string ScreeningNo { get; set; }

        public string RandomizationNo { get; set; }

        public string VisitName { get; set; }

        public string ProjectCode { get; set; }

        public string SiteCode { get; set; }

        public int ParentProjectId { get; set; }

        public int ProjectId { get; set; }

        public int? SupplyManagementKITDetailId { get; set; }

        public string ExpiryMesage { get; set; }
    }
    public class SupplyManagementVisitKITDetailGridDto : BaseAuditDto
    {
        public string KitNo { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int RandomizationId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ProductCode { get; set; }

        public string ReasonName { get; set; }

        public string ScreeningNo { get; set; }

        public string RandomizationNo { get; set; }

        public string VisitName { get; set; }

        public string ProjectCode { get; set; }

        public string SiteCode { get; set; }

        public int ParentProjectId { get; set; }

        public int ProjectId { get; set; }
    }
}
