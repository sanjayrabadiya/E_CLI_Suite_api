using System.Collections.Generic;
using System.Linq;
using GSC.Common.Common;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Audit
{
    public class AuditTrailCommonRepository : GenericRespository<AuditTrailCommon>, IAuditTrailCommonRepository
    {
        private readonly IGSCContext _context;
        public AuditTrailCommonRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _context = context;
        }

        public IList<AuditTrailCommonDto> Search(AuditTrailCommonDto search)
        {
            if (search.TableName == nameof(_context.ProjectDesign))
                return SearchProjectDesign(search);

            var query = All.AsQueryable();

            if (search.TableName?.Length > 0)
                query = query.Where(x => x.TableName == search.TableName);
            if (search.RecordId > 0)
                query = query.Where(x => x.RecordId == search.RecordId);
            if (!string.IsNullOrEmpty(search.ColumnName))
                query = query.Where(x =>
                    x.ColumnName != null && x.ColumnName.ToLower().Contains(search.ColumnName.ToLower()));
            if (!string.IsNullOrEmpty(search.OldValue))
                query = query.Where(x =>
                    x.OldValue != null && x.OldValue.ToLower().Contains(search.OldValue.ToLower()));
            if (!string.IsNullOrEmpty(search.NewValue))
                query = query.Where(x =>
                    x.NewValue != null && x.NewValue.ToLower().Contains(search.NewValue.ToLower()));


            if (!string.IsNullOrEmpty(search.ReasonName))
            {
                query = query.Where(x => x.Reason == search.ReasonName);
            }

            if (search.UserId > 0)
                query = query.Where(x => x.UserId == search.UserId);

            if (!string.IsNullOrEmpty(search.UserRoleName))
                query = query.Where(x => x.UserRole == search.UserRoleName);

            var result = GetItems(query);

            return result;
        }

        private IList<AuditTrailCommonDto> SearchProjectDesign(AuditTrailCommonDto search)
        {
            var query = All.AsQueryable();
            var designIds = new List<int>();
            if (search.RecordId > 0)
                designIds.Add(search.RecordId);
            else
                designIds = _context.ProjectDesign.Select(s => s.Id).ToList();

            var periodIds = _context.ProjectDesignPeriod.Where(t => designIds.Contains(t.ProjectDesignId))
                .Select(s => s.Id).ToList();
            var visitIds = _context.ProjectDesignVisit.Where(t => periodIds.Contains(t.ProjectDesignPeriodId))
                .Select(s => s.Id).ToList();
            var templateIds = _context.ProjectDesignTemplate.Where(t => visitIds.Contains(t.ProjectDesignVisitId))
                .Select(s => s.Id).ToList();
            var variableIds = _context.ProjectDesignVariable.Where(t => templateIds.Contains(t.ProjectDesignTemplateId))
                .Select(s => s.Id).ToList();
            var variableValueIds = _context.ProjectDesignVariableValue
                .Where(t => variableIds.Contains(t.ProjectDesignVariableId)).Select(s => s.Id).ToList();

            var skipColNames = new List<string>
            {
                "ProjectDesignId", "ProjectDesignPeriodId", "ProjectDesignVisitId", "ProjectDesignTemplateId",
                "ProjectDesignVariableId"
            };

            query = query.Where(x => !skipColNames.Contains(x.ColumnName) &&
                                     (x.TableName == nameof(_context.ProjectDesign) && designIds.Contains(x.RecordId)
                                      || x.TableName == nameof(_context.ProjectDesignPeriod) &&
                                      periodIds.Contains(x.RecordId)
                                      || x.TableName == nameof(_context.ProjectDesignVisit) &&
                                      visitIds.Contains(x.RecordId)
                                      || x.TableName == nameof(_context.ProjectDesignTemplate) &&
                                      templateIds.Contains(x.RecordId)
                                      || x.TableName == nameof(_context.ProjectDesignVariable) &&
                                      variableIds.Contains(x.RecordId)
                                      || x.TableName == nameof(_context.ProjectDesignVariableValue) &&
                                      variableValueIds.Contains(x.RecordId)
                                     ));

            var result = GetItems(query);

            return result;
        }

        private IList<AuditTrailCommonDto> GetItems(IQueryable<AuditTrailCommon> query)
        {
            return query.Select(x => new AuditTrailCommonDto
            {
                Id = x.Id,
                TableName = x.TableName,
                RecordId = x.RecordId,
                Action = x.Action,
                ColumnName = x.ColumnName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ReasonOth = x.ReasonOth,
                UserId = x.UserId,
                CreatedDate = x.CreatedDate,
                ReasonName = x.Reason,
                UserName = x.User.UserName,
                UserRoleName = x.UserRole,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone
            }).OrderByDescending(x => x.Id).ToList();
        }
    }
}