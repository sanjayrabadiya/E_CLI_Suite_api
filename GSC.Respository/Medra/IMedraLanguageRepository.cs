using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IMedraLanguageRepository : IGenericRepository<MedraLanguage>
    {
        List<DropDownDto> GetLanguageDropDown();
        string Duplicate(MedraLanguage objSave);
    }
}