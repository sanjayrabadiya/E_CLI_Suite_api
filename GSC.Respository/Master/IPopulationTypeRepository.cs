using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IPopulationTypeRepository : IGenericRepository<PopulationType>
    {
        List<DropDownDto> GetPopulationTypeDropDown();
        string Duplicate(PopulationType objSave);
    }
}