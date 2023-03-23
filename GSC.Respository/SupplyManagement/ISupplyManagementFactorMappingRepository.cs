using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementFactorMappingRepository : IGenericRepository<SupplyManagementFactorMapping>
    {
        List<SupplyManagementFactorMappingGridDto> GetSupplyFactorMappingList(bool isDeleted, int ProjectId);


        string Validation(SupplyManagementFactorMapping obj);


    }
}