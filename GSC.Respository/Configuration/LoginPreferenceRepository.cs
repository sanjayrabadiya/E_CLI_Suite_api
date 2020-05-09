using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Configuration
{
    public class LoginPreferenceRepository : GenericRespository<LoginPreference, GscContext>, ILoginPreferenceRepository
    {
        public LoginPreferenceRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}