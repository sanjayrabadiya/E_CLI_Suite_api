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
    public interface ICtmsSiteContractWorkflowApprovalRepository : IGenericRepository<CtmsSiteContractWorkflowApproval>
    {
        List<CtmsSiteContractWorkflowApprovalGridDto> GetApprovalBySender(int siteContractId, int projectId, TriggerType triggerType);
        List<CtmsSiteContractWorkflowApprovalGridDto> GetApprovalByApprover(int siteContractId, int projectId, TriggerType triggerType);
        bool GetApprovalStatus(int siteContractId, int projectId, TriggerType triggerType);
        List<ProjectRightDto> GetProjectRightByProjectId(int siteContractId, int projectId, int siteId, TriggerType triggerType);
        bool CheckSender(int siteContractId, int projectId, int siteId, TriggerType triggerType);
        List<CtmsSiteContractWorkflowApprovalGridDto> GetApproverNewComment(TriggerType triggerType);
        List<CtmsSiteContractWorkflowApprovalGridDto> GetSenderNewComment(TriggerType triggerType);
        List<DashboardDto> GetCtmsApprovalMyTask(int projectId);
        List<ApprovalUser> GetApprovalUsers(int siteContractId);
        bool IsNewComment(int siteContractId, int projectId, TriggerType triggerType);
        bool IsCommentReply(int siteContractId, int projectId, int userId, int roleId, TriggerType triggerType);
    }
}
