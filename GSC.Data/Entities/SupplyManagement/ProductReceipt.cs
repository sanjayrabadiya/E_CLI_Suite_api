using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class ProductReceipt : BaseEntity, ICommonAduit
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
        // public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public Entities.Master.Project Project { get; set; }
        public CentralDepot CentralDepot { get; set; }
        public PharmacyStudyProductType PharmacyStudyProductType { get; set; }
        public ProductVerificationStatus? Status { get; set; }
    }
}
