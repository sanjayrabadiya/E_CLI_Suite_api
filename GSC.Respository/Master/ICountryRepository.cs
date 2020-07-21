using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;

namespace GSC.Respository.Master
{
    public interface ICountryRepository : IGenericRepository<Country>
    {
        List<DropDownDto> GetCountryDropDown();
        List<DropDownDto> GetProjectCountryDropDown();
        string DuplicateCountry(Country objSave);
        List<DropDownDto> GetCountryByParentProjectIdDropDown(int ParentProjectId);
        List<DropDownDto> GetCountryByProjectIdDropDown(int ParentProjectId);
    }
}