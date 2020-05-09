using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Barcode
{
    public class BarcodeType : BaseEntity
    {
        public string BarcodeTypeCode { get; set; }

        public string BarcodeTypeName { get; set; }

        public int? CompanyId { get; set; }
    }
}