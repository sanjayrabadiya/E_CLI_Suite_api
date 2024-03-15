using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using System.Collections.Generic;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateLockUnlockRepository : IGenericRepository<ScreeningTemplateLockUnlockAudit>
    {
        void Insert(ScreeningTemplateLockUnlockAudit screeningTemplateLockUnlock);
        List<LockUnlockHistoryListDto> ProjectLockUnLockHistory(int projectId, int parentProjectId);
    }
}
