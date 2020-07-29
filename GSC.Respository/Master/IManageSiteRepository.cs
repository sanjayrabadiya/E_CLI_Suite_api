using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface IManageSiteRepository : IGenericRepository<ManageSite>
    {
        string Duplicate(ManageSite objSave);
        List<DropDownDto> GetManageSiteDropDown();
        IList<ManageSiteDto> GetManageSiteList(int Id);
    }
}