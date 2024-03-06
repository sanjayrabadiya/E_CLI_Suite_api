using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementEmailConfigurationRepository : IGenericRepository<SupplyManagementEmailConfiguration>
    {
        List<SupplyManagementEmailConfigurationGridDto> GetSupplyManagementEmailConfigurationList(int projectId, bool isDeleted);

        string Duplicate(SupplyManagementEmailConfiguration obj);

        List<ProjectRightDto> GetProjectRightsIWRS(int projectId);

        void ChildEmailUserAdd(SupplyManagementEmailConfigurationDto obj, int id);

        void DeleteChildEmailUser(int id);

        List<SupplyManagementEmailConfigurationDetailGridDto> GetSupplyManagementEmailConfigurationDetailList(int id);

        List<SupplyManagementEmailConfigurationDetailHistoryGridDto> GetEmailHistory(int id);
    }
}