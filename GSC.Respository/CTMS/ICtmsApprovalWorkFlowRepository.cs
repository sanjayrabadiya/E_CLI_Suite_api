using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsApprovalWorkFlowRepository : IGenericRepository<CtmsApprovalWorkFlow>
    {
        List<CtmsApprovalWorkFlowGridDto> GetCtmsApprovalWorkFlowList(int projectId, bool isDeleted);
        void ChildUserApprovalAdd(CtmsApprovalWorkFlowDto obj, int id);
        string Duplicate(CtmsApprovalWorkFlowDto obj);
        List<DropDownDto> GetRoleCtmsRights(int projectId);
        List<DropDownDto> GetUserCtmsRights(int roleId, int projectId);
        void DelectChildWorkflowEmailUser(CtmsApprovalWorkFlowDto obj, int id);
    }
}