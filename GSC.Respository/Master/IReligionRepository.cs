using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IReligionRepository : IGenericRepository<Religion>
    {
        List<DropDownDto> GetReligionDropDown();
        string Duplicate(Religion objSave);
        List<ReligionGridDto> GetReligionList(bool isDeleted);
    }
}