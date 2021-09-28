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

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringVisitRepository : GenericRespository<ManageMonitoringVisit>, IManageMonitoringVisitRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ManageMonitoringVisitRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<ManageMonitoringVisitDto> GetMonitoringVisit(int projectId)
        {
            var Activity = _context.Activity.Where(x => x.DeletedDate == null && x.ModuleId == Helper.AuditModule.CTMS).ToList();

            var result = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId)
                .Select(x => new ManageMonitoringVisitDto
                {
                    Id = x.Id,
                    ActivityId = x.ActivityId,
                    ScheduleStartDate = x.ScheduleStartDate,
                    ScheduleEndDate = x.ScheduleEndDate,
                    ActualStartDate = x.ActualStartDate,
                    ActualEndDate = x.ActualEndDate,
                }).ToList();

            var data = Activity.Select(x => new ManageMonitoringVisitDto
            {
                Id = result.Where(y => y.ActivityId == x.Id).FirstOrDefault() != null ? result.Where(y => y.ActivityId == x.Id).FirstOrDefault().Id : 0,
                ActivityName = x.ActivityName,
                ActivityId = x.Id,
                ScheduleStartDate = result.Where(y => y.ActivityId == x.Id).FirstOrDefault()?.ScheduleStartDate,
                ScheduleEndDate = result.Where(y => y.ActivityId == x.Id).FirstOrDefault()?.ScheduleEndDate,
                ActualStartDate = result.Where(y => y.ActivityId == x.Id).FirstOrDefault()?.ActualStartDate,
                ActualEndDate = result.Where(y => y.ActivityId == x.Id).FirstOrDefault()?.ActualEndDate,
            }).ToList();

            return data;
        }
    }
}