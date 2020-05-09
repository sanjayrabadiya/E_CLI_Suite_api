using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using System.Collections.Generic;

namespace GSC.Respository.Medra
{
    public interface IMedraConfigRepository : IGenericRepository<MedraConfig>
    {
        string Duplicate(MedraConfigDto objSave);
        List<DropDownDto> GetMedraVesrionByDictionaryDropDown(int DictionaryId);
        List<DropDownDto> GetMedraLanguageVersionDropDown();
        DropDownDto GetDetailByMeddraConfigId(int MeddraConfigId);
    }
}