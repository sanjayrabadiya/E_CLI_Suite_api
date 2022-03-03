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


    }
}