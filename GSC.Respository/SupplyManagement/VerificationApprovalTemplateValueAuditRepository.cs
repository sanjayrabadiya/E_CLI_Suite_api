using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateValueAuditRepository : GenericRespository<VerificationApprovalTemplateValueAudit>, IVerificationApprovalTemplateValueAuditRepository
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public VerificationApprovalTemplateValueAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {          
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<VerificationApprovalAuditDto> GetAudits(int VerificationApprovalTemplateValueId)
        {

            return All.Where(x => x.VerificationApprovalTemplateValueId == VerificationApprovalTemplateValueId).Select(r => new VerificationApprovalAuditDto
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
                CollectionSource = r.VerificationApprovalTemplateValue.StudyLevelFormVariable.CollectionSource,
                Id = r.Id
            }).OrderByDescending(t => t.Id).ToList();
        }

        public void Save(VerificationApprovalTemplateValueAudit audit)
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
