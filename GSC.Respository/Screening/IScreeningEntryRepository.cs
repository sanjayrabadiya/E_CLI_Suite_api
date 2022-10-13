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
        void SaveScreeningAttendance(ScreeningEntry screeningEntry, List<int> projectAttendanceTemplateIds);
        ScreeningEntry SaveScreeningRandomization(SaveRandomizationDto saveRandomizationDto);
        IList<DropDownDto> AutoCompleteSearch(string searchText);
        List<AttendanceScreeningGridDto> GetScreeningList(ScreeningSearhParamDto searchParam);
        ScreeningGridDto GetScreeningDataList(ScreeningSearhParamDto searchParam);
        List<DropDownDto> GetProjectStatusAndLevelDropDown(int parentProjectId);
        IList<DropDownDto> GetSubjectByProjecId(int projectId);
        IList<DropDownDto> GetSubjectByProjecIdLocked(int projectId, bool isLock, bool isHardLock, bool isParent); // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
        IList<DropDownDto> GetPeriodByProjectIdIsLockedDropDown(LockUnlockDDDto lockUnlockDDDto);
        IList<DropDownDto> BarcodeSearch(string searchText);
        IList<DropDownDto> VolunteerSearch(string searchText);
        List<ProjectDropDown> GetSiteByLockUnlock(int parentProjectId, bool isLock, bool isHardLock); // Add by Tinku for add separate dropdown for parent project (24/06/2022) 
        void SetFitnessValue(ScreeningTemplateValue screeningTemplateValueDto);

        List<ScreeningEntryStudyHistoryDto> GetVolunteerProjectHistory(int ScreeningEntryId);
        List<VolunteerAttendanceDto> GetVolunteerByProjectId(int projectId);
        List<VolunteerAttendanceDto> GetVolunteerScreeningList(int projectId, int volunteerId);
        List<ProjectDropDown> GetParentProjectDropdown(bool isLock, bool isHardLock);
    }
}