﻿using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITDetail : BaseEntity
    {
        
        public string KitNo { get; set; }
        public int SupplyManagementKITId { get; set; }
        public int? SupplyManagementShipmentId { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }

        public string RandomizationNo { get; set; }
        public SupplyManagementKIT SupplyManagementKIT { get; set; }
        public SupplyManagementShipment SupplyManagementShipment { get; set; }

        public KitStatus Status { get; set; }

        public string Comments { get; set; }

        public int? RandomizationId { get; set; }

        public int? NoOfImp { get; set; }

        public int? ReturnImp { get; set; }

        public string ReturnReason { get; set; }

        public bool? IsUnUsed { get; set; }

        public int? ReturnBy { get; set; }

        public DateTime? ReturnDate { get; set; }

        public KitStatus? PrevStatus { get; set; }

        public int? ToSiteId { get; set; }

        public bool IsRetension { get; set; }

        public string Barcode { get; set; }
    }
}
