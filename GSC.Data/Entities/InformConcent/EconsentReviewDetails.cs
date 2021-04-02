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
        public DateTime? investigatorRevieweddatetime { get; set; }
        public DateTime? patientapproveddatetime { get; set; }
        public string? pdfpath { get; set; }
        public bool IsReviewedByPatient { get; set; }
        public string? patientdigitalSignImagepath { get; set; }
        public Randomization Randomization { get; set; }
        public EconsentSetup EconsentSetup { get; set; }
        public List<EconsentReviewDetailsSections> EconsentReviewDetailsSections { get; set; }
        public int? ReviewDoneByUserId { get; set; }
        public bool? IsApproved { get; set; }
        public int? ApproveRejectReasonId { get; set; }
        public string? ApproveRejectReasonOth { get; set; }
    }
}
