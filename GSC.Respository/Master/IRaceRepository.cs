using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IRaceRepository : IGenericRepository<Race>
    {
        List<DropDownDto> GetRaceDropDown();
        string Duplicate(Race objSave);
        List<RaceGridDto> GetRaceList(bool isDeleted);
    }
}