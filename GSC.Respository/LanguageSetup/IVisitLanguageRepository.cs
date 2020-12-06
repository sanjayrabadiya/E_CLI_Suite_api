using GSC.Common.GenericRespository;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using System.Collections.Generic;

namespace GSC.Respository.LanguageSetup
{
    public interface IVisitLanguageRepository : IGenericRepository<VisitLanguage>
    {
        List<VisitLanguageGridDto> GetVisitLanguageList(int VisitId);
    }
}