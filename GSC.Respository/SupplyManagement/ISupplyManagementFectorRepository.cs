using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementFectorRepository : IGenericRepository<SupplyManagementFector>
    {
        List<SupplyManagementFectorGridDto> GetListByProjectId(int projectId, bool isDeleted);
        SupplyManagementFectorDto GetById(int id);
        void DeleteChild(int Id);
    }
}