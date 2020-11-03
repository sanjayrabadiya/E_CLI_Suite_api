using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface IManageSiteRepository : IGenericRepository<ManageSite>
    {
        List<ManageSiteGridDto> GetManageSites(bool isDeleted);
        string Duplicate(ManageSite objSave);
        List<DropDownDto> GetManageSiteDropDown();
        IList<ManageSiteDto> GetManageSiteList(int Id);
        void UpdateRole(ManageSite ManageSite);
    }
}