using System;
using GSC.Helper;
using GSC.Shared;

namespace GSC.Data.Dto.Screening
{
    public class MyReviewDto
    {
        public int ScreeningEntryId { get; set; }
        public int RoleId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public string ScreeningNo { get; set; }
        private DateTime _screeningDate { get; set; }

        public DateTime ScreeningDate
        {
            get => _screeningDate.UtcDate();
            set => _screeningDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        private DateTime? _submittedDate { get; set; }

        public DateTime? SubmittedDate
        {
            get => _submittedDate.UtcDate();
            set => _submittedDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public string TemplateName { get; set; }
        public string SubmittedBy { get; set; }
        public string ReviewedLevel { get; set; }

        public string LastReviewedBy { get; set; }
        public string VistName { get; set; }
        public string VolunteerName { get; set; }
        public string ProjectName { get; set; }

        private DateTime? _lastReviewedDate { get; set; }

        public DateTime? LastReviewedDate
        {
            get => _lastReviewedDate.UtcDate();
            set => _lastReviewedDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public int ProjectDesignTemplateId { get; set; }
    }
}