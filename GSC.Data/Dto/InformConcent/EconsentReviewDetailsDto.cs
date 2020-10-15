using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentReviewDetailsDto : BaseDto
    {
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public int EconsentDocumentId { get; set; }
        public bool IsApprovedByInvestigator { get; set; }
        public int ApprovedByRoleId { get; set; }
        public string AttendanceName { get; set; }
        public string EconsentDocumentName { get; set; }
        public string ApprovedByRole { get; set; }
        public int TotalNoSection { get; set; }
        public DateTime? investigatorapproveddatetime { get; set; }
        public DateTime? patientapproveddatetime { get; set; }
        public string? pdfpath { get; set; }
        public string? documentData { get; set; }
        public bool IsReviewedByPatient { get; set; }

        public string? patientdigitalSignBase64 { get; set; }
        //public string? investigatordigitalSignBase64 { get; set; }
        public string? patientdigitalSignImagepath { get; set; }
        //public string? investigatordigitalSignImagepath { get; set; }
    }
}
