using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueAuditRepository : GenericRespository<ScreeningTemplateValueAudit, GscContext>,
        IScreeningTemplateValueAuditRepository
    {
        private static List<string> _months = new List<string>
            {"UNK", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

        public ScreeningTemplateValueAuditRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public IList<ScreeningAuditDto> GetAudits(int screeningTemplateValueId)
        {
            var auditDtos =
                (from audit in Context.ScreeningTemplateValueAudit.Where(t =>
                        t.ScreeningTemplateValueId == screeningTemplateValueId)
                    join reasonTemp in Context.AuditReason on audit.ReasonId equals reasonTemp.Id into reasonDt
                    from reason in reasonDt.DefaultIfEmpty()
                    join userTemp in Context.Users on audit.UserId equals userTemp.Id into userDto
                    from user in userDto.DefaultIfEmpty()
                    join roleTemp in Context.SecurityRole on audit.UserRoleId equals roleTemp.Id into roleDto
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
                        Role = role.RoleName,
                        TimeZone = audit.TimeZone,
                        User = user.UserName
                    }).OrderByDescending(t => t.CreatedDate).ToList();

            //var projectDesignVariableId = Context.ScreeningTemplateValue.First(t => t.Id == screeningTemplateValueId).ProjectDesignVariableId;
            //var collectionSource = Context.ProjectDesignVariable.First(t => t.Id == projectDesignVariableId).CollectionSource;
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
    }
}