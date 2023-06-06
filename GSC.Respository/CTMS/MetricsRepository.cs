using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Vml;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public class MetricsRepository : GenericRespository<PlanMetrics>, IMetricsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        public MetricsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;

            _mapper = mapper;
            _context = context;
        }
        public List<PlanMetricsGridDto> GetMetricsList(bool isDeleted, int typesId)
        {
            var planMetrics = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.MetricsType == (typesId == 1 ? MetricsType.Enrolled : typesId == 2 ? MetricsType.Screened : MetricsType.Randomized)).
            ProjectTo<PlanMetricsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            foreach (var task in planMetrics)
            {
                if (task != null)
                {
                    var planned = _context.OverTimeMetrics.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.PlanMetricsId == task.Id && x.If_Active == true).ToList(); 
                    task.Planned = (int)planned.Sum(item => item.Planned);
                    task.Actual = (int)planned.Sum(item => item.Actual);
                }
            }
            return planMetrics;
        }
        public string Duplicate(PlanMetrics objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.DeletedDate == null && x.MetricsType == objSave.MetricsType))
                return "Duplicate Study in " + objSave.MetricsType;
            return "";
        }
    }
}
