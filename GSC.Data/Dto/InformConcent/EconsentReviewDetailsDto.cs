using GSC.Data.Entities.Common;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentReviewDetailsDto : BaseDto
    {
        public int Id { get; set; }
        public int RandomizationId { get; set; }
        public int EconsentSetupId { get; set; }
        public bool IsReviewDoneByInvestigator { get; set; }
        public int ReviewDoneByRoleId { get; set; }
        public string RandomizationName { get; set; }
        public string EconsentDocumentName { get; set; }
        public string ApprovedByRole { get; set; }
        public int TotalNoSection { get; set; }
        public DateTime? investigatorRevieweddatetime { get; set; }
        public DateTime? patientapproveddatetime { get; set; }
        public string? pdfpath { get; set; }
        public string? documentData { get; set; }
        public bool IsReviewedByPatient { get; set; }

        public string? patientdigitalSignBase64 { get; set; }
        //public string? investigatordigitalSignBase64 { get; set; }
        public string? patientdigitalSignImagepath { get; set; }
        //public string? investigatordigitalSignImagepath { get; set; }
        public List<EconsentReviewDetailsSectionsDto> EconsentReviewDetailsSections { get; set; }
        public int? ReviewDoneByUserId { get; set; }
        public bool? IsApproved { get; set; }
        public int? ApproveRejectReasonId { get; set; }
        public string? ApproveRejectReasonOth { get; set; }

    }
}
