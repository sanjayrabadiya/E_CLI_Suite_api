using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface IRandomizationSetupRepository : IGenericRepository<RandomizationSetup>
    {
        List<RandomizationSetupGridDto> GetRandomizationSetupList(bool isDeleted);
    }
}