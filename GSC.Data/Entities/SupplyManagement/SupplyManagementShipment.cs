﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementShipment : BaseEntity, ICommonAduit
    {
        public int SupplyManagementRequestId { get; set; }
        public SupplyMangementShipmentStatus Status { get; set; }
        public int ApprovedQty { get; set; }
        public string ShipmentNo { get; set; }
        public string CourierName { get; set; }
        public DateTime? CourierDate { get; set; }
        public string CourierTrackingNo { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public DateTime? EstimatedCourierDate { get; set; }
        public SupplyManagementRequest SupplyManagementRequest { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

    }
}
