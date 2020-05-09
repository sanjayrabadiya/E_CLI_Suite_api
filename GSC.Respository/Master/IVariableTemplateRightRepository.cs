using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IVariableTemplateRightRepository : IGenericRepository<VariableTemplateRight>
    {
        void SaveTemplateRights(VariableTemplateRightDto templateRightDto);
    }
}