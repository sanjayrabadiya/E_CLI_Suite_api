using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.UserMgt
{
    public class RolePermissionRepository : GenericRespository<RolePermission>, IRolePermissionRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public RolePermissionRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void Save(List<RolePermission> rolePermissions)
        {
            var userRoleId = rolePermissions.First().UserRoleId;

            var existing = _context.RolePermission.Where(t => t.UserRoleId == userRoleId).ToList();
            if (existing.Any())
            {
                _context.RolePermission.RemoveRange(existing);
                 _context.Save();
            }

            rolePermissions = rolePermissions.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .ToList();

            _context.RolePermission.AddRange(rolePermissions);
             _context.Save();
        }
       
        public void updatePermission(List<RolePermission> rolePermissions)
        {
            var userRoleId = rolePermissions.First().UserRoleId;

            var existing = _context.RolePermission.Where(t => t.UserRoleId == userRoleId).ToList();
            if (existing.Any())
            {
                _context.RolePermission.RemoveRange(existing);
                 _context.Save();
            }

            rolePermissions = rolePermissions.Where(t => t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .ToList();
            _context.RolePermission.UpdateRange(rolePermissions);
             _context.Save();
        }

        public List<RolePermissionDto> GetByRoleId(int roleId)
        {
            var permissions = _context.AppScreen.Where(t => t.IsPermission && t.DeletedDate == null).Select(t =>
                new RolePermissionDto
                {
                    CanAdd = t.IsAdd,
                    CanDelete = t.IsDelete,
                    CanAll = true,
                    CanEdit = t.IsEdit,
                    CanExport = t.IsExport,
                    CanView = t.IsView,
                    AppScreenId = t.Id,
                    ScreenCode = t.ScreenCode,
                    ScreenName = t.ScreenName,
                    ParentAppScreenId = t.ParentAppScreenId,
                    hasChild = t.ParentAppScreenId != null ? false : true
                }).ToList();

            permissions.ForEach(t =>
            {
                t.UserRoleId = roleId;
                var p = _context.RolePermission.Where(s =>
                    s.ScreenCode == t.ScreenCode && s.UserRoleId == roleId).FirstOrDefault();
                if (p == null) return;
                t.IsAdd = p.IsAdd;
                t.RolePermissionId = p.Id;
                t.IsDelete = p.IsDelete;
                t.IsEdit = p.IsEdit;
                t.IsExport = p.IsExport;
                t.IsView = p.IsView;
                t.IsAll = p.IsAdd && p.IsDelete && p.IsEdit && p.IsExport && p.IsView;
            });

            return permissions;
        }

        public List<AppScreen> GetByUserId(int userId, int roleId)
        {
            if (userId == 0) return new List<AppScreen>();
            var screens = _context.AppScreen.Where(x => x.DeletedDate == null).ToList();

            var isPowerAdmin = _context.Users.Find(userId).IsPowerAdmin;

            var permissions = new List<RolePermission>();
            if (!isPowerAdmin)
                permissions = _context.RolePermission.Where(t => t.UserRoleId == roleId && t.DeletedDate == null)
                    .ToList();

            var favorites = _context.UserFavoriteScreen.Where(t => t.UserId == userId && t.DeletedDate == null).ToList();

            screens.ForEach(t =>
            {
                var p = permissions.Where(s => s.ScreenCode == t.ScreenCode).ToList();
                t.IsAdd = isPowerAdmin || p.Any(s => s.IsAdd);
                t.IsDelete = isPowerAdmin || p.Any(s => s.IsDelete);
                t.IsEdit = isPowerAdmin || p.Any(s => s.IsEdit);
                t.IsExport = isPowerAdmin || p.Any(s => s.IsExport);
                t.IsView = isPowerAdmin || p.Any(s => s.IsView);
                t.IsView = isPowerAdmin || p.Any(s => s.IsView);
                t.IsFavorited = favorites.Any(f => f.AppScreenId == t.Id);
            });
            screens = screens.Where(t => !t.IsPermission || t.IsAdd || t.IsEdit || t.IsDelete || t.IsView || t.IsExport)
                .ToList();
            return screens;
        }

        public RolePermission GetRolePermissionByScreenCode(string screenCode)
        {
            var isPowerAdmin = _context.Users.Find(_jwtTokenAccesser.UserId).IsPowerAdmin;
            if (isPowerAdmin)
                return new RolePermission
                    {IsAdd = true, IsView = true, IsDelete = true, IsEdit = true, IsExport = true};
            var rolePermission = FindBy(x =>
                    x.ScreenCode == screenCode && x.UserRoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null)
                .FirstOrDefault();

            if (rolePermission != null)
                return rolePermission;
            return new RolePermission();
        }
    }
}