using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Client
{
    public class ClientAddressRepository : GenericRespository<ClientAddress>, IClientAddressRepository
    {
        private readonly IGSCContext _context;
        public ClientAddressRepository(IGSCContext context) : base(context)
        {
           _context = context;
        }

        public List<ClientAddress> GetAddresses(int clientId, bool isDeleted)
        {
            var addresses = FindByInclude(t => (isDeleted ? t.DeletedDate != null : t.DeletedDate == null) && t.ClientId == clientId, t => t.Location)
                .OrderByDescending(t => t.Id).ToList();

            foreach (var address in addresses)
            {
                if (address.Location == null)
                    continue;

                var id = address.Location.CountryId;
                address.Location.CountryName = id > 0 ? _context.Country.Find(id).CountryName : "";

                id = address.Location.StateId;
                address.Location.StateName = id > 0 ? _context.State.Find(id).StateName : "";

                id = address.Location.CityId;
                address.Location.CityName = id > 0 ? _context.City.Find(id).CityName : "";

                id = address.Location.CityAreaId;
                address.Location.CityAreaName = id > 0 ? _context.CityArea.Find(id).AreaName : "";
            }

            return addresses;
        }
    }
}