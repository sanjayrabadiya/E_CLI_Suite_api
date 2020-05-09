using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningEntryRepository : IGenericRepository<ScreeningEntry>
    {
        ScreeningEntryDto GetDetails(int id);
        void SaveScreening(ScreeningEntry screeningEntry, List<int> projectAttendanceTemplateIds);
        IList<DropDownDto> AutoCompleteSearch(string searchText);
        List<AttendanceScreeningGridDto> GetScreeningList(ScreeningSearhParamDto searchParam);
        IList<ScreeningAuditDto> GetAuditHistory(int id);
        ScreeningSummaryDto GetSummary(int id);
        List<DropDownDto> GetProjectStatusAndLevelDropDown(int parentProjectId);
    }
}