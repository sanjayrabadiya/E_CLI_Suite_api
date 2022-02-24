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
    public class CtmsMonitoringRepository : GenericRespository<CtmsMonitoring>, ICtmsMonitoringRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CtmsMonitoringRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<CtmsMonitoringGridDto> GetMonitoringForm(int projectId, int siteId, int activityId)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity).Include(x => x.VariableTemplate)
                                 .Include(x => x.Activity).ThenInclude(x => x.CtmsActivity)
                                 .Where(x => x.ProjectId == projectId && x.ActivityId == activityId
                                 && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var ctmsMonitorings = All.Where(x => x.DeletedDate == null && StudyLevelForm.Select(y => y.Id).Contains(x.StudyLevelFormId) && x.ProjectId == siteId)
                .Select(x => new CtmsMonitoringGridDto
                {
                    Id = x.Id,
                    ProjectName = x.Project.ProjectCode,
                    StudyLevelFormId = x.StudyLevelFormId,
                    ActivityName = x.StudyLevelForm.Activity.CtmsActivity.ActivityName,
                    VariableTemplateName = x.StudyLevelForm.VariableTemplate.TemplateName,
                    ScheduleStartDate = x.ScheduleStartDate,
                    ScheduleEndDate = x.ScheduleEndDate,
                    ActualStartDate = x.ActualStartDate,
                    ActualEndDate = x.ActualEndDate,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    DeletedDate = x.DeletedDate,
                    CreatedByUser = x.CreatedByUser.UserName,
                    ModifiedByUser = x.ModifiedByUser.UserName,
                    DeletedByUser = x.DeletedByUser.UserName
                }).ToList();

            var result = StudyLevelForm.Select(x => new CtmsMonitoringGridDto
            {
                Id = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault() != null ? ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault().Id : 0,
                StudyLevelFormId = x.Id,
                ActivityName = x.Activity.CtmsActivity.ActivityName,
                VariableTemplateName = x.VariableTemplate.TemplateName,
                ScheduleStartDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ScheduleStartDate,
                ScheduleEndDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ScheduleEndDate,
                ActualStartDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ActualStartDate,
                ActualEndDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ActualEndDate,
                CreatedDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.CreatedDate,
                ModifiedDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ModifiedDate,
                DeletedDate = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.DeletedDate,
                CreatedByUser = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.CreatedByUser,
                ModifiedByUser = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ModifiedByUser,
                DeletedByUser = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.DeletedByUser,
            }).ToList();

            result.ForEach(x =>
            {
                var CtmsMonitoringReport = _context.CtmsMonitoringReport.Where(y => y.DeletedDate == null && y.CtmsMonitoringId == x.Id);
                if (CtmsMonitoringReport?.FirstOrDefault() != null)
                    x.CtmsMonitoringReportId = CtmsMonitoringReport.FirstOrDefault().Id;
            });

            return result;
        }
    }
}