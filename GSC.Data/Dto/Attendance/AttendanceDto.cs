using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared;

namespace GSC.Data.Dto.Attendance
{
    public class AttendanceDto : BaseDto
    {
        private DateTime _AttendanceDate;

        [Required(ErrorMessage = "Project Name is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Volunteer Name is required.")]
        public int? VolunteerId { get; set; }

        public DateTime AttendanceDate
        {
            get => _AttendanceDate.UtcDate();
            set => _AttendanceDate = value.UtcDate();
        }

        public bool IsFingerPrint { get; set; }
        public int RoleId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int? CompanyId { get; set; }
        public string Note { get; set; }
        public int UserId { get; set; }
        public bool IsTesting { get; set; }
        public int? AuditReasonId { get; set; }
        public int? ScreeningEntryId { get; set; }
        public int? ProjectSubjectId { get; set; }
        public DataEntryType AttendanceType { get; set; }
        public bool IsProcessed { get; set; }
        public AttendaceStatus? Status { get; set; }
        public string AttendaceTypeName { get; set; }
        public int? PeriodNo { get; set; }
        public bool IsStandby { get; set; }
    }
}