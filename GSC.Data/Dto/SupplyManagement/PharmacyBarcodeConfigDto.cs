using System.Collections.Generic;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Barcode
{
    public class PharmacyBarcodeConfigDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public BarcodeModuleType BarcodeModuleType { get; set; }
        public BarcodeTypes BarcodeType { get; set; }
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        public IList<PharmacyBarcodeDisplayInfo> BarcodeDisplayInfo { get; set; } = null;
        public string BarcodeTypeName { get; set; }

    }

    public class PharmacyBarcodeConfigGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public BarcodeModuleType BarcodeModuleType { get; set; }
        public BarcodeTypes BarcodeType { get; set; }
        
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        public string BarcodeTypeName { get; set; }
        public string BarcodeDisplayInfo { get; set; }
        
        public string BarcodeModuleTypeName { get; set; }
        public IList<PharmacyBarcodeDisplayInfo> BarcodeDisplayInfoArr { get; set; } = null;
    }
}