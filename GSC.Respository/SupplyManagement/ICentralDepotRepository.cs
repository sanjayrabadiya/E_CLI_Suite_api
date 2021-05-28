using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ICentralDepotRepository : IGenericRepository<CentralDepot>
    {
        List<DropDownDto> GetCentralDepotDropDown();
        List<CentralDepotGridDto> GetCentralDepotList(bool isDeleted);
    }
}