using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Design
{
    public class VisitEmailConfigurationRolesRepository : GenericRespository<VisitEmailConfigurationRoles>, IVisitEmailConfigurationRolesRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public VisitEmailConfigurationRolesRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }
    }
}
