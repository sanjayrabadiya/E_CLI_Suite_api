using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyTemplateValueAuditRepository : GenericRespository<PharmacyTemplateValueAudit, GscContext>,
        IPharmacyTemplateValueAuditRepository
    {
        public PharmacyTemplateValueAuditRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public IList<PharmacyAuditDto> GetAudits(int pharmacyTemplateValueId)
        {
            var auditDtos =
                (from audit in Context.PharmacyTemplateValueAudit.Where(t =>
                        t.PharmacyTemplateValueId == pharmacyTemplateValueId)
                    join reasonTemp in Context.AuditReason on audit.ReasonId equals reasonTemp.Id into reasonDt
                    from reason in reasonDt.DefaultIfEmpty()
                    join userTemp in Context.Users on audit.UserId equals userTemp.Id into userDto
                    from user in userDto.DefaultIfEmpty()
                    join roleTemp in Context.SecurityRole on audit.UserRoleId equals roleTemp.Id into roleDto
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