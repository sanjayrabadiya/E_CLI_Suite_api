﻿using GSC.Common.Base;
using GSC.Common.Common;


namespace GSC.Data.Entities.SupplyManagement
{
    public class ProductVerificationDetail : BaseEntity, ICommonAduit
    {
        public int ProductReceiptId { get; set; }        
        public int? QuantityVerification { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool? IsAssayRequirement { get; set; }
        public bool? IsRetentionConfirm { get; set; }
        public bool? IsSodiumVaporLamp { get; set; }
        public bool? IsProductDescription { get; set; }
        public int? NumberOfBox { get; set; }
        public int? NumberOfQty { get; set; }
        public int? ReceivedQty { get; set; }
        public bool? IsConditionProduct { get; set; }
        public int? CompanyId { get; set; }     
        public int? RetentionSampleQty { get; set; }
        public int CentralDepotId { get; set; }
        public CentralDepot CentralDepot { get; set; }
        public int RemainingQuantity { get; set; }
        public ProductReceipt ProductReceipt { get; set; }
    }
}
