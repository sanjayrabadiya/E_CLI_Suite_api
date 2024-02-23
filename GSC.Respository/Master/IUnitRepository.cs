using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IUnitRepository : IGenericRepository<Unit>
    {
        List<DropDownDto> GetUnitDropDown();
        string Duplicate(Unit objSave);
        List<UnitGridDto> GetUnitList(bool isDeleted);
        List<DropDownDto> GetUnitAsModule(string screenCode);
    }
}