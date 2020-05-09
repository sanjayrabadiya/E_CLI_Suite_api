using System;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Attendance
{
    public class AttendanceHistoryDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string VolunteerName { get; set; }
        public string Note { get; set; }
        public int? AuditReasonId { get; set; }
        public DateTime? Date { get; set; }
        public int RoleId { get; set; }
        public string OfficerName { get; set; }
        public string RoleName { get; set; }
        public string ReasonName { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}