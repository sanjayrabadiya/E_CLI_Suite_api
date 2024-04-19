using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsWorkflowApprovalRepository : IGenericRepository<CtmsWorkflowApproval>
    {
        List<CtmsWorkflowApprovalDto> GetApprovalBySender(int studyPlanId, int projectId);
        List<CtmsWorkflowApprovalDto> GetApprovalByApprover(int studyPlanId, int projectId);
        bool GetApprovalStatus(int studyPlanId, int projectId);
        List<ProjectRightDto> GetProjectRightByProjectId(int projectId, TriggerType triggerType);
    }
}
