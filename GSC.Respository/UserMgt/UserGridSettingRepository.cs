using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.UserMgt
{
    public class UserGridSettingRepository : GenericRespository<UserGridSetting, GscContext>, IUserGridSettingRepository
    {
        public UserGridSettingRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}