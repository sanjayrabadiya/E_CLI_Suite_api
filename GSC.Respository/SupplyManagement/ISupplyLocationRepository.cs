using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyLocationRepository : IGenericRepository<SupplyLocation>
    {
        string Duplicate(SupplyLocation objSave);
        List<DropDownDto> GetSupplyLocationDropDown();
        List<SupplyLocationGridDto> GetSupplyLocationList(bool isDeleted);
    }
}