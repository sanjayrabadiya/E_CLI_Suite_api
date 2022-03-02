using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementReceipt : BaseEntity, ICommonAduit
    {
        public int SupplyManagementShipmentId { get; set; }
        public bool WithIssue { get; set; }
        public int? AuditReasonId { get; set; }       
        public string ReasonOth { get; set; }
        public string Description { get; set; }
        public SupplyManagementShipment SupplyManagementShipment { get; set; }

        public AuditReason AuditReason { get; set; }

        
    }
}
