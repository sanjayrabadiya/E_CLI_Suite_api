using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class ProductVerification : BaseEntity, ICommonAduit
    {
        public int? ProductReceiptId { get; set; }
        public BatchLotType? BatchLotId { get; set; }
        public string BatchLotNumber { get; set; }
        public string ManufactureBy { get; set; }
        public string MarketedBy { get; set; }
        public string LabelClaim { get; set; }
        public string DistributedBy { get; set; }
        public string PackDesc { get; set; }
        public string MarketAuthorization { get; set; }
        public DateTime? MfgDate { get; set; }
        public ReTestExpiry? RetestExpiryId { get; set; }
        public DateTime? RetestExpiryDate { get; set; }
        public int? CompanyId { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
    }
}
