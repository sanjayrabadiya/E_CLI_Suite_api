using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.AdverseEvent
{
    public class AEReportingDto : BaseDto
    {
        public int RandomizationId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsReviewedDone { get; set; }
        public int? ReviewedByUser { get; set; }
        public int? ReviewedByRole { get; set; }
        private DateTime? _ReviewedDateTime { get; set; }
        public DateTime? ReviewedDateTime
        {
            get => _ReviewedDateTime.UtcDate();
            set => _ReviewedDateTime = value.UtcDate();
        }
        public string EventEffectName { get; set; }
        public DesignScreeningTemplateDto template { get; set; }
        public bool? IsApproved { get; set; }
        public int? RejectReasonId { get; set; }
        public string? RejectReasonOth { get; set; }
        public DateTime? ApproveRejectDateTime { get; set; }
        public int ProjectDesignTemplateIdPatient { get; set; }
        public int ProjectDesignTemplateIdInvestigator { get; set; }
        public int SeveritySeqNo1 { get; set; }
        public int SeveritySeqNo2 { get; set; }
        public int SeveritySeqNo3 { get; set; }
        public int ProjectDesignVariableIdForEvent { get; set; }
        public int? ScreeningTemplateId { get; set; }

    }

    public class AEReportingGridDto : BaseAuditDto
    {
        public string SubjectName { get; set; }
        public string ScreeningNumber { get; set; }
        public string RandomizationNumber { get; set; }
        public string Initial { get; set; }
        public string EventDescription { get; set; }
        public string ReviewStatus { get; set; }
        public DateTime? ApproveRejectDateTime { get; set; }
        public string? RejectReasonOth { get; set; }
        public string? RejectReason { get; set; }
        private DateTime _StartDate { get; set; }
        public DateTime StartDate
        {
            get => _StartDate.UtcDate();
            set => _StartDate = value.UtcDate();
        }
        public string EventEffectName { get; set; }
        public bool IsReviewedDone { get; set; }
    }

    public class ScreeningDetailsforAE
    {
        public int AdverseEventId { get; set; }
        public int ProjectId { get; set; }
        public int ScreeningVisitId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningEntryId { get; set; }
        public int ParentProjectId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }

    }

}
