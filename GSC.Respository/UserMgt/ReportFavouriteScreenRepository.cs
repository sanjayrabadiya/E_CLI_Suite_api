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

            var screens = _context.ReportScreen.Where(x => favoriteScreenIds.Contains(x.Id))
                .ToList();

            return screens.Select(x => new ReportFavouriteScreenDto
            {
                Id = x.Id,
                ReportName = x.ReportName
            }).ToList();
        }
    }
}
