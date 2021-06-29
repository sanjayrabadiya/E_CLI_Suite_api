using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerBlockHistoryRepository : IGenericRepository<VolunteerBlockHistory>
    {
        IList<VolunteerBlockHistoryGridDto> GetVolunteerBlockHistoryById(int volunteerId);
    }
}