using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Common;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Common
{
    public class UserRecentItemRepository : GenericRespository<UserRecentItem, GscContext>, IUserRecentItemRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UserRecentItemRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public void SaveUserRecentItem(UserRecentItem userRecentItem)
        {
            var result = All.Where(x => x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(c => c.CreatedDate)
                .Take(7).ToList();

            if (!result.Any(c => c.ScreenType == userRecentItem.ScreenType && c.KeyId == userRecentItem.KeyId))
            {
                userRecentItem.UserId = _jwtTokenAccesser.UserId;
                userRecentItem.RoleId = _jwtTokenAccesser.RoleId;
                Add(userRecentItem);
                Context.SaveChanges(_jwtTokenAccesser);
            }
        }

        public IList<UserRecentItemDto> GetRecentItemByUser()
        {
            var result = All.Where(x => x.UserId == _jwtTokenAccesser.UserId).OrderByDescending(c => c.CreatedDate)
                .Take(7).ToList();
            return result.Select(x => new UserRecentItemDto
            {
                KeyId = x.KeyId,
                ScreenType = x.ScreenType,
                SubjectName = x.SubjectName,
                SubjectName1 = x.SubjectName1,
                ScreenName = x.ScreenType.GetDescription(),
                ScreenModal = GetUrlName(x.ScreenType)
            }).ToList();
        }

        private ScreenModal GetUrlName(UserRecent userRecent)
        {
            var urlName = "";
            switch (userRecent)
            {
                case UserRecent.Project:
                    urlName = "mnu_projectmaster";
                    break;
                case UserRecent.Volunteer:
                    urlName = "mnu_volunteerlist";
                    break;
                case UserRecent.ProjectDesign:
                    urlName = "mnu_projectdesign";
                    break;
                case UserRecent.Screening:
                    urlName = "mnu_screening";
                    break;
            }

            var result = Context.AppScreen.Where(x => x.ScreenCode == urlName).FirstOrDefault();
            if (result != null)
            {
                var parent = Context.AppScreen.FirstOrDefault(x => x.Id == result.ParentAppScreenId)?.IconPath;
                var finalResult = new ScreenModal {IconPath = parent + "small", UrlName = result.UrlName};
                return finalResult;
            }

            return new ScreenModal();
        }
    }
}