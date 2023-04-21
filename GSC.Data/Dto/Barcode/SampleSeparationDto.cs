using GSC.Data.Entities.Common;
using GSC.Helper;
using System;

namespace GSC.Data.Dto.Barcode
{
    public class SampleSeparationDto : BaseDto
    {
        public int PKBarcodeId { get; set; }
        public int SampleBarcodeId { get; set; }
        public DateTime SampleStartTime { get; set; }
        public SampleSeparationFilter Status { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }

    public class SampleSeparationGridDto : BaseAuditDto
    {
        public int PKBarcodeId { get; set; }
        public int SampleBarcodeId { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string RandomizationNumber { get; set; }
        public string PKBarcode { get; set; }
        public string PKActualTime { get; set; }
        public DateTime? SampleStartTime { get; set; }
        public string Status { get; set; }
        public string AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string SampleBarcode { get; set; }
        public string SampleUserBy { get; set; }
        public DateTime? SampleOn { get; set; }
        public string Template { get; set; }
        public string ActionBy { get; set; }
        public DateTime? ActionOn { get; set; }
    }

    public class SampleSaveSeparationDto : BaseDto
    {
        public int PKBarcodeId { get; set; }
        public int SampleBarcodeId { get; set; }
        public string SampleBarcodeString { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
    }

}
