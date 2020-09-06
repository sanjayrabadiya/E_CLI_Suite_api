using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.Volunteer;

namespace GSC.Respository.Attendance
{
    public interface IAttendanceRepository : IGenericRepository<Data.Entities.Attendance.Attendance>
    {
        List<AttendanceScreeningGridDto> GetAttendaceList(ScreeningSearhParamDto attendanceSearch);
        string CheckVolunteer(AttendanceDto attendanceDto);
        IList<DropDownDto> GetVolunteersByProjectId(int projectId);
        IList<DropDownDto> GetVolunteersForReplacement(int projectId);
        string ProjectSuspended(int projectId);
        IList<VolunteerAttendaceDto> GetAttendanceAnotherPeriod(VolunteerSearchDto search);
        void SaveAttendance(Data.Entities.Attendance.Attendance attendance);

        List<AttendanceScreeningGridDto> GetAttendaceForProjectRightList(ScreeningSearhParamDto attendanceSearch);
        List<DropDownDto> GetAttendanceForMeddraCodingDropDown(MeddraCodingSearchDto filters);
    }
}