using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Barcode
{
    public class BarcodeConfigDto : BaseDto
    {
        public int AppScreenId { get; set; }
        public string ModuleName { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
        public int BarcodeTypeId { get; set; }
        public string BarcodeTypeName { get; set; }
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        public IList<BarcodeCombination> BarcodeCombination { get; set; } = null;
        public IList<BarcodeDisplayInfo> BarcodeDisplayInfo { get; set; } = null;
        public int? CompanyId { get; set; }
        public List<int> BarcodeCombinationList { get; set; } = null;
    }

    public class BarcodeConfigGridDto : BaseAuditDto
    {
        public int id { get; set; }
        public int AppScreenId { get; set; }
        public string ModuleName { get; set; }
        public int PageId { get; set; }
        public string PageName { get; set; }
        public int BarcodeTypeId { get; set; }
        public string BarcodeTypeName { get; set; }
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        public string BarcodeCombination { get; set; }
        public string BarcodeDisplayInfo { get; set; }

        public IList<BarcodeDisplayInfo> BarcodeDisplayInfoArr { get; set; } = null;
    }
}