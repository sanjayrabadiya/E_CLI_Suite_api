using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using System;

namespace GSC.Respository.Screening
{
    public interface IScreeningVisitHistoryRepository : IGenericRepository<ScreeningVisitHistory>
    {
        void SaveByScreeningVisit(ScreeningVisit screeningVisit, ScreeningVisitStatus screeningVisitStatus, DateTime? statusDate);
        void Save(ScreeningVisitHistoryDto screeningVisitHistoryDto);
    }
}
