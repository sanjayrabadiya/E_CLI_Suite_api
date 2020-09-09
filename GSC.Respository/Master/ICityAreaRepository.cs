using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;

namespace GSC.Respository.Master
{
    public interface ICityAreaRepository : IGenericRepository<CityArea>
    {
        List<DropDownDto> GetCityAreaDropDown(int cityId);
        IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false);
        List<CityAreaGridDto> GetCityAreaList(bool isDeleted);
    }
}