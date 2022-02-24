using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportVariableValueRepository : GenericRespository<CtmsMonitoringReportVariableValue>, ICtmsMonitoringReportVariableValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CtmsMonitoringReportVariableValueRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public void UpdateChild(List<CtmsMonitoringReportVariableValueChild> children)
        {
            _context.CtmsMonitoringReportVariableValueChild.UpdateRange(children);
        }

        public List<CtmsMonitoringReportVariableValueBasic> GetVariableValues(int CtmsMonitoringReportId)
        {
            return All.Include(x => x.CtmsMonitoringReport).AsNoTracking().Where(t => t.CtmsMonitoringReportId == CtmsMonitoringReportId)
                    .ProjectTo<CtmsMonitoringReportVariableValueBasic>(_mapper.ConfigurationProvider).ToList();
        }
    }
}