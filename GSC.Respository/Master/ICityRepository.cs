using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;

namespace GSC.Respository.Master
{
    public interface ICityRepository : IGenericRepository<City>
    {
        string DuplicateCity(City objSave);
        List<DropDownDto> GetCityDropDown();
        List<CityDto> GetCities(bool isDeleted);
        IList<DropDownDto> AutoCompleteSearch(string searchText, bool isAutoSearch = false);
        List<DropDownDto> GetCityByStateDropDown(int StateId);
    }
}