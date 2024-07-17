using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
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
        List<DropDownDto> GetUserCtmsRights(int roleId, int projectId, int siteId);
        void DeleteChildWorkflowEmailUser(int id);
        bool CheckIsApprover(int projectId, TriggerType triggerType);
        List<DropDownDto> GetSiteList(int projectId);
        bool CheckIsApproverForSiteContract(int projectId, int siteId, TriggerType triggerType);
    }
}