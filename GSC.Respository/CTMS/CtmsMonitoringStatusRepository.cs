using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringStatusRepository : GenericRespository<CtmsMonitoringStatus>, ICtmsMonitoringStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CtmsMonitoringStatusRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<CtmsMonitoringStatusGridDto> GetCtmsMonitoringStatusList(int CtmsMonitoringId)
        {
            return All.Where(x => x.CtmsMonitoringId == CtmsMonitoringId).
                   ProjectTo<CtmsMonitoringStatusGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public CtmsMonitoringStatusGridDto GetSiteStatus(int ProjectId)
        {
            var result = All.Where(x => x.CtmsMonitoring.ProjectId == ProjectId && x.DeletedDate == null)
                        .Select(x => new CtmsMonitoringStatusGridDto
                        {
                            Id = x.Id,
                            ActivityName = x.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                            StatusName = x.Status.GetDescription(),
                            Status = x.Status
                        }).OrderByDescending(x => x.Id).ToList();

            return result.FirstOrDefault();
        }

        public string GetFormApprovedOrNot(int projectId, int siteId, int tabNumber)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();

            string ActivityCode = tabNumber == 0 ? "act_001" : tabNumber == 1 ? "act_002" : tabNumber == 2 ? "act_003" :
                tabNumber == 3 ? "act_004" : "act_005";

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == ActivityCode && x.DeletedDate == null).FirstOrDefault();

            var Activity = _context.Activity.Where(x => x.CtmsActivityId == CtmsActivity.Id && x.AppScreenId == appscreen.Id && x.DeletedDate == null).FirstOrDefault();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                                .Where(x => x.ProjectId == projectId && x.ActivityId == Activity.Id
                                && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var CtmsMonitoringReport = All.Where(x => x.CtmsMonitoring.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId)
                                       && x.CtmsMonitoring.DeletedDate == null).ToList();
            //if (!(CtmsMonitoringReport.Count() != 0 && CtmsMonitoringReport.All(z => z.ReportStatus == MonitoringReportStatus.FormApproved)))
            //    return "Please Complete Previous Form.";

            return "";
        }
    }
}