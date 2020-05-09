using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Master;

namespace GSC.Respository.Client
{
    public interface IClientRepository : IGenericRepository<Data.Entities.Client.Client>
    {
        List<DropDownDto> GetClientDropDown();
        string DuplicateClient(Data.Entities.Client.Client objSave);
    }
}