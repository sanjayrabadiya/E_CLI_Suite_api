using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ICurrencyRepository : IGenericRepository<Currency>
    {
        List<CurrencyGridDto> GetCurrencyList(bool isDeleted);
        string Duplicate(Currency objSave);
        List<DropDownDto> GetCountryDropDown();
    }
}