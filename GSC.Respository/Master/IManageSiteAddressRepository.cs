using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface IManageSiteAddressRepository : IGenericRepository<ManageSiteAddress>
    {
        List<ManageSiteAddressGridDto> GetManageSiteAddress(int id, bool isDeleted);
        string Duplicate(ManageSiteAddress objSave);
        List<DropDownDto> GetSiteAddressDropdown(int id);
    }
}
