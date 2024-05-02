using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public interface ICtmsStudyPlanTaskCommentRepository:IGenericRepository<CtmsStudyPlanTaskComment>
    {
        List<CtmsStudyPlanTaskCommentGridDto> GetCommentHistory(int id, int studyPlanId, TriggerType triggerType);
        List<CtmsStudyPlanTaskCommentGridDto> GetSenderCommentHistory(int id, int userId, int roleId, int studyPlanId, TriggerType triggerType);
        bool CheckAllTaskComment(int ctmsApprovalId);
    }
}
