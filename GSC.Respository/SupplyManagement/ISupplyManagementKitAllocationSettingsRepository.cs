using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementKitAllocationSettingsRepository : IGenericRepository<SupplyManagementKitAllocationSettings>
    {
        List<SupplyManagementKitAllocationSettingsDto> GetKITAllocationList(bool isDeleted, int ProjectId);

        IList<DropDownDto> GetVisitDropDownByProjectId(int projectId);
    }
}