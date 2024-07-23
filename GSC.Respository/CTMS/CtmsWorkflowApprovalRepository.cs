using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class CtmsWorkflowApprovalRepository : GenericRespository<CtmsWorkflowApproval>, ICtmsWorkflowApprovalRepository
    {
        private readonly IMapper _mapper;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public CtmsWorkflowApprovalRepository(IGSCContext context, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<CtmsWorkflowApprovalGridDto> GetApprovalBySender(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var ctmsWorkflows = All.Where(x => x.SenderId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId && x.TriggerType == triggerType)
                 .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                 .ToList();

            ctmsWorkflows.ForEach(x => x.HasChild = x.CtmsWorkflowApprovalId != null);

            return ctmsWorkflows;
        }

        public List<CtmsWorkflowApprovalGridDto> GetApprovalByApprover(int studyPlanId, int projectId, TriggerType triggerType)
        {
            return All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId && x.TriggerType == triggerType)
                .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public bool GetApprovalStatus(int studyPlanId, int projectId, TriggerType triggerType)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId && x.TriggerType == triggerType)
                .GroupBy(x => x.UserId)
                .All(group => group.Any(c => c.IsApprove == true));
        }

        public bool CheckSender(int studyPlanId, int projectId, TriggerType triggerType)
        {
            return All.Any(x => x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId && x.SenderId == _jwtTokenAccesser.UserId && x.TriggerType == triggerType);
        }

        public List<ProjectRightDto> GetProjectRightByProjectId(int projectId, TriggerType triggerType)
        {
            var roles = _context.CtmsApprovalRoles
                .Include(i => i.SecurityRole)
                .Where(x => x.DeletedDate == null && x.SecurityRole.Id != 2 && x.ProjectId == projectId && x.TriggerType == triggerType)
                .Select(c => new ProjectRightDto
                {
                    RoleId = c.SecurityRole.Id,
                    Name = c.SecurityRole.RoleName,
                    users = _context.CtmsApprovalUsers
                        .Include(i => i.Users)
                        .Where(a => a.CtmsApprovalRolesId == c.Id && a.Users.DeletedDate == null && a.DeletedDate == null)
                        .Select(r => new ProjectRightDto
                        {
                            RoleId = c.SecurityRole.Id,
                            UserId = r.UserId,
                            Name = r.Users.UserName,
                            IsSelected = All.Any(b => b.ProjectId == projectId && b.RoleId == c.SecurityRoleId && b.UserId == r.UserId && b.DeletedDate == null && b.TriggerType == triggerType)
                        })
                        .Where(x => !x.IsSelected)
                        .ToList()
                })
                .Where(x => x.users.Any())
                .ToList();

            return roles;
        }

        public List<CtmsWorkflowApprovalGridDto> GetApproverNewComment(TriggerType triggerType)
        {
            return All.Where(x => x.DeletedDate == null && x.IsApprove == null && string.IsNullOrEmpty(x.ApproverComment) && x.TriggerType == triggerType && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId)
                .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public List<CtmsWorkflowApprovalGridDto> GetSenderNewComment(TriggerType triggerType)
        {
            return All.Where(x => x.DeletedDate == null && x.IsApprove == false && x.TriggerType == triggerType && x.SenderId == _jwtTokenAccesser.UserId)
                .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public List<DashboardDto> GetCtmsApprovalMyTask(int projectId)
        {
            var myTaskList = new List<DashboardDto>();

            var ctmsMyTasks = GetCtmsTasks(projectId, senderId: _jwtTokenAccesser.UserId, isApprove: false);
            var ctmsMyTasksApprover = GetCtmsTasks(projectId, userId: _jwtTokenAccesser.UserId, roleId: _jwtTokenAccesser.RoleId, isApprove: false);

            myTaskList.AddRange(ctmsMyTasks);
            myTaskList.AddRange(ctmsMyTasksApprover);

            return myTaskList;
        }

        private List<DashboardDto> GetCtmsTasks(int projectId, int? senderId = null, int? userId = null, int? roleId = null, bool? isApprove = null)
        {
            var query = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId);

            if (senderId.HasValue) query = query.Where(x => x.SenderId == senderId.Value);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
            if (roleId.HasValue) query = query.Where(x => x.RoleId == roleId.Value);
            if (isApprove.HasValue) query = query.Where(x => x.IsApprove == isApprove.Value);

            return query.Select(s => new DashboardDto
            {
                Id = s.Id,
                TaskInformation = $"Sender Comment : {s.SenderComment}, Approver Comment:{s.ApproverComment}",
                ExtraData = s.StudyPlanId,
                CreatedDate = s.SendDate,
                CreatedByUser = _context.Users.FirstOrDefault(x => x.Id == s.UserId).UserName,
                Module = "CTMS",
                DataType = s.TriggerType.ToString(),
                ControlType = DashboardMyTaskType.CTMSApprovalSystem
            }).ToList();
        }

        public List<ApprovalUser> GetApprovalUsers(int studyPlanId)
        {
            return All.Where(x => x.DeletedDate == null && x.StudyPlanId == studyPlanId)
                .GroupBy(g => g.UserId)
                .Select(s => new ApprovalUser
                {
                    StudyPlanTaskId = 0,
                    UserId = s.Key,
                    RoleId = s.First().RoleId,
                    Username = $"{s.First().User.FirstName} {s.First().User.LastName}"
                })
                .ToList();
        }

        public bool IsNewComment(int studyPlanId, int projectId, TriggerType triggerType)
        {
            return All.Any(x => x.DeletedDate == null && x.StudyPlanId == studyPlanId && x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.TriggerType == triggerType && x.IsApprove == null);
        }

        public bool IsCommentReply(int studyPlanId, int projectId, int userId, int roleId, TriggerType triggerType)
        {
            return All.Any(x => x.DeletedDate == null && x.StudyPlanId == studyPlanId && x.ProjectId == projectId && x.UserId == userId && x.RoleId == roleId && x.TriggerType == triggerType && x.IsApprove == false);
        }
    }
}
