using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Project.Design
{
    public class VisitEmailConfigurationRepository : GenericRespository<VisitEmailConfiguration>, IVisitEmailConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public VisitEmailConfigurationRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public List<VisitEmailConfigurationGridDto> GetVisitEmailConfigurationList(bool isDeleted,int projectDesignVisitId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectDesignVisitId== projectDesignVisitId).
                   ProjectTo<VisitEmailConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(VisitEmailConfiguration objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.VisitStatusId == objSave.VisitStatusId && x.ProjectDesignVisitId==objSave.ProjectDesignVisitId && x.DeletedDate == null))
                return "Duplicate visit status.";

            return "";
        }

    }
}