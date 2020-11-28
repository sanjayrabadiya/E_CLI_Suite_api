using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Common
{
    public class LocationRepository : GenericRespository<Location>, ILocationRepository
    {
        public LocationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }

        public Location SaveLocation(Location location)
        {
            if (location == null || string.IsNullOrEmpty(location.Address))
                return null;

            var result = All.Where(x => x.Address == location.Address
                                                 && x.CountryId == location.CountryId
                                                 && x.StateId == location.StateId
                                                 && x.CityId == location.CityId
                                                 && x.CityAreaId == location.CityAreaId
                                                 && x.Zip == location.Zip).FirstOrDefault();
            if (result != null)
                return result;

            return location;
        }
    }
}