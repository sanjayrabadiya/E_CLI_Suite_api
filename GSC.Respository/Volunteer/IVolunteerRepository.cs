using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerRepository : IGenericRepository<Data.Entities.Volunteer.Volunteer>
    {
        IList<VolunteerGridDto> GetVolunteerList();
        IList<VolunteerGridDto> Search(VolunteerSearchDto search);
        VolunteerStatusCheck CheckStatus(int id);
        IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false);
        IList<DropDownDto> GetVolunteerForAttendance(VolunteerSearchDto search);
        List<VolunteerGridDto> GetVolunteerDetail(bool isDeleted, int volunteerid);
        IList<DropDownDto> QueryAutoCompleteSearch(string searchText, bool isAutoSearch = false);
        string DuplicateOldReference(VolunteerDto objSave);

        //IList<DropDownDto> getVolunteersForDataEntryByPeriodIdLocked(int? projectDesignPeriodId, int projectId, bool isLock);

    }
}