using GSC.Data.Entities.Common;


namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementVisitKITSequenceDetailDto : BaseDto
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

        public int? SupplyManagementKITSeriesdetailId { get; set; }

        public int? SupplyManagementShipmentId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
    public class SupplyManagementVisitKITSequenceDetailGridDto : BaseAuditDto
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
