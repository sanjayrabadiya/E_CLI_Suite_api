using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKitNumberSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int KitNumberLength { get; set; }
        public int KitNumberStartWIth { get; set; }
        public string Prefix { get; set; }
        public string ProjectCode { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public int KitNoseries { get; set; }
        public bool? IsBlindedStudy { get; set; }

        public KitCreationType KitCreationType { get; set; }

        public int? ThresholdValue { get; set; }

    }

    public class SupplyManagementKitNumberSettingsGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public int KitNumberLength { get; set; }
        public int KitNumberStartWIth { get; set; }
        public string Prefix { get; set; }
        public string ProjectCode { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public bool? IsBlindedStudy { get; set; }
        public int? ThresholdValue { get; set; }
        public string  KitCreationTypeName { get; set; }
    }

}
