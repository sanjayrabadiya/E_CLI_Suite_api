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

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringReportVariableCommentRepository : GenericRespository<ManageMonitoringReportVariableComment>, IManageMonitoringReportVariableCommentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IManageMonitoringReportVariableRepository _manageMonitoringReportVariableRepository;


        public ManageMonitoringReportVariableCommentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser,
           IManageMonitoringReportVariableRepository manageMonitoringReportVariableRepository
        )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _manageMonitoringReportVariableRepository = manageMonitoringReportVariableRepository;
        }
        public IList<ManageMonitoringReportVariableCommentDto> GetComments(int manageMonitoringReportVariableId)
        {
            var comments = All.Where(x => x.ManageMonitoringReportVariableId == manageMonitoringReportVariableId
                                          && x.DeletedDate == null)
                .Select(t => new ManageMonitoringReportVariableCommentDto
                {
                    Id= t.Id,
                    Comment = t.Comment,
                    CommentStatusName = t.CommentStatus.GetDescription(),
                    CreatedDate = t.CreatedDate,
                    CreatedByName = t.CreatedByUser.UserName,
                    RoleName = t.Role.RoleName,
                    ReasonName = t.Reason.ReasonName,
                    ReasonOth = t.ReasonOth,
                    Note = string.IsNullOrEmpty(t.Note) ? t.ReasonOth : t.Note,
                    CollectionSource = t.ManageMonitoringReportVariable.Variable.CollectionSource
                }).ToList();

            return comments;
        }

        public void UpdateQuery(ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto, ManageMonitoringReportVariableComment manageMonitoringReportVariableComment, ManageMonitoringReportVariable manageMonitoringReportVariable)
        {
            var value = string.IsNullOrEmpty(manageMonitoringReportVariableCommentDto.ValueName)
                ? manageMonitoringReportVariableCommentDto.Value
                : manageMonitoringReportVariableCommentDto.ValueName;

            var updateCommentStatus = manageMonitoringReportVariableCommentDto.Value == manageMonitoringReportVariableComment.Value
                ? CtmsCommentStatus.Answered
                : CtmsCommentStatus.Resolved;


            manageMonitoringReportVariableComment.CommentStatus = updateCommentStatus;
            manageMonitoringReportVariableComment.Value = value;
            manageMonitoringReportVariableComment.OldValue = manageMonitoringReportVariableCommentDto.OldValue;

            //manageMonitoringReportVariable.QueryStatus = updateCommentStatus;
            manageMonitoringReportVariable.Value = manageMonitoringReportVariableCommentDto.Value;

            //var screeningTemplate = _context.ScreeningTemplate.Find(screeningTemplateValue.ScreeningTemplateId);
            //QueryAudit(screeningTemplateValueQueryDto, screeningTemplateValue, updateQueryStatus.ToString(), value, screeningTemplateValueQuery);

            Save(manageMonitoringReportVariableComment);

            //_screeningTemplateValueChildRepository.Save(manageMonitoringReportVariable);

            _manageMonitoringReportVariableRepository.Update(manageMonitoringReportVariable);
        }

        public void Save(ManageMonitoringReportVariableComment manageMonitoringReportVariableComment)
        {
            manageMonitoringReportVariableComment.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            //manageMonitoringReportVariableComment.UserName = _jwtTokenAccesser.UserName;
            manageMonitoringReportVariableComment.RoleId = _jwtTokenAccesser.RoleId;
            manageMonitoringReportVariableComment.CreatedDate = _jwtTokenAccesser.GetClientDate();
            Add(manageMonitoringReportVariableComment);
        }
    }
}