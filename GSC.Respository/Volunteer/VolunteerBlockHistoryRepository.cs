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

        public IList<VolunteerBlockHistoryGridDto> GetVolunteerBlockHistoryById(int volunteerId)
        {
            return All.Where(x => x.DeletedDate == null && x.VolunteerId == volunteerId)
                .ProjectTo<VolunteerBlockHistoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(t => t.Id).ToList();
        }

        public IList<VolunteerBlockHistoryGridDto> GetTemporaryVolunteer()
        {
            return All.Where(x => x.DeletedDate == null && x.ToDate != null && x.ToDate <= System.DateTime.Now)
                .ProjectTo<VolunteerBlockHistoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(t => t.Id).ToList();
        }
    }
}