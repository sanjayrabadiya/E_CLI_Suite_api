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
    public class CtmsMonitoringReportVariableValueRepository : GenericRespository<CtmsMonitoringReportVariableValue>, ICtmsMonitoringReportVariableValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public CtmsMonitoringReportVariableValueRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public void UpdateChild(List<CtmsMonitoringReportVariableValueChild> children)
        {
            _context.CtmsMonitoringReportVariableValueChild.UpdateRange(children);
        }

        public List<CtmsMonitoringReportVariableValueBasic> GetVariableValues(int CtmsMonitoringReportId)
        {
            return All.Include(x => x.CtmsMonitoringReport).AsNoTracking().Where(t => t.CtmsMonitoringReportId == CtmsMonitoringReportId)
                    .ProjectTo<CtmsMonitoringReportVariableValueBasic>(_mapper.ConfigurationProvider).ToList();
        }

        public string GetValueForAudit(CtmsMonitoringReportVariableValueDto cstmsMonitoringReportVariableValueDto)
        {
            if (cstmsMonitoringReportVariableValueDto.IsDeleted) return null;

            if (cstmsMonitoringReportVariableValueDto.Children?.Count > 0)
            {
                var child = cstmsMonitoringReportVariableValueDto.Children.First();

                var variableValue = _context.StudyLevelFormVariableValue.Find(child.StudyLevelFormVariableValueId);
                if (variableValue != null)
                {
                    var valueChild = _context.CtmsMonitoringReportVariableValueChild.AsNoTracking()
                        .FirstOrDefault(t => t.Id == child.Id);
                    if (valueChild != null && child.Value == "false")
                    {
                        cstmsMonitoringReportVariableValueDto.OldValue = variableValue.ValueName;
                        return "";
                    }

                    cstmsMonitoringReportVariableValueDto.OldValue = "";
                    return variableValue.ValueName;
                }

                return child.Value;
            }

            if (cstmsMonitoringReportVariableValueDto.IsNa)
                return "N/A";

            return string.IsNullOrWhiteSpace(cstmsMonitoringReportVariableValueDto.ValueName)
                ? cstmsMonitoringReportVariableValueDto.Value
                : cstmsMonitoringReportVariableValueDto.ValueName;
        }

        public void DeleteChild(int ctmsMonitoringReportVariableValueId)
        {
            var childs = _context.CtmsMonitoringReportVariableValueChild
                .Where(t => t.CtmsMonitoringReportVariableValueId == ctmsMonitoringReportVariableValueId).ToList();
            _context.CtmsMonitoringReportVariableValueChild.RemoveRange(childs);
        }

        public bool GetQueryStatusByReportId(int ctmsMonitoringReportId)
        {
            var result = All.Where(x => x.DeletedDate == null
                    && x.CtmsMonitoringReportId == ctmsMonitoringReportId && x.QueryStatus != null
                    && x.QueryStatus != CtmsCommentStatus.Closed).Count();

            return result != 0;
        }
    }
}