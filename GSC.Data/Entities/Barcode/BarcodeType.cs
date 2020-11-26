using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Barcode
{
    public class BarcodeType : BaseEntity, ICommonAduit
    {
        public string BarcodeTypeCode { get; set; }

        public string BarcodeTypeName { get; set; }

        public int? CompanyId { get; set; }
    }
}