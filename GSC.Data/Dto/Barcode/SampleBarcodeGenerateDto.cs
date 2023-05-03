using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Barcode
{
    public class SampleBarcodeGenerateDto : BaseDto
    {
        public int SampleBarcodeId { get; set; }
        public int BarcodeConfigId { get; set; }
        public string BarcodeString { get; set; }
        public bool? IsRePrint { get; set; }
        public BarcodeConfig BarcodeConfig { get; set; }
    }
    public class SampleBarcodeGenerateGridDto : BaseDto
    {
        public string BarcodeString { get; set; }
        public bool? IsRePrint { get; set; }
        public string BarcodeType { get; set; }
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        public IList<BarcodeDisplayInfo> BarcodeDisplayInfo { get; set; } = null;
    }
}
