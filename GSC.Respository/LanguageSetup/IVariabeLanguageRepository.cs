using GSC.Common.GenericRespository;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using System.Collections.Generic;

namespace GSC.Respository.LanguageSetup
{
    public interface IVariabeLanguageRepository : IGenericRepository<VariableLanguage>
    {
        List<VariableLanguageGridDto> GetVariableLanguageList(int VariableId);
        bool IsLanguageExist(int LanguageId);
    }
}