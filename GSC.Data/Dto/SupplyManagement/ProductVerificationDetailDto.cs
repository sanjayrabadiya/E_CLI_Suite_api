using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class ProductVerificationDetailDto : BaseDto
    {
        public int ProductReceiptId { get; set; }
        public int ProductVerificationId { get; set; }
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
        public bool IsSendBack { get; set; }
        public bool IsApprove { get; set; }
        public bool IsSendForApprove { get; set; }
        public int? VerificationApprovalTemplateId { get; set; }
        public string Comment { get; set; }
    }
}
