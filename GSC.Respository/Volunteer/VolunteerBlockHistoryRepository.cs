using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Volunteer
{
    public class VolunteerBlockHistoryRepository : GenericRespository<VolunteerBlockHistory>,
        IVolunteerBlockHistoryRepository
    {
        public VolunteerBlockHistoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }

        public IList<VolunteerBlockHistoryDto> GetVolunteerBlockHistoryById(int volunteerId)
        {
            return FindByInclude(x => x.DeletedDate == null && x.VolunteerId == volunteerId, x => x.CreatedByUser,
                x => x.BlockCategory).Select(
                c => new VolunteerBlockHistoryDto
                {
                    FromDate = c.FromDate,
                    ToDate = c.ToDate,
                    PermanentlyString = c.IsPermanently ? "Yes" : "No",
                    BlockString = c.IsBlock ? "Yes" : "No",
                    Note = c.Note,
                    UserName = c.CreatedByUser.UserName,
                    BlockDate = c.CreatedDate,
                    CategoryName = c.BlockCategory.BlockCategoryName
                }).OrderByDescending(i => i.BlockDate).ToList();
        }
    }
}