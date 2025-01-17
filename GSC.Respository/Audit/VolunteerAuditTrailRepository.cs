using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Audit
{
    public class VolunteerAuditTrailRepository : GenericRespository<VolunteerAuditTrail>, IVolunteerAuditTrailRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public VolunteerAuditTrailRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void Save(AuditModule moduleId, AuditTable tableId, AuditAction action, int recordId,
            int? parentRecordId, List<VolunteerAuditTrail> changes)
        {
            if (action == AuditAction.Deleted)
            {
                int.TryParse(_jwtTokenAccesser.GetHeader("audit-reason-id"), out var reasonId);
                var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                changes = new List<VolunteerAuditTrail>
                {
                    new VolunteerAuditTrail
                    {
                        IsRecordDeleted = true,
                        ReasonId = reasonId > 0 ? reasonId : (int?) null,
                        ReasonOth = reasonOth,
                        IpAddress = _jwtTokenAccesser.IpAddress,
                        TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                    }
                };
            }
            else if (action == AuditAction.Activated)
            {
                int.TryParse(_jwtTokenAccesser.GetHeader("audit-reason-id"), out var reasonId);
                var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                changes = new List<VolunteerAuditTrail>
                {
                    new VolunteerAuditTrail
                    {
                        IsRecordDeleted = false,
                        ReasonId = reasonId > 0 ? reasonId : (int?) null,
                        ReasonOth = reasonOth,
                        IpAddress = _jwtTokenAccesser.IpAddress,
                        TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                    }
                };
            }

            if (!changes.Any()) return;
            changes.ForEach(t =>
            {
                t.ModuleId = moduleId;
                t.TableId = tableId;
                t.RecordId = recordId;
                t.ParentRecordId = parentRecordId;
                t.Action = action;
                t.UserId = _jwtTokenAccesser.UserId;
                t.UserRoleId = _jwtTokenAccesser.RoleId;
                t.CreatedDate = DateTime.Now;
                t.IpAddress = _jwtTokenAccesser.IpAddress;
                t.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                Add(t);
            });

            _context.Save();
        }

        public IList<VolunteerAuditTrailDto> Search(VolunteerAuditTrailDto search)
        {
            var query = All.AsQueryable();

            if (search.ModuleId.HasValue)
                query = query.Where(x => x.ModuleId == search.ModuleId);
            if (search.TableId.HasValue)
                query = query.Where(x => x.TableId == search.TableId);
            if (search.RecordId > 0)
                query = query.Where(x => x.RecordId == search.RecordId);
            if (search.ParentRecordId.HasValue)
                query = query.Where(x => x.ParentRecordId == search.ParentRecordId);
            if (search.Action.HasValue)
                query = query.Where(x => x.Action == search.Action);
            if (!string.IsNullOrEmpty(search.ColumnName))
                query = query.Where(x =>
                    x.ColumnName != null && x.ColumnName.ToLower().Contains(search.ColumnName.ToLower()));
            if (!string.IsNullOrEmpty(search.LabelName))
                query = query.Where(x =>
                    x.LabelName != null && x.LabelName.ToLower().Contains(search.LabelName.ToLower()));
            if (!string.IsNullOrEmpty(search.OldValue))
                query = query.Where(x =>
                    x.OldValue != null && x.OldValue.ToLower().Contains(search.OldValue.ToLower()));
            if (!string.IsNullOrEmpty(search.NewValue))
                query = query.Where(x =>
                    x.NewValue != null && x.NewValue.ToLower().Contains(search.NewValue.ToLower()));
            if (search.IsRecordDeleted.HasValue)
                query = query.Where(x => x.IsRecordDeleted == search.IsRecordDeleted);
            if (search.ReasonId.HasValue)
            {
                if (search.ReasonId == -1)
                    query = query.Where(x => x.ReasonId != null);
                else
                    query = query.Where(x => x.ReasonId == search.ReasonId);
            }

            if (search.UserId > 0)
                query = query.Where(x => x.UserId == search.UserId);
            if (search.UserRoleId > 0)
                query = query.Where(x => x.UserRoleId == search.UserRoleId);

            var result = GetItems(query);

            return result;
        }

        private IList<VolunteerAuditTrailDto> GetItems(IQueryable<VolunteerAuditTrail> query)
        {
            return query.Select(x => new VolunteerAuditTrailDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                TableId = x.TableId,
                RecordId = x.RecordId,
                ParentRecordId = x.ParentRecordId,
                Action = x.Action,
                ColumnName = x.ColumnName,
                LabelName = x.LabelName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                IsRecordDeleted = x.IsRecordDeleted,
                ReasonId = x.ReasonId,
                ReasonOth = x.ReasonOth,
                UserId = x.UserId,
                UserRoleId = x.UserRoleId,
                CreatedDate = x.CreatedDate,
                ModuleName = x.ModuleId.GetDescription(),
                TableName = x.TableId.GetDescription(),
                ActionName = x.Action == AuditAction.Deleted ? "In Activated" : x.Action.GetDescription(),
                ReasonName = x.Reason.ReasonName,
                UserName = x.User.UserName,
                UserRoleName = _context.SecurityRole.First(t => t.Id == x.UserRoleId).RoleName,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone,
                VolunteerName = _context.Volunteer
                    .Where(c => c.Id == x.RecordId && x.TableId == AuditTable.Volunteer || c.Id == x.ParentRecordId)
                    .Select(a => (string.IsNullOrWhiteSpace(a.VolunteerNo) ? a.VolunteerNo : a.VolunteerNo + " - " ) + a.FirstName + " " + a.MiddleName + " " + a.LastName).FirstOrDefault()
            }).OrderByDescending(x => x.Id).ToList();
        }
    }
}