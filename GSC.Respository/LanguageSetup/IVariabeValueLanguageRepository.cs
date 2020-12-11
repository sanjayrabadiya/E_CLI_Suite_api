using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using System.Collections.Generic;

namespace GSC.Respository.LanguageSetup
{
    public interface IVariabeValueLanguageRepository : IGenericRepository<VariableValueLanguage>
    {
        List<VariableValueLanguageGridDto> GetVariableValueLanguageList(int VariableValueId);
        bool IsLanguageExist(int LanguageId);
    }
}