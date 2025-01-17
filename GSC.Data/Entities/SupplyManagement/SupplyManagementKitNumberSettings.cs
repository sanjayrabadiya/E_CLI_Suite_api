﻿
using GSC.Common.Base;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKitNumberSettings : BaseEntity
    {
        public int ProjectId { get; set; }
        public int KitNumberLength { get; set; }
        public int KitNumberStartWIth { get; set; }
        public string Prefix { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public KitCreationType KitCreationType { get; set; }
        public GSC.Data.Entities.Master.Project Project { get; set; }
        public int KitNoseries { get; set; }
        public bool? IsBlindedStudy { get; set; }
        public int? ThresholdValue { get; set; }
        public bool IsUploadWithKit { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
        public bool IsDoseWiseKit { get; set; }
        public bool IsBarcodeScan { get; set; }
        public bool? IsStaticRandomizationNo { get; set; }

    }
}
