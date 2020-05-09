﻿using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Attendance
{
    public class AttendanceHistoryRepository : GenericRespository<AttendanceHistory, GscContext>,
        IAttendanceHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public AttendanceHistoryRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<AttendanceHistoryDto> GetAttendanceHistory(int projectId)
        {
            return All.Where(x => x.DeletedDate == null && x.Attendance.ProjectId == projectId).Select(r =>
                new AttendanceHistoryDto
                {
                    Date = r.CreatedDate,
                    VolunteerName = r.Attendance.Volunteer.FullName,
                    OfficerName = r.CreatedByUser.UserName,
                    RoleName = r.Role.RoleName,
                    ReasonName = r.AuditReason.ReasonName,
                    Note = r.Note,
                    TimeZone = r.TimeZone,
                    IpAddress = r.IpAddress
                }).ToList();
        }

        public AttendanceHistory SaveHistory(string note, int attendanceId, int? auditReasonId)
        {
            var attendanceHistory = new AttendanceHistory();
            attendanceHistory.RoleId = _jwtTokenAccesser.RoleId;
            attendanceHistory.AuditReasonId = auditReasonId;
            attendanceHistory.AttendanceId = attendanceId;
            attendanceHistory.Note = note;
            attendanceHistory.TimeZone = _jwtTokenAccesser.GetHeader("timeZone");
            Add(attendanceHistory);
            return attendanceHistory;
        }
    }
}