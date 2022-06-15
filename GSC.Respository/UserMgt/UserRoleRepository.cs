using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.UserMgt
{
    public class UserRoleRepository : GenericRespository<UserRole>, IUserRoleRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public UserRoleRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IRolePermissionRepository rolePermissionRepository)
            : base(context)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<DropDownDto> GetRoleByUserName(string userName)
        {
            return All.Include(x => x.User).Include(x => x.SecurityRole).Where(x =>
                       x.User.DeletedDate == null && x.User.UserName == userName && x.DeletedDate == null &&
                       x.SecurityRole.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.SecurityRole.Id, Value = c.SecurityRole.RoleName })
                .OrderBy(o => o.Value).ToList();
        }

        public IList<DropDownDto> GetUserNameByRoleId(int roleId)
        {
            return All.Where(x => x.SecurityRole.Id == roleId)
                .Select(c => new DropDownDto { Id = c.User.Id, Value = c.User.FirstName + ' ' + c.User.LastName, IsDeleted = c.DeletedDate != null })
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

        public IList<string> GetUserEmailByRole(int roleId)
        {
            return All.Include(x => x.User).Include(x => x.SecurityRole).Where(x => x.SecurityRole.Id == roleId &&
                       x.User.DeletedDate == null && x.DeletedDate == null  && 
                       !(x.User.ValidFrom.HasValue && x.User.ValidFrom.Value > DateTime.Now || x.User.ValidTo.HasValue 
                       && x.User.ValidTo.Value < DateTime.Now))
                .Select(c => c.User.Email).ToList();
        }

        public IList<DropDownDto> GetRoleByUserId(int Userid)
        {
            return All.Include(x => x.User).Include(x => x.SecurityRole).Where(x =>
                       x.User.DeletedDate == null && x.User.Id == Userid && x.DeletedDate == null &&
                       x.SecurityRole.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.SecurityRole.Id, Value = c.SecurityRole.RoleName })
                .OrderBy(o => o.Value).ToList();
        }
    }
}