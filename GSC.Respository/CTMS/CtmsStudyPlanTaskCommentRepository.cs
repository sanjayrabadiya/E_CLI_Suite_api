using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsStudyPlanTaskCommentRepository : GenericRespository<CtmsStudyPlanTaskComment>, ICtmsStudyPlanTaskCommentRepository
    {
        private readonly IGSCContext _context;
        private readonly ICtmsWorkflowApprovalRepository _workflowApprovalRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public CtmsStudyPlanTaskCommentRepository(IGSCContext context,
            ICtmsWorkflowApprovalRepository workflowApprovalRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _context = context;
            _workflowApprovalRepository = workflowApprovalRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<CtmsStudyPlanTaskCommentGridDto> GetCommentHistory(int id, int studyPlanId, TriggerType triggerType)
        {
            var ctmsWorkflows = _workflowApprovalRepository.All
                .Where(x => x.UserId == _jwtTokenAccesser.UserId
                    && x.RoleId == _jwtTokenAccesser.RoleId
                    && x.DeletedDate == null
                    && x.StudyPlanId == studyPlanId
                    && x.TriggerType == triggerType)
                .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(x => x.Id)
                .ToList();

            var taskComments = _mapper.Map<List<CtmsStudyPlanTaskCommentGridDto>>(ctmsWorkflows);
            var commentTasks = All
                .Where(x => x.DeletedDate == null
                    && x.StudyPlanTaskId == id
                    && x.CtmsWorkflowApproval.DeletedDate == null)
                .ToList();

            if (commentTasks.Any())
            {
                taskComments.ForEach(taskComment =>
                {
                    var matchedComment = commentTasks
                        .FirstOrDefault(comment => comment.CtmsWorkflowApprovalId == taskComment.CtmsWorkflowApprovalId
                            && comment.DeletedDate == null);

                    if (matchedComment != null)
                    {
                        taskComment.Id = matchedComment.Id;
                        taskComment.StudyPlanTaskId = matchedComment.StudyPlanTaskId;
                        taskComment.CtmsWorkflowApprovalId = matchedComment.CtmsWorkflowApprovalId;
                        taskComment.Comment = matchedComment.Comment;
                        if (taskComment.IsApprove == true)
                        {
                            taskComment.IsReply = matchedComment.IsReply;
                            taskComment.ReplyComment = matchedComment.ReplyComment;
                        }
                    }
                });
            }

            return taskComments;
        }

        public List<CtmsStudyPlanTaskCommentGridDto> GetSenderCommentHistory(int id, int userId, int roleId, int studyPlanId, TriggerType triggerType)
        {
            var commentTasks = All
                .Where(x => x.DeletedDate == null
                    && x.StudyPlanTaskId == id
                    && x.CtmsWorkflowApproval.RoleId == roleId
                    && x.CtmsWorkflowApproval.UserId == userId
                    && x.CtmsWorkflowApproval.TriggerType == triggerType
                    && x.CtmsWorkflowApproval.StudyPlanId == studyPlanId
                    && (x.CtmsWorkflowApproval.SenderId == _jwtTokenAccesser.UserId
                        ? x.CtmsWorkflowApproval.IsApprove == false || x.CtmsWorkflowApproval.IsApprove == true
                        : true)
                    && x.CtmsWorkflowApproval.DeletedDate == null)
                .ProjectTo<CtmsStudyPlanTaskCommentGridDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(o => o.Id)
                .ToList();

            commentTasks.ForEach(taskComment =>
            {
                if (!taskComment.FinalReply && taskComment.SenderId != _jwtTokenAccesser.UserId)
                {
                    taskComment.ReplyComment = string.Empty;
                }
            });

            return commentTasks;
        }

        public bool CheckAllTaskComment(int ctmsApprovalId)
        {
            return All.Any(x => x.DeletedDate == null && x.CtmsWorkflowApprovalId == ctmsApprovalId && !x.IsReply);
        }
    }
}
