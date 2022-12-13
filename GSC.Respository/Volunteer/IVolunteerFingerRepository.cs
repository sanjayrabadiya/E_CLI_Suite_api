using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerFingerRepository : IGenericRepository<Data.Entities.Volunteer.VolunteerFinger>
    {
        List<DbRecords> GetFingers();
    }
}