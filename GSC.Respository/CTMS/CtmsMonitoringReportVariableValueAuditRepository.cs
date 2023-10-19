using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportVariableValueAuditRepository : GenericRespository<CtmsMonitoringReportVariableValueAudit>, ICtmsMonitoringReportVariableValueAuditRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public CtmsMonitoringReportVariableValueAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<CtmsMonitoringReportVariableValueAuditDto> GetAudits(int CtmsMonitoringReportVariableValueId)
        {

            return All.Where(x => x.CtmsMonitoringReportVariableValueId == CtmsMonitoringReportVariableValueId)
                .Select(r => new CtmsMonitoringReportVariableValueAuditDto
                {
                    CreatedDate = r.CreatedDate,
                    IpAddress = r.IpAddress,
                    NewValue = r.Value,
                    Note = r.Note,
                    OldValue = !string.IsNullOrEmpty(r.Value) && string.IsNullOrEmpty(r.OldValue)
                            ? "Default"
                            : r.OldValue,
                    Reason = r.AuditReason.ReasonName,
                    ReasonOth = r.ReasonOth,
                    Role = r.UserRole,
                    TimeZone = r.TimeZone,
                    User = r.UserName,
                    CollectionSource = r.CtmsMonitoringReportVariableValue.StudyLevelFormVariable.CollectionSource,
                    Id = r.Id
                }).OrderByDescending(t => t.Id).ToList();
        }

        public void Save(CtmsMonitoringReportVariableValueAudit audit)
        {
            audit.IpAddress = _jwtTokenAccesser.IpAddress;
            audit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            audit.UserName = _jwtTokenAccesser.UserName;
            audit.UserRole = _jwtTokenAccesser.RoleName;
            audit.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(audit);
        }
    }
}
