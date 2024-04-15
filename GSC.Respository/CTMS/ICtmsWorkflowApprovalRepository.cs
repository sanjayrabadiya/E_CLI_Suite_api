using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsWorkflowApprovalRepository : IGenericRepository<CtmsWorkflowApproval>
    {
        List<CtmsWorkflowApprovalDto> GetApprovalBySender(int studyPlanId, int projectId);
        List<CtmsWorkflowApprovalDto> GetApprovalByApprover(int studyPlanId, int projectId);
        bool GetApprovalStatus(int studyPlanId, int projectId);
    }
}
