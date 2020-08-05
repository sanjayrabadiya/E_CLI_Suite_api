using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IScopeNameRepository : IGenericRepository<ScopeName>
    {
        List<DropDownDto> GetScopeNameDropDown();
        string Duplicate(ScopeName objSave);
        List<ScopeNameGridDto> GetScopeNameList(bool isDeleted);
    }
}