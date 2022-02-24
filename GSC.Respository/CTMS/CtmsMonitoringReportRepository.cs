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

            var ManageMonitoringTemplateBasic = GetFormBasic(CtmsMonitoringReportId);

            designTemplateDto.VariableTemplateId = designTemplateDto.VariableTemplateId;
            designTemplateDto.CtmsMonitoringReportId = CtmsMonitoringReportId;
            designTemplateDto.ProjectId = ManageMonitoringTemplateBasic.ProjectId;
            designTemplateDto.IsSender = ctmsMonitoringReport.CreatedBy == _jwtTokenAccesser.UserId;
            designTemplateDto.ReportStatus = ctmsMonitoringReport.ReportStatus;

            var reviewPerson = _ctmsMonitoringReportReviewRepository.GetReview(CtmsMonitoringReportId);

            var values = _ctmsMonitoringReportVariableValueRepository.GetVariableValues(ManageMonitoringTemplateBasic.Id);

            values.ForEach(t =>
            {
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.VariableId == t.StudyLevelFormVariableId);
                if (variable != null)
                {
                    variable.VariableValue = t.Value;
                    variable.VariableValueOld = t.Value;
                    variable.CtmsMonitoringReportVariableValueId = t.Id;
                    variable.HasComments = t.IsComment;
                    variable.IsReviewPerson = reviewPerson;
                    variable.QueryStatus = t.QueryStatus;

                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.VariableValueId == val.Id);
                            if (childValue != null)
                            {
                                val.VariableValue = childValue.Value;
                                val.VariableValueOld = childValue.Value;
                                val.CtmsMonitoringReportVariableValueId = childValue.Id;
                            }
                        });
                }
            });

            return designTemplateDto;
        }

        private ManageMonitoringTemplateBasic GetFormBasic(int ManageMonitoringReportId)
        {
            return _context.ManageMonitoringReport.Where(r => r.Id == ManageMonitoringReportId).Select(
               c => new ManageMonitoringTemplateBasic
               {
                   Id = c.Id,
                   ManageMonitoringVisitId = c.ManageMonitoringVisitId,
                   ProjectId = c.ManageMonitoringVisit.ProjectId,
                   VariableTemplateId = c.VariableTemplateId,
               }).FirstOrDefault();
        }
    }
}