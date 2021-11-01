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
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringReportVariableRepository : GenericRespository<ManageMonitoringReportVariable>, IManageMonitoringReportVariableRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ManageMonitoringReportVariableRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public MonitoringReportTemplateDto GetReportTemplateVariable(MonitoringReportTemplateDto designTemplateDto, int ManageMonitoringReportId)
        {
            int reportTemplateId = _context.ManageMonitoringReport.Where(x => x.Id == ManageMonitoringReportId && x.DeletedDate == null).FirstOrDefault().VariableTemplateId;

            var ManageMonitoringTemplateBasic = GetScreeningTemplateBasic(ManageMonitoringReportId);

            designTemplateDto.VariableTemplateId = reportTemplateId;
            designTemplateDto.ManageMonitoringReportId = ManageMonitoringReportId;
            designTemplateDto.ProjectId = ManageMonitoringTemplateBasic.ProjectId;

            var values = GetVariableValues(ManageMonitoringTemplateBasic.Id);

            values.ForEach(t =>
            {
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.VariableId == t.VariableId);
                if (variable != null)
                {
                    variable.VariableValue = t.Value;
                    variable.VariableValueOld = t.IsNa ? "N/A" : t.Value;
                    variable.ManageMonitoringReportVariableId = t.Id;

                    variable.IsNaValue = t.IsNa;

                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.VariableValueId == val.Id);
                            if (childValue != null)
                            {
                                val.VariableValue = childValue.Value;
                                val.VariableValueOld = childValue.Value;
                                val.ManageMonitoringReportVariableChildId = childValue.Id;
                            }
                        });
                }
            });

            return designTemplateDto;
        }

        private ManageMonitoringTemplateBasic GetScreeningTemplateBasic(int ManageMonitoringReportId)
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

        private List<ManageMonitoringValueBasic> GetVariableValues(int ManageMonitoringReportId)
        {
            return All.Include(x => x.ManageMonitoringReport).AsNoTracking().Where(t => t.ManageMonitoringReportId == ManageMonitoringReportId)
                    .ProjectTo<ManageMonitoringValueBasic>(_mapper.ConfigurationProvider).ToList();
        }

        public string GetValueForAudit(ManageMonitoringReportVariableDto manageMonitoringReportVariableDto)
        {
            if (manageMonitoringReportVariableDto.IsDeleted) return null;

            if (manageMonitoringReportVariableDto.Children?.Count > 0)
            {
                var child = manageMonitoringReportVariableDto.Children.First();

                var variableValue = _context.VariableValue.Find(child.VariableValueId);
                if (variableValue != null)
                {
                    var valueChild = _context.VerificationApprovalTemplateValueChild.AsNoTracking()
                        .FirstOrDefault(t => t.Id == child.Id);
                    if (valueChild != null && child.Value == "false")
                    {
                        manageMonitoringReportVariableDto.OldValue = variableValue.ValueName;
                        return "";
                    }

                    manageMonitoringReportVariableDto.OldValue = "";
                    return variableValue.ValueName;
                }

                return child.Value;
            }

            if (manageMonitoringReportVariableDto.IsNa)
                return "N/A";

            return string.IsNullOrWhiteSpace(manageMonitoringReportVariableDto.ValueName)
                ? manageMonitoringReportVariableDto.Value
                : manageMonitoringReportVariableDto.ValueName;
        }

        public void DeleteChild(int manageMonitoringReportVariableId)
        {
            var childs = _context.ManageMonitoringReportVariableChild
                .Where(t => t.ManageMonitoringReportVariableId == manageMonitoringReportVariableId).ToList();
            _context.ManageMonitoringReportVariableChild.RemoveRange(childs);
        }
    }
}