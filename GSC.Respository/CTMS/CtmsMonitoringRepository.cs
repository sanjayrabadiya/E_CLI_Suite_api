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
                    DeletedByUser = x.DeletedByUser.UserName,
                    ParentId = x.ParentId
                }).ToList();

            var StudyLevelFormList = StudyLevelForm.Select(x => new CtmsMonitoringGridDto
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
                ParentId = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ParentId
            }).ToList();

            var result = ctmsMonitorings.Count() == 0 ? StudyLevelFormList : ctmsMonitorings;

            result.ForEach(x =>
            {
                var CtmsMonitoringReport = _context.CtmsMonitoringReport.Where(y => y.DeletedDate == null && y.CtmsMonitoringId == x.Id);
                if (CtmsMonitoringReport?.FirstOrDefault() != null)
                {
                    x.CtmsMonitoringReportId = CtmsMonitoringReport.FirstOrDefault().Id;
                    x.ReportStatus = CtmsMonitoringReport.FirstOrDefault().ReportStatus.GetDescription();
                    x.ReportStatusId = (int)CtmsMonitoringReport.FirstOrDefault().ReportStatus;
                    x.IsSender = CtmsMonitoringReport.FirstOrDefault().CreatedBy == _jwtTokenAccesser.UserId;
                    x.IsReviewerApprovedForm = CtmsMonitoringReport.Count() != 0 && CtmsMonitoringReport.All(z => z.ReportStatus == MonitoringReportStatus.Approved);//Changes made by Sachin
                }
            });

            return result;
        }

        // If Study Level Form already use than not delete and Edit
        public string StudyLevelFormAlreadyUse(int StudyLevelFormId)
        {
            if (All.Any(x => x.StudyLevelFormId == StudyLevelFormId && x.DeletedDate == null))
                return "Study Level Form is in use. Cannot edit or delete!";
            return "";
        }

        public CtmsMonitoringGridDto GetMonitoringFormforDashboard(int ctmsMonitoringId, int activityId)
        {
            var monitoring = All.Include(x => x.Project).Where(x => x.Id == ctmsMonitoringId).FirstOrDefault();
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();
            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity).Include(x => x.VariableTemplate)
                                  .Include(x => x.Activity).ThenInclude(x => x.CtmsActivity)
                                  .Where(x => x.ProjectId == monitoring.Project.ParentProjectId && x.ActivityId == activityId
                                  && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            var ctmsMonitorings = All.Where(x => x.DeletedDate == null && x.Id == ctmsMonitoringId)
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
                    DeletedByUser = x.DeletedByUser.UserName,
                    ParentId = x.ParentId,
                    ScreenCode = x.StudyLevelForm.Activity.CtmsActivity.ActivityCode == "act_001" ? "mnu_feasibility" :
                                 x.StudyLevelForm.Activity.CtmsActivity.ActivityCode == "act_002" ? "mnu_siteselection" :
                                 x.StudyLevelForm.Activity.CtmsActivity.ActivityCode == "act_003" ? "mnu_siteinitiate" :
                                 x.StudyLevelForm.Activity.CtmsActivity.ActivityCode == "act_004" ? "mnu_monitoringvisit" :
                                 x.StudyLevelForm.Activity.CtmsActivity.ActivityCode == "act_005" ? "mnu_closeout" : ""
                }).ToList();

            var StudyLevelFormList = StudyLevelForm.Select(x => new CtmsMonitoringGridDto
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
                ParentId = ctmsMonitorings.Where(y => y.StudyLevelFormId == x.Id).FirstOrDefault()?.ParentId,
                ScreenCode = x.Activity.CtmsActivity.ActivityCode == "act_001" ? "mnu_feasibility" :
                                 x.Activity.CtmsActivity.ActivityCode == "act_002" ? "mnu_siteselection" :
                                 x.Activity.CtmsActivity.ActivityCode == "act_003" ? "mnu_siteinitiate" :
                                 x.Activity.CtmsActivity.ActivityCode == "act_004" ? "mnu_monitoringvisit" : 
                                 x.Activity.CtmsActivity.ActivityCode == "act_005" ? "mnu_monitoringvisit" : ""
            }).ToList();

            var result = ctmsMonitorings.Count() == 0 ? StudyLevelFormList : ctmsMonitorings;

            result.ForEach(x =>
            {
                var CtmsMonitoringReport = _context.CtmsMonitoringReport.Where(y => y.DeletedDate == null && y.CtmsMonitoringId == x.Id);
                if (CtmsMonitoringReport?.FirstOrDefault() != null)
                {
                    x.CtmsMonitoringReportId = CtmsMonitoringReport.FirstOrDefault().Id;
                    x.ReportStatus = CtmsMonitoringReport.FirstOrDefault().ReportStatus.GetDescription();
                    x.ReportStatusId = (int)CtmsMonitoringReport.FirstOrDefault().ReportStatus;
                    x.IsSender = CtmsMonitoringReport.FirstOrDefault().CreatedBy == _jwtTokenAccesser.UserId;
                    //Changes made by Sachin
                    x.IsReviewerApprovedForm = CtmsMonitoringReport.Count() != 0 && CtmsMonitoringReport.All(z => z.ReportStatus == MonitoringReportStatus.Approved);
                }
            });

            return result.FirstOrDefault();
        }
    }
}