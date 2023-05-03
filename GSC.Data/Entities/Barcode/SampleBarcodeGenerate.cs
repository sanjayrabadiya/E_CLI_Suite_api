using GSC.Common.Base;
using GSC.Data.Entities.Attendance;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class SampleBarcodeGenerate : BaseEntity
    {
        public int SampleBarcodeId { get; set; }
        public int BarcodeConfigId { get; set; }
        public string BarcodeString { get; set; }
        public bool? IsRePrint { get; set; }
        public BarcodeConfig BarcodeConfig { get; set; }
        public SampleBarcode SampleBarcode { get; set; }
    }
}
