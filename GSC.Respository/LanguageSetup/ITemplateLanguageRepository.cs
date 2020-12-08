using GSC.Common.GenericRespository;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using System.Collections.Generic;

namespace GSC.Respository.LanguageSetup
{
    public interface ITemplateLanguageRepository : IGenericRepository<TemplateLanguage>
    {
        List<TemplateLanguageGridDto> GetTemplateLanguageList(int TemplateId);
    }
}