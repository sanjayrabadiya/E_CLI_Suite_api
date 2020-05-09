using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IUserFavoriteScreenRepository : IGenericRepository<UserFavoriteScreen>
    {
        void Favorite(int appScreenId);
        List<MenuDto> GetFavoriteByUserId();
    }
}