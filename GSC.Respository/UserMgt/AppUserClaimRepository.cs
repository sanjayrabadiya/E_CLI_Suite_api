using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.UserMgt
{
    public class AppUserClaimRepository : GenericRespository<AppUserClaim>, IAppUserClaimRepository
    {
        public AppUserClaimRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}