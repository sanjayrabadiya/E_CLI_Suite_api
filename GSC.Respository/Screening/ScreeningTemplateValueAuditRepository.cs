using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueAuditRepository : GenericRespository<ScreeningTemplateValueAudit>,
        IScreeningTemplateValueAuditRepository
    {
        private static List<string> _months = new List<string>
            {"UNK", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ScreeningTemplateValueAuditRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public IList<ScreeningAuditDto> GetAudits(int screeningTemplateValueId)
        {
            var auditDtos =
                (from audit in _context.ScreeningTemplateValueAudit.Where(t =>
                        t.ScreeningTemplateValueId == screeningTemplateValueId)
                    join reasonTemp in _context.AuditReason on audit.ReasonId equals reasonTemp.Id into reasonDt
                    from reason in reasonDt.DefaultIfEmpty()
                    join userTemp in _context.Users on audit.UserId equals userTemp.Id into userDto
                    from user in userDto.DefaultIfEmpty()
                    join roleTemp in _context.SecurityRole on audit.UserRoleId equals roleTemp.Id into roleDto
                    from role in roleDto.DefaultIfEmpty()
                    select new ScreeningAuditDto
                    {
                        CreatedDate = audit.CreatedDate,
                        IpAddress = audit.IpAddress,
                        NewValue = audit.Value,
                        Note = audit.Note,
                        OldValue = !string.IsNullOrEmpty(audit.Value) && string.IsNullOrEmpty(audit.OldValue)
                            ? "Default"
                            : audit.OldValue,
                        Reason = reason.ReasonName,
                        ReasonOth = audit.ReasonOth,
                        Role = role.RoleName,
                        TimeZone = audit.TimeZone,
                        User = user.UserName
                    }).OrderByDescending(t => t.CreatedDate).ToList();

            //var projectDesignVariableId = _context.ScreeningTemplateValue.First(t => t.Id == screeningTemplateValueId).ProjectDesignVariableId;
            //var collectionSource = _context.ProjectDesignVariable.First(t => t.Id == projectDesignVariableId).CollectionSource;
            //if (collectionSource == CollectionSources.PartialDate)
            //{
            //    auditDtos.ForEach(data =>
            //    {

            //        data.OldValue = GetPartialDateDisplayText(data.OldValue);
            //        data.NewValue = GetPartialDateDisplayText(data.NewValue);

            //    });
            //}

            return auditDtos;
        }

        private string GetPartialDateDisplayText(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.ToLower() == "default") return value;

            var values = value.Split("-");
            var year = Convert.ToInt32(values[0]);
            var month = Convert.ToInt32(values[1]);
            var day = Convert.ToInt32(values[2]);

            return "";
        }


        public void Save(ScreeningTemplateValueAudit audit)
        {
            audit.IpAddress = _jwtTokenAccesser.IpAddress;
            audit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            audit.UserId = _jwtTokenAccesser.UserId;
            audit.UserRoleId = _jwtTokenAccesser.RoleId;

            Add(audit);
        }
    }
}