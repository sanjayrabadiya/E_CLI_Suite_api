using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Audit
{
    public class AuditTrailCommonRepository : GenericRespository<AuditTrailCommon, GscContext>,
        IAuditTrailCommonRepository
    {
        public AuditTrailCommonRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public IList<AuditTrailCommonDto> Search(AuditTrailCommonDto search)
        {
            if (search.TableName == nameof(GscContext.ProjectDesign))
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

        private IList<AuditTrailCommonDto> SearchProjectDesign(AuditTrailCommonDto search)
        {
            var query = All.AsQueryable();
            var designIds = new List<int>();
            if (search.RecordId > 0)
                designIds.Add(search.RecordId);
            else
                designIds = Context.ProjectDesign.Select(s => s.Id).ToList();

            var periodIds = Context.ProjectDesignPeriod.Where(t => designIds.Contains(t.ProjectDesignId))
                .Select(s => s.Id).ToList();
            var visitIds = Context.ProjectDesignVisit.Where(t => periodIds.Contains(t.ProjectDesignPeriodId))
                .Select(s => s.Id).ToList();
            var templateIds = Context.ProjectDesignTemplate.Where(t => visitIds.Contains(t.ProjectDesignVisitId))
                .Select(s => s.Id).ToList();
            var variableIds = Context.ProjectDesignVariable.Where(t => templateIds.Contains(t.ProjectDesignTemplateId))
                .Select(s => s.Id).ToList();
            var variableValueIds = Context.ProjectDesignVariableValue
                .Where(t => variableIds.Contains(t.ProjectDesignVariableId)).Select(s => s.Id).ToList();

            var skipColNames = new List<string>
            {
                "ProjectDesignId", "ProjectDesignPeriodId", "ProjectDesignVisitId", "ProjectDesignTemplateId",
                "ProjectDesignVariableId"
            };

            query = query.Where(x => !skipColNames.Contains(x.ColumnName) &&
                                     (x.TableName == nameof(GscContext.ProjectDesign) && designIds.Contains(x.RecordId)
                                      || x.TableName == nameof(GscContext.ProjectDesignPeriod) &&
                                      periodIds.Contains(x.RecordId)
                                      || x.TableName == nameof(GscContext.ProjectDesignVisit) &&
                                      visitIds.Contains(x.RecordId)
                                      || x.TableName == nameof(GscContext.ProjectDesignTemplate) &&
                                      templateIds.Contains(x.RecordId)
                                      || x.TableName == nameof(GscContext.ProjectDesignVariable) &&
                                      variableIds.Contains(x.RecordId)
                                      || x.TableName == nameof(GscContext.ProjectDesignVariableValue) &&
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
                ReasonId = x.ReasonId,
                ReasonOth = x.ReasonOth,
                UserId = x.UserId,
                UserRoleId = x.UserRoleId,
                CreatedDate = x.CreatedDate,
                ReasonName = x.Reason.ReasonName,
                UserName = x.User.UserName,
                UserRoleName = Context.SecurityRole.First(t => t.Id == x.UserRoleId).RoleName,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone
            }).OrderByDescending(x => x.Id).ToList();
        }
    }
}