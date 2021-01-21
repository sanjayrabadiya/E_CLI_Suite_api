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
        public string EventDescription { get; set; }
        private DateTime _StartDate { get; set; }
        public DateTime StartDate
        {
            get => _StartDate.UtcDate();
            set => _StartDate = value.UtcDate();
        }
        public AdverseEventEffect EventEffect { get; set; }
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
    }

    public class AEReportingGridDto : BaseAuditDto
    {
        public string SubjectName { get; set; }
        public string EventDescription { get; set; }
        private DateTime _StartDate { get; set; }
        public DateTime StartDate
        {
            get => _StartDate.UtcDate();
            set => _StartDate = value.UtcDate();
        }
        public string EventEffectName { get; set; }
        public bool IsReviewedDone { get; set; }
    }
}
