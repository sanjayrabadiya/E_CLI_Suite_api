using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningHistoryRepository : IGenericRepository<ScreeningHistory>
    {
        List<ScreeningHistoryDto> GetScreeningHistoryByVolunteerId(int volunteerId, int lastDay);
        string CheckVolunteerEligibaleDate(AttendanceDto attendanceDto);

    }
}