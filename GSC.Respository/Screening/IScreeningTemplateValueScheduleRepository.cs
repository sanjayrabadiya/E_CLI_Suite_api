using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueScheduleRepository : IGenericRepository<ScreeningTemplateValueSchedule>
    {
        void InsertUpdate(ScreeningTemplateValueScheduleDto objSave);
        void CloseSystemQuery(int screeningTemplateId, int projectDesignVariableId);
    }
}