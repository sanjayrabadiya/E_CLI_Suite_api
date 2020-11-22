using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Volunteer
{
    public class VolunteerAddressRepository : GenericRespository<VolunteerAddress, GscContext>,
        IVolunteerAddressRepository
    {
        public VolunteerAddressRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public List<VolunteerAddress> GetAddresses(int volunteerId)
        {
            var addresses = FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.Location)
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