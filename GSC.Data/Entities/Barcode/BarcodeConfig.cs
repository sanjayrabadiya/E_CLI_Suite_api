using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System.Collections.Generic;

namespace GSC.Data.Entities.Barcode
{
    public class BarcodeConfig : BaseEntity, ICommonAduit
    {
        public int AppScreenId { get; set; }
        public int PageId { get; set; }
        public int BarcodeTypeId { get; set; }
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        public BarcodeType BarcodeType { get; set; }
        public AppScreen AppScreen { get; set; }
        public IList<BarcodeCombination> BarcodeCombination { get; set; } = null;
        public IList<BarcodeDisplayInfo> BarcodeDisplayInfo { get; set; } = null;
        public int? CompanyId { get; set; }
    }
}