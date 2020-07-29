using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Client
{
    public class ClientAddressRepository : GenericRespository<ClientAddress, GscContext>, IClientAddressRepository
    {
        public ClientAddressRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public List<ClientAddress> GetAddresses(int clientId, bool isDeleted)
        {
            var addresses = FindByInclude(t => t.ClientId == clientId && isDeleted ? t.DeletedDate != null : t.DeletedDate == null, t => t.Location)
                .OrderByDescending(t => t.Id).ToList();

            foreach (var address in addresses)
            {
                if (address.Location == null)
                    continue;

                var id = address.Location.CountryId;
                address.Location.CountryName = id > 0 ? Context.Country.Find(id).CountryName : "";

                id = address.Location.StateId;
                address.Location.StateName = id > 0 ? Context.State.Find(id).StateName : "";

                id = address.Location.CityId;
                address.Location.CityName = id > 0 ? Context.City.Find(id).CityName : "";

                id = address.Location.CityAreaId;
                address.Location.CityAreaName = id > 0 ? Context.CityArea.Find(id).AreaName : "";
            }

            return addresses;
        }
    }
}