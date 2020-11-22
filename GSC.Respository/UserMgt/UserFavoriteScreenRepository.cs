using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.UserMgt
{
    public class UserFavoriteScreenRepository : GenericRespository<UserFavoriteScreen>,
        IUserFavoriteScreenRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public UserFavoriteScreenRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void Favorite(int appScreenId)
        {
            var favorite = FindBy(t => t.UserId == _jwtTokenAccesser.UserId && t.AppScreenId == appScreenId)
                .FirstOrDefault();
            if (favorite != null)
            {
                Remove(favorite);
            }
            else
            {
                favorite = new UserFavoriteScreen
                {
                    UserId = _jwtTokenAccesser.UserId,
                    AppScreenId = appScreenId
                };

                Add(favorite);
            }
        }

        public List<MenuDto> GetFavoriteByUserId()
        {
            if (_jwtTokenAccesser.UserId == 0) return new List<MenuDto>();

            var favoriteScreenIds =
                FindBy(t => t.UserId == _jwtTokenAccesser.UserId).Select(t => t.AppScreenId).ToList();

            var screens = _context.AppScreen.Where(x => favoriteScreenIds.Contains(x.Id) && x.DeletedDate == null)
                .ToList();

            return screens.Select(x => new MenuDto
            {
                Id = x.Id,
                Title = x.ScreenName,
                Url = x.UrlName,
                Icon = x.IconPath,
                IsFavorited = true
            }).ToList();
        }
    }
}