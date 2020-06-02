using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.UserMgt
{
    public class UserRoleRepository : GenericRespository<UserRole, GscContext>, IUserRoleRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public UserRoleRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IRolePermissionRepository rolePermissionRepository)
            : base(uow, jwtTokenAccesser)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<DropDownDto> GetRoleByUserName(string userName)
        {
            return All.Where(x =>
                    x.User.DeletedDate == null && x.User.UserName == userName && x.DeletedDate == null &&
                    x.SecurityRole.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.SecurityRole.Id, Value = c.SecurityRole.RoleName})
                .OrderBy(o => o.Value).ToList();
        }

        public IList<MenuDto> GetMenuList()
        {
            var menus = _rolePermissionRepository.GetByUserId(_jwtTokenAccesser.UserId, _jwtTokenAccesser.RoleId)
                .Where(t => t.IsMenu).ToList();
            return PreparedMenu(menus, menus.Where(x => x.ParentAppScreenId == null).OrderBy(a => a.SeqNo).ToList());
        }

        public IList<DropDownDto> GetUserNameByRoleId(int roleId)
        {
            return All.Where(x => x.SecurityRole.Id == roleId)
                .Select(c => new DropDownDto {Id = c.User.Id, Value = c.User.FirstName + ' ' + c.User.LastName, IsDeleted = c.DeletedDate != null })
                .OrderBy(o => o.Value).ToList();
        }

        private List<MenuDto> PreparedMenu(List<AppScreen> appScreens, List<AppScreen> childAppScreens)
        {
            return childAppScreens.Select(x => new MenuDto
            {
                Id = x.Id,
                Title = x.ScreenName,
                Url = x.UrlName,
                Icon = x.IconPath,
                Type = x.ParentAppScreenId == null ? "collapsable" : "item",
                ExactMatch = true,
                IsFavorited = x.IsFavorited,
                Children = PreparedMenu(appScreens,
                        appScreens.Where(c => c.ParentAppScreenId == x.Id).OrderBy(a => a.SeqNo).ToList())
                    .ToList()
            }).ToList();
        }
    }
}