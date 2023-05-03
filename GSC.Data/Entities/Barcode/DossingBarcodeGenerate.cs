using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class DossingBarcodeGenerate : BaseEntity
    {
        public int DossingBarcodeId { get; set; }
        public int BarcodeConfigId { get; set; }
        public string BarcodeString { get; set; }
        public bool? IsRePrint { get; set; }
        public BarcodeConfig BarcodeConfig { get; set; }
        public DossingBarcode DossingBarcode { get; set; }
    }
}
