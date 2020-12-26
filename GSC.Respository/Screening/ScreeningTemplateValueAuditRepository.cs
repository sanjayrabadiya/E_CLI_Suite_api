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

            return All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValueId).Select(r => new ScreeningAuditDto
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
                Id = r.Id
            }).OrderByDescending(t => t.Id).ToList();

        }

        public IList<ScreeningAuditDto> GetAuditHistoryByScreeningEntry(int id)
        {
            return All.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntryId == id).Select(r => new ScreeningAuditDto
            {
                CreatedDate = r.CreatedDate,
                IpAddress = r.IpAddress,
                NewValue = r.Value,
                Note = r.Note,
                OldValue = !string.IsNullOrEmpty(r.Value) && string.IsNullOrEmpty(r.OldValue)
                                     ? "Default"
                                     : r.OldValue,
                Reason = r.AuditReason.ReasonName,
                Role = r.UserRole,
                Template = r.ScreeningTemplateValue.ScreeningTemplate.ProjectDesignTemplate.TemplateName,
                TimeZone = r.TimeZone,
                User = r.UserName,
                Variable = r.ScreeningTemplateValue.ProjectDesignVariable.VariableName,
                Visit = r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ProjectDesignVisit.DisplayName +
                Convert.ToString(r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber == null ? "" : "_" + r.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.RepeatedVisitNumber)
            }).OrderByDescending(t => t.CreatedDate).ToList();
        }

        public void Save(ScreeningTemplateValueAudit audit)
        {
            audit.IpAddress = _jwtTokenAccesser.IpAddress;
            audit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            audit.UserName = _jwtTokenAccesser.UserName;
            audit.UserRole = _jwtTokenAccesser.RoleName;

            var clientDate = _jwtTokenAccesser.GetHeader("clientDateTime");
            DateTime createdDate;
            var isSucess = DateTime.TryParse(clientDate, out createdDate);
            if (!isSucess) createdDate = System.DateTime.Now;
            audit.CreatedDate = createdDate;

            Add(audit);
        }
    }
}