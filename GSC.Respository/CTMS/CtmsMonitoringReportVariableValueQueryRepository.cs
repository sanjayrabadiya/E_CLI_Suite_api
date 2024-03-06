using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportVariableValueQueryRepository : GenericRespository<CtmsMonitoringReportVariableValueQuery>, ICtmsMonitoringReportVariableValueQueryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly ICtmsMonitoringReportVariableValueAuditRepository _ctmsMonitoringReportVariableValueAuditRepository;
        private readonly ICtmsMonitoringReportVariableValueChildRepository _ctmsMonitoringReportVariableValueChildRepository;


        public CtmsMonitoringReportVariableValueQueryRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
           ICtmsMonitoringReportVariableValueAuditRepository ctmsMonitoringReportVariableValueAuditRepository,
           ICtmsMonitoringReportVariableValueChildRepository ctmsMonitoringReportVariableValueChildRepository
        )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _ctmsMonitoringReportVariableValueAuditRepository = ctmsMonitoringReportVariableValueAuditRepository;
            _ctmsMonitoringReportVariableValueChildRepository = ctmsMonitoringReportVariableValueChildRepository;
        }
        public IList<CtmsMonitoringReportVariableValueQueryDto> GetQueries(int ctmsMonitoringReportVariableValueId)
        {
            var comments = All.Where(x => x.CtmsMonitoringReportVariableValueId == ctmsMonitoringReportVariableValueId)
                .Select(t => new CtmsMonitoringReportVariableValueQueryDto
                {
                    Id = t.Id,
                    CreatedDate = t.CreatedDate,
                    ReasonName = t.Reason.ReasonName,
                    ReasonOth = t.ReasonOth,
                    StatusName = t.QueryStatus.GetDescription(),
                    Value = t.Value,
                    OldValue = t.OldValue,
                    CreatedByName = t.UserName + "(" + t.UserRole + ")",
                    Note = string.IsNullOrEmpty(t.Note) ? t.ReasonOth : t.Note,
                    CollectionSource = t.CtmsMonitoringReportVariableValue.StudyLevelFormVariable.CollectionSource
                }).OrderByDescending(x => x.Id).ToList();

            return comments;
        }

        public void UpdateQuery(CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto, CtmsMonitoringReportVariableValueQuery CtmsMonitoringReportVariableValueQuery, CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue)
        {
            var value = string.IsNullOrEmpty(ctmsMonitoringReportVariableValueQueryDto.ValueName)
                ? ctmsMonitoringReportVariableValueQueryDto.Value
                : ctmsMonitoringReportVariableValueQueryDto.ValueName;

            var updateQueryStatus = ctmsMonitoringReportVariableValueQueryDto.OldValue == CtmsMonitoringReportVariableValueQuery.Value
                ? CtmsCommentStatus.Answered
                : CtmsCommentStatus.Resolved;


            CtmsMonitoringReportVariableValueQuery.QueryStatus = updateQueryStatus;
            CtmsMonitoringReportVariableValueQuery.Value = value;
            CtmsMonitoringReportVariableValueQuery.OldValue = ctmsMonitoringReportVariableValueQueryDto.OldValue;

            ctmsMonitoringReportVariableValue.QueryStatus = updateQueryStatus;
            ctmsMonitoringReportVariableValue.Value = ctmsMonitoringReportVariableValueQueryDto.Value;

            QueryAudit(ctmsMonitoringReportVariableValueQueryDto, ctmsMonitoringReportVariableValue, updateQueryStatus.ToString(), CtmsMonitoringReportVariableValueQuery);

            Save(CtmsMonitoringReportVariableValueQuery);

            _ctmsMonitoringReportVariableValueChildRepository.Save(ctmsMonitoringReportVariableValue);

            _ctmsMonitoringReportVariableValueRepository.Update(ctmsMonitoringReportVariableValue);
        }

        private void QueryAudit(CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto,
            CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue, string status,
            CtmsMonitoringReportVariableValueQuery ctmsMonitoringReportVariableValueQuery)
        {
            var queryOldValue = "";
            var queryValue = "";
            if (ctmsMonitoringReportVariableValueQueryDto.Children?.Count > 0)
            {
                var oldProjectDesignVariableValueIds = _context.CtmsMonitoringReportVariableValueChild.AsNoTracking().Where(t =>
                        ctmsMonitoringReportVariableValueQueryDto.Children.Select(s => s.Id).Contains(t.Id) && t.Value == "true")
                    .Select(t => t.StudyLevelFormVariableValueId).ToList();

                queryOldValue = string.Join(", ",
                    _context.ProjectDesignVariableValue.Where(t => oldProjectDesignVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                var newVariableValueIds = ctmsMonitoringReportVariableValueQueryDto.Children
                    .Where(t => t.Value == "true").Select(t => t.StudyLevelFormVariableValueId).ToList();

                queryValue = string.Join(", ",
                    _context.VariableValue.Where(t => newVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                _ctmsMonitoringReportVariableValueRepository.UpdateChild(ctmsMonitoringReportVariableValueQueryDto.Children.ToList());
            }
            else
            {
                queryOldValue = ctmsMonitoringReportVariableValueQueryDto.OldValue;
                queryValue = ctmsMonitoringReportVariableValueQuery.QueryStatus == CtmsCommentStatus.Resolved ? ctmsMonitoringReportVariableValueQuery.Value : queryValue;
            }

            ctmsMonitoringReportVariableValueQuery.Value = queryValue;
            ctmsMonitoringReportVariableValueQuery.OldValue = queryOldValue;

            var audit = new CtmsMonitoringReportVariableValueAudit
            {
                CtmsMonitoringReportVariableValueId = ctmsMonitoringReportVariableValue.Id,
                OldValue = queryOldValue,
                Value = queryValue,
                Note = ctmsMonitoringReportVariableValueQueryDto.Note + " " + status,
                ReasonId = ctmsMonitoringReportVariableValueQueryDto.ReasonId,
                ReasonOth = ctmsMonitoringReportVariableValueQueryDto.ReasonOth
            };
            _ctmsMonitoringReportVariableValueAuditRepository.Save(audit);
        }

        public void GenerateQuery(CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto,
           CtmsMonitoringReportVariableValueQuery ctmsMonitoringReportVariableValueQuery, CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue)
        {
            var value = string.IsNullOrEmpty(ctmsMonitoringReportVariableValueQueryDto.ValueName)
                ? ctmsMonitoringReportVariableValueQueryDto.Value
                : ctmsMonitoringReportVariableValueQueryDto.ValueName;

            ctmsMonitoringReportVariableValue.QueryStatus = CtmsCommentStatus.Open;

            ctmsMonitoringReportVariableValueQuery.Value = value;
            ctmsMonitoringReportVariableValueQuery.QueryStatus = CtmsCommentStatus.Open;
            ctmsMonitoringReportVariableValueQuery.OldValue = ctmsMonitoringReportVariableValueQueryDto.OldValue;
            ctmsMonitoringReportVariableValueQuery.UserName = _jwtTokenAccesser.UserName;
            ctmsMonitoringReportVariableValueQuery.UserRole = _jwtTokenAccesser.RoleName;
            ctmsMonitoringReportVariableValueQuery.CreatedDate = _jwtTokenAccesser.GetClientDate();
            ctmsMonitoringReportVariableValueQuery.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

            _ctmsMonitoringReportVariableValueRepository.Update(ctmsMonitoringReportVariableValue);

            Add(ctmsMonitoringReportVariableValueQuery);
        }

        public void Save(CtmsMonitoringReportVariableValueQuery ctmsMonitoringReportVariableValueQuery)
        {
            ctmsMonitoringReportVariableValueQuery.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            ctmsMonitoringReportVariableValueQuery.UserName = _jwtTokenAccesser.UserName;
            ctmsMonitoringReportVariableValueQuery.UserRole = _jwtTokenAccesser.RoleName;
            ctmsMonitoringReportVariableValueQuery.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(ctmsMonitoringReportVariableValueQuery);
        }

        public void SaveCloseQuery(CtmsMonitoringReportVariableValueQuery ctmsMonitoringReportVariableValueQuery)
        {
            ctmsMonitoringReportVariableValueQuery.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            ctmsMonitoringReportVariableValueQuery.UserName = _jwtTokenAccesser.UserName;
            ctmsMonitoringReportVariableValueQuery.UserRole = _jwtTokenAccesser.RoleName;

            if (ctmsMonitoringReportVariableValueQuery.QueryStatus != CtmsCommentStatus.Open)
            {
                var lastQuery = All.Where(x => x.CtmsMonitoringReportVariableValueId == ctmsMonitoringReportVariableValueQuery.CtmsMonitoringReportVariableValueId).OrderByDescending(t => t.Id).FirstOrDefault();
                if (lastQuery != null)
                {
                    ctmsMonitoringReportVariableValueQuery.QueryParentId = lastQuery.Id;
                    ctmsMonitoringReportVariableValueQuery.PreviousQueryDate = lastQuery.CreatedDate;
                }
            }

            ctmsMonitoringReportVariableValueQuery.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(ctmsMonitoringReportVariableValueQuery);
        }
    }
}