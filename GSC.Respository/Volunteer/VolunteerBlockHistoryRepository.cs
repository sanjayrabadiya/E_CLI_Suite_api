using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public VolunteerBlockHistoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public IList<VolunteerBlockHistoryDto> GetVolunteerBlockHistoryById(int volunteerId)
        {
            //var volunteerBlcokHistory = _context.VolunteerBlockHistory.Where(x => x.DeletedDate == null && x.VolunteerId == volunteerId)
            //    .ProjectTo<VolunteerBlockHistoryDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var result = _context.VolunteerBlockHistory.Where(x => x.DeletedDate == null && x.VolunteerId == volunteerId).Select(x => new VolunteerBlockHistoryDto
            {
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                PermanentlyString = x.IsPermanently ? "Yes" : "No",
                BlockString = x.IsBlock ? "Yes" : "No",
                Note = x.Note,
                UserName = x.CreatedByUser.UserName,
                BlockDate = x.CreatedDate,
                CategoryName = x.BlockCategory.BlockCategoryName
            }).OrderByDescending(x => x.Id).ToList();

            return result;

            //return FindByInclude(x => x.DeletedDate == null && x.VolunteerId == volunteerId, x => x.CreatedByUser,
            //    x => x.BlockCategory).Select(
            //    c => new VolunteerBlockHistoryDto
            //    {
            //        FromDate = c.FromDate,
            //        ToDate = c.ToDate,
            //        PermanentlyString = c.IsPermanently ? "Yes" : "No",
            //        BlockString = c.IsBlock ? "Yes" : "No",
            //        Note = c.Note,
            //        UserName = c.CreatedByUser.UserName,
            //        BlockDate = c.CreatedDate,
            //        CategoryName = c.BlockCategory.BlockCategoryName
            //    }).OrderByDescending(i => i.BlockDate).ToList();
        }
    }
}