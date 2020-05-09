using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Volunteer
{
    public class VolunteerImageRepository : GenericRespository<VolunteerImage, GscContext>, IVolunteerImageRepository
    {
        public VolunteerImageRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}