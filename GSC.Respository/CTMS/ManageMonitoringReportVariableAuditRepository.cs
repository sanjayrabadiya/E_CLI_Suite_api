using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class ManageMonitoringReportVariableAuditRepository : GenericRespository<ManageMonitoringReportVariableAudit>, IManageMonitoringReportVariableAuditRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ManageMonitoringReportVariableAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<ManageMonitoringReportVariableAuditDto> GetAudits(int ManageMonitoringReportVariableId)
        {

            return All.Where(x => x.ManageMonitoringReportVariableId == ManageMonitoringReportVariableId)
                .Select(r => new ManageMonitoringReportVariableAuditDto
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
                    CollectionSource = r.ManageMonitoringReportVariable.Variable.CollectionSource,
                    Id = r.Id
                }).OrderByDescending(t => t.Id).ToList();
        }

        public void Save(ManageMonitoringReportVariableAudit audit)
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
