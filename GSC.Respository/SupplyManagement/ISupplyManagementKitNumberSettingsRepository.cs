using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementKitNumberSettingsRepository : IGenericRepository<SupplyManagementKitNumberSettings>
    {
        List<SupplyManagementKitNumberSettingsGridDto> GetKITNumberList(bool isDeleted, int ProjectId);

        string CheckKitCreateion(SupplyManagementKitNumberSettings obj);

       void SaveRoleNumberSetting(SupplyManagementKitNumberSettingsDto supplyManagementKitNumberSettingsDto);

        void DeleteRoleNumberSetting(int id);
    }
}