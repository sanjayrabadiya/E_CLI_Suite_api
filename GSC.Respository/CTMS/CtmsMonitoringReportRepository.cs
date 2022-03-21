using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportRepository : GenericRespository<CtmsMonitoringReport>, ICtmsMonitoringReportRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ICtmsMonitoringReportReviewRepository _ctmsMonitoringReportReviewRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;

        public CtmsMonitoringReportRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
        }

        public CtmsMonitoringReportFormDto GetCtmsMonitoringReportVariableValue(CtmsMonitoringReportFormDto designTemplateDto, int CtmsMonitoringReportId)
        {
            var ctmsMonitoringReport = All.Where(x => x.Id == CtmsMonitoringReportId && x.DeletedDate == null).FirstOrDefault();

            var ctmsMonitoringReportFormBasic = GetFormBasic(CtmsMonitoringReportId);

            designTemplateDto.VariableTemplateId = designTemplateDto.VariableTemplateId;
            designTemplateDto.CtmsMonitoringReportId = CtmsMonitoringReportId;
            designTemplateDto.ProjectId = ctmsMonitoringReportFormBasic.ProjectId;
            designTemplateDto.IsSender = ctmsMonitoringReport.CreatedBy == _jwtTokenAccesser.UserId;
            designTemplateDto.ReportStatus = ctmsMonitoringReport.ReportStatus;
            designTemplateDto.VariableDisable = ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.SendForReview
                || ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.QueryGenerated
                || ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.FormApproved;

            var reviewPerson = _ctmsMonitoringReportReviewRepository.GetReview(CtmsMonitoringReportId);

            var values = _ctmsMonitoringReportVariableValueRepository.GetVariableValues(ctmsMonitoringReportFormBasic.Id);

            values.ForEach(t =>
            {
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.StudyLevelFormVariableId == t.StudyLevelFormVariableId);
                if (variable != null)
                {
                    variable.VariableValue = t.Value;
                    variable.VariableValueOld = t.Value;
                    variable.CtmsMonitoringReportVariableValueId = t.Id;
                    variable.HasComments = t.IsComment;
                    variable.IsReviewPerson = reviewPerson;
                    variable.QueryStatus = t.QueryStatus;
                    variable.IsNaValue = t.IsNa;
                    if (!string.IsNullOrWhiteSpace(variable.VariableValue) || variable.IsNaValue)
                        variable.IsValid = true;

                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.StudyLevelFormVariableValueId == val.Id);
                            if (childValue != null)
                            {
                                variable.IsValid = true;
                                val.VariableValue = childValue.Value;
                                val.VariableValueOld = childValue.Value;
                                val.CtmsMonitoringReportVariableValueChildId = childValue.Id;
                            }
                        });
                }
            });

            return designTemplateDto;
        }

        public CtmsMonitoringReportBasic GetFormBasic(int ManageMonitoringReportId)
        {
            return All.Include(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)
                .Where(r => r.Id == ManageMonitoringReportId).Select(
               c => new CtmsMonitoringReportBasic
               {
                   Id = c.Id,
                   StudyLevelFormId = c.CtmsMonitoring.StudyLevelFormId,
                   ProjectId = c.CtmsMonitoring.ProjectId,
                   VariableTemplateId = c.CtmsMonitoring.StudyLevelForm.VariableTemplateId,
               }).FirstOrDefault();
        }

        public string GetMonitoringFormApprovedOrNOt(int projectId, int siteId, int tabNumber)
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
            if (!(CtmsMonitoringReport.Count() != 0 && CtmsMonitoringReport.All(z => z.ReportStatus == MonitoringReportStatus.FormApproved)))
                return "Please Complete Previous Form.";

            return "";
        }
    }
}