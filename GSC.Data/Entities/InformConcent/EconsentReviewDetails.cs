using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentReviewDetails : BaseEntity, ICommonAduit
    {
        public int RandomizationId { get; set; }
        public int EconsentSetupId { get; set; }
        public bool IsReviewDoneByInvestigator { get; set; }
        public int ReviewDoneByRoleId { get; set; }
        public DateTime? InvestigatorReviewedDatetime { get; set; }
        public DateTime? PatientApprovedDatetime { get; set; }
        public string? PdfPath { get; set; }
        public bool IsReviewedByPatient { get; set; }
        public string? PatientdigitalSignImagepath { get; set; }
        public Randomization Randomization { get; set; }
        public EconsentSetup EconsentSetup { get; set; }
        public List<EconsentReviewDetailsSections> EconsentReviewDetailsSections { get; set; }
        public int? ReviewDoneByUserId { get; set; }
        public bool? IsApproved { get; set; }
        public int? ApproveRejectReasonId { get; set; }
        public string? ApproveRejectReasonOth { get; set; }
        public bool? IsWithDraw { get; set; }
        public string WithdrawReason { get; set; }
        public string WithdrawComment { get; set; }
        public bool? IsLAR { get; set; }
    }
}
