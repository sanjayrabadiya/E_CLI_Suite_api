﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Attendance;
using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningEntry : BaseEntity
    {
        public int? AttendanceId { get; set; }
        public int? RandomizationId { get; set; }
        public DataEntryType EntryType { get; set; }
        public int ProjectDesignPeriodId { get; set; }

        public string ScreeningNo { get; set; }
        private DateTime _screeningDate { get; set; }

        public DateTime ScreeningDate
        {
            get => _screeningDate.UtcDate();
            set => _screeningDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

  
        public int? CompanyId { get; set; }
        public bool? IsFitnessFit { get; set; }
        public bool? IsEnrolled { get; set; }
        public string ProjectNo { get; set; }
        public int ProjectDesignId { get; set; }
        public double? StudyVersion { get; set; }
        public int ProjectId { get; set; }
        public bool IsTesting { get; set; }
        public string FitnessReason { get; set; }
        public string FitnessNotes { get; set; }
        public int Progress { get; set; }
        public int? StudyId { get; set; }
        public string Notes { get; set; }
        public ICollection<ScreeningVisit> ScreeningVisit { get; set; }
        public Master.Project Project { get; set; }

        [ForeignKey("StudyId")] public Master.Project Study { get; set; }
        public ScreeningHistory ScreeningHistory { get; set; }

        [ForeignKey("AttendanceId")] public Attendance.Attendance Attendance { get; set; }

        public ProjectDesignPeriod ProjectDesignPeriod { get; set; }
        public Randomization Randomization { get; set; }
    }
}