using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsActionPointRepository : GenericRespository<CtmsActionPoint>, ICtmsActionPointRepository
    {
        private readonly IMapper _mapper;
        public CtmsActionPointRepository(IGSCContext context,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }
        public List<CtmsActionPointGridDto> GetActionPointList(int CtmsMonitoringId)
        {
            return All.Where(x => x.CtmsMonitoringId == CtmsMonitoringId).
                   ProjectTo<CtmsActionPointGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public List<CtmsActionPointGridDto> GetActionPointForFollowUpList(int ProjectId)
        {
            return All.Include(x => x.CtmsMonitoring).Where(x => x.CtmsMonitoring.ProjectId == ProjectId && x.Status == CtmsActionPointStatus.Open).
                   ProjectTo<CtmsActionPointGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
