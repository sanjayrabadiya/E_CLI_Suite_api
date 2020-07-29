using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Client;

namespace GSC.Respository.Client
{
    public interface IClientAddressRepository : IGenericRepository<ClientAddress>
    {
        List<ClientAddress> GetAddresses(int clientId, bool isDeleted);
    }
}