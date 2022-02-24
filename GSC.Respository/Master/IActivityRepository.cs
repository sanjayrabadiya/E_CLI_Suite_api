using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IActivityRepository : IGenericRepository<Activity>
    {
        List<DropDownDto> GetActivityDropDown();
        List<ActivityGridDto> GetActivityList(bool isDeleted);
        List<DropDownDto> GetActivityDropDownByModuleId(int moduleId);
        string Duplicate(Activity objSave);
        DropDownDto GetActivityForFormList(int tabNumber);
    }
}