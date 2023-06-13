using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class ProductReceiptDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int CentralDepotId { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }
        public string ProductName { get; set; }
        public string ReceivedFromLocation { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReferenceNo { get; set; }
        public string ShipmentNo { get; set; }
        public string ConditionOfPackReceived { get; set; }
        public string TransporterName { get; set; }
        public FileModel FileModel { get; set; }
        // public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public DepotType? DepotType { get; set; }
        public ProductVerificationStatus? Status { get; set; }
        public int? CountryId { get; set; }
    }

    public class ProductReceiptGridDto : BaseAuditDto
    {
        public string StudyCode { get; set; }
        public string Status { get; set; }
        public string StorageArea { get; set; }
        public string PharmacyStudyProductType { get; set; }
        public string ProductName { get; set; }
        public string ReceivedFromLocation { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReferenceNo { get; set; }
        public string ShipmentNo { get; set; }
        public string ConditionOfPackReceived { get; set; }
        public string TransporterName { get; set; }
        // public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public int CentralDepotId { get; set; }

        public int? CountryId { get; set; }
        public int? ProductVerificationDetaild { get; set; }

        public string CountryName { get; set; }

        public string PacketTypeName { get; set; }

        public string UnitName { get; set; }

        public decimal? Dose { get; set; }
    }
}
