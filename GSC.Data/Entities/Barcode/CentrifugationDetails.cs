using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Barcode
{
    public class CentrifugationDetails : BaseEntity, ICommonAduit
    {
        public int PKBarcodeId { get; set; }
        public DateTime CentrifugationStartTime { get; set; }
        public int CentrifugationBy { get; set; }
        public DateTime CentrifugationOn { get; set; }
        public CentrifugationFilter Status { get; set; }
        public int? ReCentrifugationBy { get; set; }
        public DateTime? ReCentrifugationOn { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public PKBarcode PKBarcode { get; set; }
        public AuditReason AuditReason { get; set; }

        [ForeignKey("CentrifugationBy")]
        public User Centrifugationed { get; set; }

        [ForeignKey("ReCentrifugationBy")]
        public User ReCentrifugation { get; set; }

        public int? MissedBy { get; set; }
        public DateTime? MissedOn { get; set; }

        public int? ReCentrifugateReason { get; set; }
        public string ReCentrifugateReasonOth { get; set; }
    }
}
