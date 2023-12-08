using GSC.Data.Entities.Common;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class ProductVerificationDto : BaseDto
    {
        public int? ProductReceiptId { get; set; }
        public BatchLotType? BatchLotId { get; set; }
        public string BatchLotNumber { get; set; }
        public string ManufactureBy { get; set; }
        public string MarketedBy { get; set; }
        public string LabelClaim { get; set; }
        public string DistributedBy { get; set; }
        public string PackDesc { get; set; }
        public FileModel FileModel { get; set; }
        // public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public string MarketAuthorization { get; set; }
        public DateTime? MfgDate { get; set; }
        public ReTestExpiry? RetestExpiryId { get; set; }
        public DateTime? RetestExpiryDate { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsSendForApprove { get; set; }

        public PacketType PacketTypeId { get; set; }
        public decimal? Dose { get; set; }

        public int? UnitId { get; set; }

        public string PacketTypeName { get; set; }

        public string UnitName { get; set; }
    }

    public class ProductVerificationGridDto : BaseAuditDto
    {
        public int? ProductReceiptId { get; set; }
        public string StudyCode { get; set; }
        public string StorageArea { get; set; }
        public string ProjectName { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string BatchLot { get; set; }
        public string BatchLotNumber { get; set; }
        public string ManufactureBy { get; set; }
        public string MarketedBy { get; set; }
        public string LabelClaim { get; set; }
        public string DistributedBy { get; set; }
        public string PackDesc { get; set; }
        public string MarketAuthorization { get; set; }
       
        public DateTime? MfgDate { get; set; }
        public string RetestExpiry { get; set; }
        public DateTime? RetestExpiryDate { get; set; }
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
        public int? RetentionSampleQty { get; set; }
        public int RemainingQuantity { get; set; }
        public int? StorageId { get; set; }
        public bool? IsConditionProduct { get; set; }
        public int? ClientId { get; set; }
        public VerificationApprovalTemplate VerificationApprovalTemplate { get; set; }
        public VerificationApprovalTemplateHistory VerificationApprovalTemplateHistory { get; set; }
        public string Status { get; set; }
        public int CentralDepotId { get; set; }
        public int ProductVerificationDetailId { get; set; }

        public string RecieptPath { get; set; }

        public string RecieptMimeType { get; set; }

        public string VerificationPath { get; set; }

        public string VerificationMimeType { get; set; }

        public string PacketTypeName { get; set; }

        public string UnitName { get; set; }

        public decimal? Dose { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
    }
}
