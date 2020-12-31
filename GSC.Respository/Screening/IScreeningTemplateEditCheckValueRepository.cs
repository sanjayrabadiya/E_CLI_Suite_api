using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateEditCheckValueRepository : IGenericRepository<ScreeningTemplateEditCheckValue>
    {
        bool CheckUpdateEditCheckRefValue(int screeningTemplateId, int projectDesignVariableId, int editCheckDetailId, EditCheckValidateType validateType, string sampleResult);
    }

}
