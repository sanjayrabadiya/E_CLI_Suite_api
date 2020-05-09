using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IClientTypeRepository : IGenericRepository<ClientType>
    {
        string Duplicate(ClientType objSave);

        List<DropDownDto> GetClientTypeDropDown();
    }
}