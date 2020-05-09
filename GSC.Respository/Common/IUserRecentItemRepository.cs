using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Common;
using GSC.Data.Entities.Common;

namespace GSC.Respository.Common
{
    public interface IUserRecentItemRepository : IGenericRepository<UserRecentItem>
    {
        void SaveUserRecentItem(UserRecentItem userRecentItem);
        IList<UserRecentItemDto> GetRecentItemByUser();
    }
}