using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;

namespace GSC.Data.Entities.Barcode
{
    public class SampleSeparation : BaseEntity, ICommonAduit
    {
        public int PKBarcodeId { get; set; }
        public int SampleBarcodeId { get; set; }
        public DateTime SampleStartTime { get; set; }
        public SampleSeparationFilter Status { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public PKBarcode PKBarcode { get; set; }
        public SampleBarcode SampleBarcode { get; set; }
        public AuditReason AuditReason { get; set; }
        public string SampleBarcodeString {get;set;}
    }
}
