using GSC.Common.GenericRespository;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;

namespace GSC.Respository.Volunteer
{
    public class VolunteerHistoryRepository : GenericRespository<VolunteerHistory>,
        IVolunteerHistoryRepository
    {
        public VolunteerHistoryRepository(IGSCContext context)
            : base(context)
        {
        }
    }
}