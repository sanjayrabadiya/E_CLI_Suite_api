using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IScreeningVisitHistoryRepository : IGenericRepository<ScreeningVisitHistory>
    {
        void SaveByScreeningVisit(ScreeningVisit screeningVisit, ScreeningVisitStatus screeningVisitStatus);
        void Save(int screeningVisitId, ScreeningVisitStatus screeningVisitStatus, string note);
    }
}
