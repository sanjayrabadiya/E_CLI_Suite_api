using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using System;

namespace GSC.Respository.Screening
{
    public interface IScreeningVisitRepository : IGenericRepository<ScreeningVisit>
    {
        void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId,int projectDesignVisitId, DateTime visitDate);
        void StatusUpdate(ScreeningVisitHistoryDto screeningVisitHistoryDto);
    }
}
