using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.UserMgt
{
    public class UserImageRepository : GenericRespository<UserImage>, IUserImageRepository
    {
        public UserImageRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}