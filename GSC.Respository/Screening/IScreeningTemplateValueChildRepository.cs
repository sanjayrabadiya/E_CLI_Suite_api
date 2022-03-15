using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;


namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueChildRepository : IGenericRepository<ScreeningTemplateValueChild>
    {
        void Save(ScreeningTemplateValue screeningTemplateValue);
        bool IsSameValue(ScreeningTemplateValue screeningTemplateValue);
    }
}
