using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Attendance
{
    public class AttendanceHistory : BaseEntity
    {
        public int AttendanceId { get; set; }
        public string Note { get; set; }
        public int? AuditReasonId { get; set; }
        public int RoleId { get; set; }
        public AuditReason AuditReason { get; set; }

        [ForeignKey("CreatedBy")] public User CreatedByUser { get; set; }

        [ForeignKey("RoleId")] public SecurityRole Role { get; set; }

        public Attendance Attendance { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}