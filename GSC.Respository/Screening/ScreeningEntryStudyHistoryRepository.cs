using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;

namespace GSC.Respository.Screening
{
    public class ScreeningEntryStudyHistoryRepository : GenericRespository<ScreeningEntryStudyHistory>, IScreeningEntryStudyHistoryRepository
    {
        public ScreeningEntryStudyHistoryRepository(IGSCContext context)
            : base(context)
        {
        }
    }
}
