using System;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Attendance
{
    public class AttendanceScreeningGridDto : BaseAuditDto
    {
        private DateTime _AttendanceDate;
        private DateTime? _ScreeningDate;
        public int Id { get; set; }
        public int ScreeningEntryId { get; set; }
        public int AttendanceId { get; set; }
        public int ProjectId { get; set; }

        public int? VolunteerId { get; set; }

        public DateTime AttendanceDate
        {
            get => _AttendanceDate.UtcDate();
            set => _AttendanceDate = value.UtcDate();
        }

        public DateTime? ScreeningDate
        {
            get => _ScreeningDate.UtcDate();
            set => _ScreeningDate = value.UtcDate();
        }

        public bool IsFingerPrint { get; set; }
        public string ScreeningNo { get; set; }
        public string Note { get; set; }
        public string VolunteerNumber { get; set; }
        public string AliasName { get; set; }
        public string VolunteerName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string AttendedBy { get; set; }
        public string Gender { get; set; }
        public string IsFitnessFit { get; set; }
        public int ProjectDesignId { get; set; }
        public string ScreeningStatusName { get; set; }
        public string AuditReasonName { get; set; }
        public int? AttendanceScreeningEntryId { get; set; }
        public int? AttendanceTemplateId { get; set; }
        public int? DiscontinuedTemplateId { get; set; }
        public bool IsScreeningStarted { get; set; }
        public int? ProjectSubjectId { get; set; }
        public int? PeriodNo { get; set; }
        public string SubjectNumber { get; set; }
        public DataEntryType AttendanceType { get; set; }
        public AttendaceStatus? Status { get; set; }
        public string AttendaceStatusName { get; set; }
        public bool IsReplaced { get; set; }
        public string IsStandby { get; set; }
        public bool IsLocked { get; set; }
        public bool IsBarcodeGenerated { get; set; }
        public int? AttendanceBarcodeGenerateId { get; set; }
    }
}