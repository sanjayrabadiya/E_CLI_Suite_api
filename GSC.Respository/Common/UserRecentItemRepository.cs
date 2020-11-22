using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Common;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared;

namespace GSC.Respository.Common
{
    public class UserRecentItemRepository : GenericRespository<UserRecentItem>, IUserRecentItemRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public UserRecentItemRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
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
                _context.Save();
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

            var result = _context.AppScreen.Where(x => x.ScreenCode == urlName).FirstOrDefault();
            if (result != null)
            {
                var parent = _context.AppScreen.Where(x => x.Id == result.ParentAppScreenId).FirstOrDefault()?.IconPath;
                var finalResult = new ScreenModal {IconPath = parent + "small", UrlName = result.UrlName};
                return finalResult;
            }

            return new ScreenModal();
        }
    }
}