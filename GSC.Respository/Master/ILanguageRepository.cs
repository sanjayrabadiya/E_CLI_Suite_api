using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ILanguageRepository : IGenericRepository<Language>
    {
        List<DropDownDto> GetLanguageDropDown();
        string Duplicate(Language objSave);
        List<LanguageGridDto> GetLanguageList(bool isDeleted);
    }
}