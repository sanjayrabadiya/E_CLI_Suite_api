using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsWorkflowApprovalRepository : IGenericRepository<CtmsWorkflowApproval>
    {
        List<CtmsWorkflowApprovalGridDto> GetApprovalBySender(int studyPlanId, int projectId, TriggerType triggerType);
        List<CtmsWorkflowApprovalGridDto> GetApprovalByApprover(int studyPlanId, int projectId, TriggerType triggerType);
        bool GetApprovalStatus(int studyPlanId, int projectId, TriggerType triggerType);
        List<ProjectRightDto> GetProjectRightByProjectId(int projectId, TriggerType triggerType);
        bool CheckSender(int studyPlanId, int projectId, TriggerType triggerType);
        List<CtmsWorkflowApprovalGridDto> GetApproverNewComment(TriggerType triggerType);
        List<CtmsWorkflowApprovalGridDto> GetSenderNewComment(TriggerType triggerType);
        List<DashboardDto> GetCtmsApprovalMyTask(int projectId);
        List<ApprovalUser> GetApprovalUsers(int studyPlanId);
        bool IsNewComment(int studyPlanId, int projectId, TriggerType triggerType);
        bool IsCommentReply(int studyPlanId, int projectId, int userId, int roleId, TriggerType triggerType);
    }
}
