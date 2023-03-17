using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementFectorDetailRepository : IGenericRepository<SupplyManagementFectorDetail>
    {
        SupplyManagementFectorDto GetDetailList(int id);
        SupplyManagementFectorDetailDto GetDetail(int id);
        bool CheckType(SupplyManagementFectorDetailDto supplyManagementFectorDetailDto);

        bool CheckrandomizationStarted(int id);

        bool CheckUploadRandomizationsheet(SupplyManagementFectorDetailDto supplyManagementFectorDetailDto);
    }
}