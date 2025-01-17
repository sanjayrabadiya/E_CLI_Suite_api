using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;

namespace GSC.Respository.Master
{
    public interface IStateRepository : IGenericRepository<State>
    {
        List<DropDownDto> GetStateDropDown(int countryId);
        string DuplicateState(State objSave);
        List<StateGridDto> GetStateList(bool isDeleted);
    }
}