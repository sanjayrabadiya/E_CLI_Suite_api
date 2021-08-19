using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateValueAuditRepository : GenericRespository<VerificationApprovalTemplateValueAudit>, IVerificationApprovalTemplateValueAuditRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public VerificationApprovalTemplateValueAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
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
                CollectionSource = r.VerificationApprovalTemplateValue.Variable.CollectionSource,
                Id = r.Id
            }).OrderByDescending(t => t.Id).ToList();
        }
    }
}
