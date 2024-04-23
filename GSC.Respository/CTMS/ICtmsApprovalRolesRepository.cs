using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsApprovalRolesRepository : IGenericRepository<CtmsApprovalRoles>
    {
        List<CtmsApprovalRolesGridDto> GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted);
        List<CtmsApprovalUsersGridDto> GetCtmsApprovalWorkFlowDetailsList(int projectId, bool isDeleted);
        void ChildUserApprovalAdd(CtmsApprovalRolesDto obj, int id);
        string Duplicate(CtmsApprovalRolesDto obj);
        List<DropDownDto> GetRoleCtmsRights(int projectId);
        List<DropDownDto> GetUserCtmsRights(int roleId, int projectId);
        void DeleteChildWorkflowEmailUser(CtmsApprovalRolesDto obj, int id);
    }
}