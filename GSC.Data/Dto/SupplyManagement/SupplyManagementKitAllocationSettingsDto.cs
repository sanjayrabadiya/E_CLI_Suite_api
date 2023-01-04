﻿using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKitAllocationSettingsDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }
        public int NoOfImp { get; set; }
        public string VisitName { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public string ReasonName { get; set; }

        public int PharmacyStudyProductTypeId { get; set; }

        public string ProductName { get; set; }

    }

    public class SupplyManagementKitAllocationSettingsGridDto : BaseAuditDto
    {
        public int ProjectDesignVisitId { get; set; }
        public int NoOfImp { get; set; }
        public string VisitName { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }

        public int PharmacyStudyProductTypeId { get; set; }

        public string ProductName { get; set; }
    }

}
