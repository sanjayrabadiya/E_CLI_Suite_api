using GSC.Common.GenericRespository;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using System.Collections.Generic;

namespace GSC.Respository.LanguageSetup
{
    public interface IVariableCategoryLanguageRepository : IGenericRepository<VariableCategoryLanguage>
    {
        List<VariableCategoryLanguageGridDto> GetVariableCategoryLanguageList(int VariableCategoryId);
        bool IsLanguageExist(int LanguageId);
    }
}