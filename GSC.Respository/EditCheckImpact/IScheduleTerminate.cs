using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.EditCheckImpact
{
    public interface IScheduleTerminate : IGenericRepository<ScreeningTemplate>
    {
        void TerminateScheduleTemplateVisit(int projectDesignTemplateId, int screeningEntryId, bool isSelfCorrection);
    }
}
