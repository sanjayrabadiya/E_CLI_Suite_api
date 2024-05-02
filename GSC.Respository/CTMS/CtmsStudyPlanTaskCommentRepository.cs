using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public class CtmsStudyPlanTaskCommentRepository : GenericRespository<CtmsStudyPlanTaskComment>, ICtmsStudyPlanTaskCommentRepository
    {
        private readonly IGSCContext _context;
        private readonly ICtmsWorkflowApprovalRepository _ctmsWorkflowApprovalRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public CtmsStudyPlanTaskCommentRepository(IGSCContext context,
            ICtmsWorkflowApprovalRepository ctmsWorkflowApprovalRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _context = context;
            _ctmsWorkflowApprovalRepository = ctmsWorkflowApprovalRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<CtmsStudyPlanTaskCommentGridDto> GetCommentHistory(int id, int studyPlanId, TriggerType triggerType)
        {
            var ctmsWorkflows = _ctmsWorkflowApprovalRepository.All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null && x.StudyPlanId == studyPlanId && x.TriggerType == triggerType)
                 .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var takComments = _mapper.Map<List<CtmsStudyPlanTaskCommentGridDto>>(ctmsWorkflows);

            var ctmsCommentTasks = All.Where(x => x.DeletedDate == null && x.StudyPlanTaskId == id && x.CtmsWorkflowApproval.DeletedDate == null).ToList();
            if (ctmsCommentTasks.Any())
            {
                takComments.ForEach(x =>
                {
                    var taskComment = ctmsCommentTasks.Find(y => y.CtmsWorkflowApprovalId == x.CtmsWorkflowApprovalId && y.DeletedDate == null);
                    if (taskComment != null)
                    {
                        x.Id = taskComment.Id;
                        x.StudyPlanTaskId = taskComment.StudyPlanTaskId;
                        x.CtmsWorkflowApprovalId = taskComment.CtmsWorkflowApprovalId;
                        x.Comment = taskComment.Comment;
                        if (x.IsApprove == true)
                        {
                            x.IsReply = taskComment.IsReply;
                            x.ReplyComment = taskComment.ReplyComment;
                        }
                    }
                });
            }

            return takComments;
        }

        public List<CtmsStudyPlanTaskCommentGridDto> GetSenderCommentHistory(int id, int userId, int roleId, int studyPlanId, TriggerType triggerType)
        {
            var ctmsCommentTasks = All.Where(x => x.DeletedDate == null && x.StudyPlanTaskId == id && x.CtmsWorkflowApproval.RoleId == roleId
            && x.CtmsWorkflowApproval.UserId == userId
            && x.CtmsWorkflowApproval.TriggerType == triggerType
            && x.CtmsWorkflowApproval.StudyPlanId == studyPlanId
            && (x.CtmsWorkflowApproval.SenderId == _jwtTokenAccesser.UserId ? (x.CtmsWorkflowApproval.IsApprove == false || x.CtmsWorkflowApproval.IsApprove == true) : true)
            && x.CtmsWorkflowApproval.DeletedDate == null)
                .ProjectTo<CtmsStudyPlanTaskCommentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(o => o.Id).ToList();

            ctmsCommentTasks.ForEach(x =>
            {
                if (!x.FinalReply && x.SenderId != _jwtTokenAccesser.UserId)
                {
                    x.ReplyComment = "";
                }
            });

            return ctmsCommentTasks;
        }

        public bool CheckAllTaskComment(int ctmsApprovalId)
        {
            var isReply = All.Any(x => x.DeletedDate == null && x.CtmsWorkflowApprovalId == ctmsApprovalId && !x.IsReply);
            return isReply;
        }
    }
}
