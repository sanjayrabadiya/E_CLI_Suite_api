using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningVisitRepository : IGenericRepository<ScreeningVisit>
    {
        void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId,int projectDesignVisitId);
    }
}
