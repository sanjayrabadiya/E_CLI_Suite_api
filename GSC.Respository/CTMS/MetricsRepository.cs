using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class MetricsRepository : GenericRespository<PlanMetrics>, IMetricsRepository
    {
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectRightRepository _projectRightRepository;
        public MetricsRepository(IGSCContext context,
            IMapper mapper, IProjectRightRepository projectRightRepository) : base(context)
        {

            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }
        public List<PlanMetricsGridDto> GetMetricsList(bool isDeleted, int typesId)
        {
            var projectList = _projectRightRepository.GetProjectCTMSRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var planMetrics = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.MetricsType == GetMetricsType(typesId)
             && projectList.Any(c => c == x.ProjectId)).
            ProjectTo<PlanMetricsGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            foreach (var task in planMetrics)
            {
                if (task != null)
                {
                    var planned = _context.OverTimeMetrics.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.PlanMetricsId == task.Id && x.If_Active == true).ToList();
                    task.Planned = planned.Sum(item => item.Planned);
                    task.Actual = (int)planned.Sum(item => item.Actual);
                }
            }
            return planMetrics;
        }
        public MetricsType GetMetricsType(int typesId)
        {
            if (typesId == 1)
                return MetricsType.Enrolled;
            else if (typesId == 2)
                return MetricsType.Screened;
            else
                return MetricsType.Randomized;
        }
        public string Duplicate(PlanMetrics objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.DeletedDate == null && x.MetricsType == objSave.MetricsType))
                return "Duplicate Study in " + objSave.MetricsType;
            return "";
        }
    }
}
