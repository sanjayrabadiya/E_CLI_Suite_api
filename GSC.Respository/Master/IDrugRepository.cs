using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDrugRepository : IGenericRepository<Drug>
    {
        List<DropDownDto> GetDrugDropDown();
        string Duplicate(Drug objSave);
    }
}