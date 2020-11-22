using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Configuration
{
    public class LoginPreferenceRepository : GenericRespository<LoginPreference>, ILoginPreferenceRepository
    {
        public LoginPreferenceRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}