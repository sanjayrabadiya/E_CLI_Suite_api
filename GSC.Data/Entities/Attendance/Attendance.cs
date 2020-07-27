using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;

namespace GSC.Data.Entities.Attendance
{
    public class Attendance : BaseEntity
    {
        private DateTime _AttendanceDate;
        public int ProjectId { get; set; }
        public int? VolunteerId { get; set; }

        public DateTime AttendanceDate
        {
            get => _AttendanceDate.UtcDate();
            set => _AttendanceDate = value.UtcDate();
        }

        public bool IsFingerPrint { get; set; }
        public int RoleId { get; set; }
        public int? AuditReasonId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int? CompanyId { get; set; }
        public string Note { get; set; }
        public int UserId { get; set; }
        public Volunteer.Volunteer Volunteer { get; set; }
        public Master.Project Project { get; set; }
        public User User { get; set; }
        public ProjectDesignPeriod ProjectDesignPeriod { get; set; }
        public AuditReason AuditReason { get; set; }
        public bool IsTesting { get; set; }
        public int? ScreeningEntryId { get; set; }
        public int? PeriodNo { get; set; }
        public int? ProjectSubjectId { get; set; }
        public ProjectSubject ProjectSubject { get; set; }
        public AttendanceType AttendanceType { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsStandby { get; set; }
        public AttendaceStatus? Status { get; set; }
        public AttendanceHistory AttendanceHistory { get; set; }
        public virtual ScreeningEntry ScreeningEntry { get; set; }
        public NoneRegister NoneRegister { get; set; }
    }
}