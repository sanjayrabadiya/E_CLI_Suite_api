using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IFreezerRepository : IGenericRepository<Freezer>
    {
        List<DropDownDto> GetFreezerDropDown();
        string Duplicate(Freezer objSave);
    }
}