using System.Collections.Generic;
using GSC.Data.Dto.Master;

namespace GSC.Respository.Medra
{
    public interface IDictionaryRepository
    {
        List<DropDownDto> GetDictionaryDropDown();
    }
}