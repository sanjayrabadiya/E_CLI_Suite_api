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
    public class ReportFavouriteScreenRepository : GenericRespository<ReportFavouriteScreen>,
        IReportFavouriteScreenRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ReportFavouriteScreenRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }
        public void Favorite(int ReportId)
        {
            var favorite = FindBy(t => t.UserId == _jwtTokenAccesser.UserId && t.ReportId == ReportId).FirstOrDefault();
            if (favorite != null)
            {
               Remove(favorite);
            }
            else
            {
                favorite = new ReportFavouriteScreen
                {
                    UserId = _jwtTokenAccesser.UserId,
                    ReportId = ReportId
                };

                Add(favorite);
            }
        }

        public List<ReportFavouriteScreenDto> GetFavoriteByUserId()
        {
            if (_jwtTokenAccesser.UserId == 0) return new List<ReportFavouriteScreenDto>();

            var favoriteScreenIds =
                FindBy(t => t.UserId == _jwtTokenAccesser.UserId).Select(t => t.ReportId).ToList();

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
                t.IsAdd = isPowerAdmin || p.Any(s => s.IsAdd);
                t.IsDelete = isPowerAdmin || p.Any(s => s.IsDelete);
                t.IsEdit = isPowerAdmin || p.Any(s => s.IsEdit);
                t.IsExport = isPowerAdmin || p.Any(s => s.IsExport);
                t.IsView = isPowerAdmin || p.Any(s => s.IsView);
                t.IsView = isPowerAdmin || p.Any(s => s.IsView);
                t.IsFavorited = favorites.Any(f => f.AppScreenId == t.Id);
            });

            var Reportscreens = _context.ReportScreen.Where(x => favoriteScreenIds.Contains(x.Id))
                .ToList();

            var result = Reportscreens.Select(x => new ReportFavouriteScreenDto
            {
                Id = x.Id,
                ReportName = x.ReportName,
                ReportCode = x.ReportCode
            }).ToList();

            result.ForEach(t =>
            {
                var p = screens.Where(s => s.ScreenCode == t.ReportCode).ToList();
                t.IsAdd = isPowerAdmin || p.Any(s => s.IsAdd);
                t.IsDelete = isPowerAdmin || p.Any(s => s.IsDelete);
                t.IsEdit = isPowerAdmin || p.Any(s => s.IsEdit);
                t.IsExport = isPowerAdmin || p.Any(s => s.IsExport);
                t.IsView = isPowerAdmin || p.Any(s => s.IsView);
                t.IsView = isPowerAdmin || p.Any(s => s.IsView);
            });

            return result;
        }
    }
}
