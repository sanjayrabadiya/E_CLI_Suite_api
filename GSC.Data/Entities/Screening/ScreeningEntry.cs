using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningEntry : BaseEntity
    {
        public int AttendanceId { get; set; }
        public AttendanceType EntryType { get; set; }
        public int ProjectDesignPeriodId { get; set; }

        public string ScreeningNo { get; set; }
        private DateTime _screeningDate { get; set; }

        public DateTime ScreeningDate
        {
            get => _screeningDate.UtcDate();
            set => _screeningDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public ScreeningStatus Status { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsFitnessFit { get; set; }
        public bool? IsEnrolled { get; set; }
        public string ProjectNo { get; set; }
        public int ProjectDesignId { get; set; }
        public int ProjectId { get; set; }
        public bool IsTesting { get; set; }
        public string FitnessReason { get; set; }
        public string FitnessNotes { get; set; }
        public int Progress { get; set; }
        public ICollection<ScreeningTemplate> ScreeningTemplates { get; set; }
        public Master.Project Project { get; set; }
        public ScreeningHistory ScreeningHistory { get; set; }

        [ForeignKey("AttendanceId")] public Attendance.Attendance Attendance { get; set; }

        public ProjectDesignPeriod ProjectDesignPeriod { get; set; }
    }
}