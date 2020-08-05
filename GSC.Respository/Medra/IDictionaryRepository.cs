using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Medra;

namespace GSC.Respository.Medra
{
    public interface IDictionaryRepository : GSC.Common.GenericRespository.IGenericRepository<Dictionary>
    {
        List<DropDownDto> GetDictionaryDropDown();
    }
}