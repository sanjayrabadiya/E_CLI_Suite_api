using GSC.Common.GenericRespository;
using GSC.Data.Entities.Location;

namespace GSC.Respository.Common
{
    public interface ILocationRepository : IGenericRepository<Location>
    {
        Location SaveLocation(Location location);
    }
}