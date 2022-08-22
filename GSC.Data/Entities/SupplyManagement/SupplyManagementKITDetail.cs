﻿
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITDetail : BaseEntity
    {
        
        public int KitNo { get; set; }
        public int SupplyManagementKITId { get; set; }
        public int? SupplyManagementShipmentId { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }

        public string RandomizationNo { get; set; }
        public SupplyManagementKIT SupplyManagementKIT { get; set; }
        public SupplyManagementShipment SupplyManagementShipment { get; set; }

        public KitStatus? Status { get; set; }

        public string Comments { get; set; }
    }
}