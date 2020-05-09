using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;

namespace GSC.Respository.Attendance
{
    public interface IAttendanceHistoryRepository : IGenericRepository<AttendanceHistory>
    {
        List<AttendanceHistoryDto> GetAttendanceHistory(int projectId);
        AttendanceHistory SaveHistory(string note, int attendanceId, int? auditReasonId);
    }
}