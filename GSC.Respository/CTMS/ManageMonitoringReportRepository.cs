using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringReportRepository : GenericRespository<ManageMonitoringReport>, IManageMonitoringReportRepository
    {
        private readonly IManageMonitoringVisitRepository _manageMonitoringVisitRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ManageMonitoringReportRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IManageMonitoringVisitRepository manageMonitoringVisitRepository,
            IVariableTemplateRepository variableTemplateRepository)
            : base(context)
        {
            _manageMonitoringVisitRepository = manageMonitoringVisitRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _variableTemplateRepository = variableTemplateRepository;
        }


        public List<ManageMonitoringReportGridDto> GetMonitoringReport(int projectId)
        {
            var visit = _manageMonitoringVisitRepository.All.Include(z => z.Activity)
                .Where(x => x.ProjectId == projectId && x.DeletedDate == null && x.ActualStartDate != null).ToList();

            var ManageMonitoringReport = All.Where(x => x.DeletedDate == null).ToList();

            var result = visit.Select(x => new ManageMonitoringReportGridDto
            {
                Id = ManageMonitoringReport.Any(t => t.ManageMonitoringVisitId == x.Id) ? ManageMonitoringReport.Where(t => t.ManageMonitoringVisitId == x.Id).LastOrDefault().Id : 0,
                ManageMonitoringVisitId = x.Id,
                Status = MonitoringReportStatus.NotInitiated.GetDescription(),
                StatusId = MonitoringReportStatus.NotInitiated,
                ActualStartDate = x.ActualStartDate,
                ActualEndDate = x.ActualEndDate,
                ActivityName = x.Activity.ActivityName,
                VariableTemplate = _variableTemplateRepository.All.Where(y => y.ActivityId == x.ActivityId).FirstOrDefault().TemplateName,
                VariableTemplateId = _variableTemplateRepository.All.Where(y => y.ActivityId == x.ActivityId).FirstOrDefault().Id
            }).ToList();

            return result;
        }
    }
}