using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Barcode
{
    public class CentrifugationDetailsDto : BaseDto
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
        public int? MissedBy { get; set; }
        public DateTime? MissedOn { get; set; }
    }

    public class CentrifugationDetailsGridDto : BaseAuditDto
    {
        public int PKBarcodeId { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string RandomizationNumber { get; set; }
        public string PKBarcode { get; set; }
        public string PKActualTime { get; set; }
        public DateTime? CentrifugationStartTime { get; set; }
        public string CentrifugationByUser { get; set; }
        public DateTime? CentrifugationOn { get; set; }
        public string Status { get; set; }
        public string ReCentrifugationByUser { get; set; }
        public DateTime? ReCentrifugationOn { get; set; }
        public string AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string MissedBy { get; set; }
        public DateTime? MissedOn { get; set; }
        public string ReCentrifugateReason { get; set; }
        public string ReCentrifugateReasonOth { get; set; }
    }

    public class ReCentrifugationDto : BaseDto
    {
        public int[] Ids { get; set; }
        public int AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }

}
