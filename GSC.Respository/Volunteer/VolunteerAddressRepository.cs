using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;

namespace GSC.Respository.Volunteer
{
    public class VolunteerAddressRepository : GenericRespository<VolunteerAddress>,
        IVolunteerAddressRepository
    {
        private readonly IGSCContext _context;
        public VolunteerAddressRepository(IGSCContext context) : base(context)
        {
            _context = context;
        }

        public List<VolunteerAddress> GetAddresses(int volunteerId)
        {
            var addresses = FindByInclude(t => t.VolunteerId == volunteerId && t.DeletedDate == null, t => t.Location)
                .OrderByDescending(t => t.Id).ToList();

            for (int i = 0; i < addresses.Count; i++)
            {
                VolunteerAddress address = addresses[i];
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