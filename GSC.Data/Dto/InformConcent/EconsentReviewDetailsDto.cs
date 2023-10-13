using GSC.Data.Entities.Common;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentReviewDetailsDto : BaseAuditDto
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
        public DateTime? InvestigatorRevieweddatetime { get; set; }
        public DateTime? Patientapproveddatetime { get; set; }
        public string? Pdfpath { get; set; }
        public string? DocumentData { get; set; }
        public bool IsReviewedByPatient { get; set; }

        public string? PatientdigitalSignBase64 { get; set; }
        //public string? investigatordigitalSignBase64 { get; set; }
        public string? PatientdigitalSignImagepath { get; set; }
        //public string? investigatordigitalSignImagepath { get; set; }
        public List<EconsentReviewDetailsSectionsDto> EconsentReviewDetailsSections { get; set; }
        public int? ReviewDoneByUserId { get; set; }
        public bool? IsApproved { get; set; }
        public int? ApproveRejectReasonId { get; set; }
        public string? ApproveRejectReasonOth { get; set; }
        public bool? IsLAR { get; set; }

    }

    public class EconsentDocumentDetailsDto
    {
        public int Id { get; set; }
        public string EconsentDocumentName { get; set; }
        public bool IsReviewedByPatient { get; set; }
        public bool IsReviewDoneByInvestigator { get; set; }
    }

    public class EconsentDocumetViwerDto
    {
        public int EconcentReviewDetailsId { get; set; }
        public string PatientdigitalSignBase64 { get; set; }
        //public bool IsWithdraw { get; set; }
        public string FileExtension { get; set; }
        public List<EconsentReviewDetailsSectionsDto> EconsentReviewDetailsSections { get; set; }
    }

    public class AppEConsentSection
    {
        public bool isReference { get; set; }
        public string SectionHtml { get; set; }
    }
}
