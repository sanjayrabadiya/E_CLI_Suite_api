using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Client;
using GSC.Data.Entities.Client;

namespace GSC.Respository.Client
{
    public interface IClientContactRepository : IGenericRepository<ClientContact>
    {
        IList<ClientContactDto> GetContactList(int clientId);

        string DuplicateContact(ClientContact objSave);
    }
}