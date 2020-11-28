using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyTemplateValueAuditRepository : GenericRespository<PharmacyTemplateValueAudit>,
        IPharmacyTemplateValueAuditRepository
    {
        private readonly IGSCContext _context;
        public PharmacyTemplateValueAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
        }

        public IList<PharmacyAuditDto> GetAudits(int pharmacyTemplateValueId)
        {
            var auditDtos =
                (from audit in _context.PharmacyTemplateValueAudit.Where(t =>
                        t.PharmacyTemplateValueId == pharmacyTemplateValueId)
                    join reasonTemp in _context.AuditReason on audit.ReasonId equals reasonTemp.Id into reasonDt
                    from reason in reasonDt.DefaultIfEmpty()
                    join userTemp in _context.Users on audit.UserId equals userTemp.Id into userDto
                    from user in userDto.DefaultIfEmpty()
                    join roleTemp in _context.SecurityRole on audit.UserRoleId equals roleTemp.Id into roleDto
                    from role in roleDto.DefaultIfEmpty()
                    select new PharmacyAuditDto
                    {
                        CreatedDate = audit.CreatedDate,
                        IpAddress = audit.IpAddress,
                        NewValue = audit.Value,
                        Note = audit.Note,
                        OldValue = string.IsNullOrEmpty(audit.OldValue) ? "Default" : audit.OldValue,
                        Reason = reason.ReasonName,
                        Role = role.RoleName,
                        TimeZone = audit.TimeZone,
                        User = user.UserName
                    }).OrderByDescending(t => t.CreatedDate).ToList();

            return auditDtos;
        }
    }
}