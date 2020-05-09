using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IMaritalStatusRepository : IGenericRepository<MaritalStatus>
    {
        List<DropDownDto> GetMaritalStatusDropDown();
        string Duplicate(MaritalStatus objSave);
    }
}