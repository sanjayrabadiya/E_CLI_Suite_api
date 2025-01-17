﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementAllocation : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int? PharmacyStudyProductTypeId { get; set; }
        public SupplyManagementAllocationType? Type { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

        public PharmacyStudyProductType PharmacyStudyProductType { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }

    }
}
