using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementApprovalRepository : IGenericRepository<SupplyManagementApproval>
    {
        List<SupplyManagementApprovalGridDto> GetSupplyManagementApprovalList(int projectId, bool isDeleted);
        void ChildUserApprovalAdd(SupplyManagementApprovalDto obj, int id);
        string Duplicate(SupplyManagementApprovalDto obj);

        List<DropDownDto> GetProjectRightsRoleShipmentApproval(int projectId);

        List<DropDownDto> GetRoleUserShipmentApproval(int roleId,int projectId);

        void SendShipmentWorkflowApprovalEmail(SupplyManagementShipmentApproval supplyManagementShipmentApproval);

        void DelectChildWorkflowEmailUser(SupplyManagementApprovalDto obj, int id);
    }
}