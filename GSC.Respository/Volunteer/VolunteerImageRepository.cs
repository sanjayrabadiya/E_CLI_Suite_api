using GSC.Common.GenericRespository;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;

namespace GSC.Respository.Volunteer
{
    public class VolunteerImageRepository : GenericRespository<VolunteerImage>, IVolunteerImageRepository
    {
        public VolunteerImageRepository(IGSCContext context)
            : base(context)
        {
        }
    }
}