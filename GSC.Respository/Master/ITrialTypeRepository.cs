using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ITrialTypeRepository : IGenericRepository<TrialType>
    {
        List<DropDownDto> GetTrialTypeDropDown();
        string Duplicate(TrialType objSave);
    }
}