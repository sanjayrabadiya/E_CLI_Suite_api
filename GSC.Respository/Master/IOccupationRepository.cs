using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IOccupationRepository : IGenericRepository<Occupation>
    {
        List<DropDownDto> GetOccupationDropDown();
        string Duplicate(Occupation objSave);
    }
}