using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerAddressRepository : IGenericRepository<VolunteerAddress>
    {
        List<VolunteerAddress> GetAddresses(int volunteerId);
    }
}