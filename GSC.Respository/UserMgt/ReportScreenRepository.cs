using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.UserMgt
{
    public class ReportScreenRepository : GenericRespository<ReportScreen>,
        IReportScreenRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public ReportScreenRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }
        public List<ReportScreenDto> GetReportScreen()
        {
            var reportmainmenu = _context.AppScreen.Where(x => x.ScreenCode == "mnu_report").FirstOrDefault();

            var screens = _context.AppScreen.Where(x => x.ParentAppScreenId == reportmainmenu.Id && x.DeletedDate == null).ToList();

            var isPowerAdmin = _context.Users.Find(_jwtTokenAccesser.UserId).IsPowerAdmin;

            var permissions = new List<RolePermission>();
            if (!isPowerAdmin)
                permissions = _context.RolePermission.Where(t => t.UserRoleId == _jwtTokenAccesser.RoleId && t.DeletedDate == null)
                    .ToList();

            var favorites = _context.UserFavoriteScreen.Where(t => t.UserId == _jwtTokenAccesser.UserId && t.DeletedDate == null).ToList();

            screens.ForEach(t =>
            {
                var p = permissions.Where(s => s.ScreenCode == t.ScreenCode).ToList();
                t.IsAdd = isPowerAdmin || p.Exists(s => s.IsAdd);
                t.IsDelete = isPowerAdmin || p.Exists(s => s.IsDelete);
                t.IsEdit = isPowerAdmin || p.Exists(s => s.IsEdit);
                t.IsExport = isPowerAdmin || p.Exists(s => s.IsExport);
                t.IsView = isPowerAdmin || p.Exists(s => s.IsView);
                t.IsView = isPowerAdmin || p.Exists(s => s.IsView);
                t.IsFavorited = favorites.Exists(f => f.AppScreenId == t.Id);
            });

            var result = All
                .Select(c => new ReportScreenDto { Id = c.Id, ReportCode = c.ReportCode, ReportName = c.ReportName, ReportGroup = c.ReportGroup, IsFavourite = false }).OrderBy(o => o.Id).ToList();

            result.ForEach(t =>
            {
                var p = screens.Where(s => s.ScreenCode == t.ReportCode).ToList();
                t.IsAdd = isPowerAdmin || p.Exists(s => s.IsAdd);
                t.IsDelete = isPowerAdmin || p.Exists(s => s.IsDelete);
                t.IsEdit = isPowerAdmin || p.Exists(s => s.IsEdit);
                t.IsExport = isPowerAdmin || p.Exists(s => s.IsExport);
                t.IsView = isPowerAdmin || p.Exists(s => s.IsView);
                t.IsView = isPowerAdmin || p.Exists(s => s.IsView);
            });

            return result;
        }
    }
}
