using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningEntryDto : BaseDto
    {
       
        public int? AttendanceId { get; set; }
        public int? RandomizationId { get; set; }

        public DataEntryType EntryType { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int ProjectDesignId { get; set; }
        public string ScreeningNo { get; set; }
        private DateTime? _screeningDate { get; set; }

        public DateTime? ScreeningDate
        {
            get => _screeningDate.UtcDate();
            set => _screeningDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public ScreeningTemplateStatus Status { get; set; }
        public int ProjectId { get; set; }
        public bool IsTesting { get; set; }
        public bool? IsFitnessFit { get; set; }
        public bool? IsEnrolled { get; set; }
        public string ProjectNo { get; set; }
        public string FitnessReason { get; set; }
        public string FitnessNotes { get; set; }
        public int Progress { get; set; }
        public List<ScreeningVisitTree> ScreeningVisits { get; set; }
        public string VolunteerName { get; set; }
        public string PatientStatusName { get; set; }
        public ScreeningPatientStatus? PatientStatusId { get; set; }
        public bool IsMultipleVisits { get; set; }
        public bool MyReview { get; set; }
        public string LevelName1 { get; set; }
        public bool IsSystemQueryUpdate { get; set; }
        public List<int> ProjectAttendanceTemplateIds { get; set; }
        public bool IsElectronicSignature { get; set; }
        public double? StudyVersion { get; set; }
    }

    public class ScreeningAuditDto
    {
        public int Id { get; set; }
        private DateTime? _createdDate;
        public string Visit { get; set; }
        public string Template { get; set; }
        public string Variable { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string Note { get; set; }
        public string User { get; set; }
        public string Role { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public CollectionSources? CollectionSource { get; set; }
        public TableCollectionSource? TableCollectionSource { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }

    public class SaveRandomizationDto
    {
        public int RandomizationId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        private DateTime _visitDate;
        public DateTime VisitDate
        {
            get => _visitDate.UtcDateTime();
            set => _visitDate = value.UtcDateTime();
        }
    }

    public class DataEntryAuditReportDto
    {
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string PatientInitial { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }
        public int Id { get; set; }
        private DateTime? _createdDate;
        public string Visit { get; set; }
        public string Template { get; set; }
        public string Variable { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string Note { get; set; }
        public string User { get; set; }
        public string Role { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public CollectionSources? CollectionSource { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}