using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using System.Collections.Generic;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateLockUnlockRepository : IGenericRepository<ScreeningTemplateLockUnlockAudit>
    {
        List<LockUnlockHistoryListDto> ProjectLockUnLockHistory(int projectId, int parentProjectId);
    }
}
