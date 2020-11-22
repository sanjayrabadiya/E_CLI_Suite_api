using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.UserMgt
{
    public class AppUserClaimRepository : GenericRespository<AppUserClaim, GscContext>, IAppUserClaimRepository
    {
        public AppUserClaimRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}