using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringReportVariableCommentRepository : GenericRespository<ManageMonitoringReportVariableComment>, IManageMonitoringReportVariableCommentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IManageMonitoringReportVariableRepository _manageMonitoringReportVariableRepository;
        private readonly IManageMonitoringReportVariableAuditRepository _manageMonitoringReportVariableAuditRepository;
        private readonly IManageMonitoringReportVariableChildRepository _manageMonitoringReportVariableChildRepository;


        public ManageMonitoringReportVariableCommentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IManageMonitoringReportVariableRepository manageMonitoringReportVariableRepository,
           IManageMonitoringReportVariableAuditRepository manageMonitoringReportVariableAuditRepository,
           IManageMonitoringReportVariableChildRepository manageMonitoringReportVariableChildRepository
        )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _manageMonitoringReportVariableRepository = manageMonitoringReportVariableRepository;
            _manageMonitoringReportVariableRepository = manageMonitoringReportVariableRepository;
            _manageMonitoringReportVariableAuditRepository = manageMonitoringReportVariableAuditRepository;
            _manageMonitoringReportVariableChildRepository = manageMonitoringReportVariableChildRepository;
        }
        public IList<ManageMonitoringReportVariableCommentDto> GetComments(int manageMonitoringReportVariableId)
        {
            var comments = All.Where(x => x.ManageMonitoringReportVariableId == manageMonitoringReportVariableId)
                .Select(t => new ManageMonitoringReportVariableCommentDto
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
                    CollectionSource = t.ManageMonitoringReportVariable.Variable.CollectionSource
                }).OrderByDescending(x => x.Id).ToList();

            return comments;
        }

        public void UpdateQuery(ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto, ManageMonitoringReportVariableComment manageMonitoringReportVariableComment, ManageMonitoringReportVariable manageMonitoringReportVariable)
        {
            var value = string.IsNullOrEmpty(manageMonitoringReportVariableCommentDto.ValueName)
                ? manageMonitoringReportVariableCommentDto.Value
                : manageMonitoringReportVariableCommentDto.ValueName;

            var updateQueryStatus = manageMonitoringReportVariableCommentDto.Value == manageMonitoringReportVariableComment.Value
                ? CtmsCommentStatus.Answered
                : CtmsCommentStatus.Resolved;


            manageMonitoringReportVariableComment.QueryStatus = updateQueryStatus;
            manageMonitoringReportVariableComment.Value = value;
            manageMonitoringReportVariableComment.OldValue = manageMonitoringReportVariableCommentDto.OldValue;

            manageMonitoringReportVariable.QueryStatus = updateQueryStatus;
            manageMonitoringReportVariable.Value = manageMonitoringReportVariableCommentDto.Value;

            QueryAudit(manageMonitoringReportVariableCommentDto, manageMonitoringReportVariable, updateQueryStatus.ToString(), value, manageMonitoringReportVariableComment);

            Save(manageMonitoringReportVariableComment);

            _manageMonitoringReportVariableChildRepository.Save(manageMonitoringReportVariable);

            _manageMonitoringReportVariableRepository.Update(manageMonitoringReportVariable);
        }

        private void QueryAudit(ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto,
            ManageMonitoringReportVariable manageMonitoringReportVariable, string status, string value,
            ManageMonitoringReportVariableComment manageMonitoringReportVariableComment)
        {
            var queryOldValue = "";
            var queryValue = "";
            if (manageMonitoringReportVariableCommentDto.Children?.Count > 0)
            {
                var oldProjectDesignVariableValueIds = _context.ManageMonitoringReportVariableChild.AsNoTracking().Where(t =>
                        manageMonitoringReportVariableCommentDto.Children.Select(s => s.Id).Contains(t.Id) && t.Value == "true")
                    .Select(t => t.VariableValueId).ToList();

                queryOldValue = string.Join(", ",
                    _context.ProjectDesignVariableValue.Where(t => oldProjectDesignVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                var newVariableValueIds = manageMonitoringReportVariableCommentDto.Children
                    .Where(t => t.Value == "true").Select(t => t.VariableValueId).ToList();

                queryValue = string.Join(", ",
                    _context.VariableValue.Where(t => newVariableValueIds.Contains(t.Id))
                        .Select(t => t.ValueName).ToList());

                _manageMonitoringReportVariableRepository.UpdateChild(manageMonitoringReportVariableCommentDto.Children.ToList());
            }
            else
            {
                queryOldValue = manageMonitoringReportVariableCommentDto.OldValue;
                queryValue = manageMonitoringReportVariableCommentDto.IsNa ? "N/A" : value;
            }

            manageMonitoringReportVariableComment.Value = queryValue;
            manageMonitoringReportVariableComment.OldValue = queryOldValue;

            var audit = new ManageMonitoringReportVariableAudit
            {
                ManageMonitoringReportVariableId = manageMonitoringReportVariable.Id,
                OldValue = queryOldValue,
                Value = queryValue,
                Note = manageMonitoringReportVariableCommentDto.Note + " " + status,
                ReasonId = manageMonitoringReportVariableCommentDto.ReasonId,
                ReasonOth = manageMonitoringReportVariableCommentDto.ReasonOth
            };
            _manageMonitoringReportVariableAuditRepository.Save(audit);
        }

        public void GenerateQuery(ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto,
           ManageMonitoringReportVariableComment manageMonitoringReportVariableComment, ManageMonitoringReportVariable manageMonitoringReportVariables)
        {
            var value = string.IsNullOrEmpty(manageMonitoringReportVariableCommentDto.ValueName)
                ? manageMonitoringReportVariableCommentDto.Value
                : manageMonitoringReportVariableCommentDto.ValueName;

            manageMonitoringReportVariables.QueryStatus = CtmsCommentStatus.Open;

            manageMonitoringReportVariableComment.Value = value;
            manageMonitoringReportVariableComment.QueryStatus = CtmsCommentStatus.Open;
            manageMonitoringReportVariableComment.OldValue = manageMonitoringReportVariableCommentDto.OldValue;
            manageMonitoringReportVariableComment.UserName = _jwtTokenAccesser.UserName;
            manageMonitoringReportVariableComment.UserRole = _jwtTokenAccesser.RoleName;
            manageMonitoringReportVariableComment.CreatedDate = _jwtTokenAccesser.GetClientDate();
            manageMonitoringReportVariableComment.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

            _manageMonitoringReportVariableRepository.Update(manageMonitoringReportVariables);

            Add(manageMonitoringReportVariableComment);
        }

        public void Save(ManageMonitoringReportVariableComment manageMonitoringReportVariableComment)
        {
            manageMonitoringReportVariableComment.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            manageMonitoringReportVariableComment.UserName = _jwtTokenAccesser.UserName;
            //manageMonitoringReportVariableComment.RoleId = _jwtTokenAccesser.RoleId;
            manageMonitoringReportVariableComment.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(manageMonitoringReportVariableComment);
        }

        public void SaveCloseQuery(ManageMonitoringReportVariableComment manageMonitoringReportVariableComment)
        {
            manageMonitoringReportVariableComment.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            manageMonitoringReportVariableComment.UserName = _jwtTokenAccesser.UserName;
            manageMonitoringReportVariableComment.UserRole = _jwtTokenAccesser.RoleName;

            if (manageMonitoringReportVariableComment.QueryStatus != CtmsCommentStatus.Open)
            {
                var lastQuery = All.Where(x => x.ManageMonitoringReportVariableId == manageMonitoringReportVariableComment.ManageMonitoringReportVariableId).OrderByDescending(t => t.Id).FirstOrDefault();
                manageMonitoringReportVariableComment.QueryParentId = lastQuery.Id;
                manageMonitoringReportVariableComment.PreviousQueryDate = lastQuery.CreatedDate;
            }

            manageMonitoringReportVariableComment.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(manageMonitoringReportVariableComment);
        }
    }
}