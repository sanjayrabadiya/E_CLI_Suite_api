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
    public class CtmsSiteContractWorkflowApprovalRepository : GenericRespository<CtmsSiteContractWorkflowApproval>, ICtmsSiteContractWorkflowApprovalRepository
    {
        private readonly IMapper _mapper;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public CtmsSiteContractWorkflowApprovalRepository(IGSCContext context, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        /// <summary>
        /// Filters workflows based on common parameters.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>An IQueryable of filtered workflows.</returns>
        private IQueryable<CtmsSiteContractWorkflowApproval> FilteredWorkflows(int siteContractId, int projectId, TriggerType triggerType)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.SiteContractId == siteContractId && x.TriggerType == triggerType);
        }

        /// <summary>
        /// Gets approval records by sender.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>A list of approval records for the sender.</returns>
        public List<CtmsSiteContractWorkflowApprovalGridDto> GetApprovalBySender(int siteContractId, int projectId, TriggerType triggerType)
        {
            var senderId = _jwtTokenAccesser.UserId;
            var ctmsWorkflows = FilteredWorkflows(siteContractId, projectId, triggerType)
                                .Where(x => x.SenderId == senderId)
                                .ProjectTo<CtmsSiteContractWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                                .ToList();

            ctmsWorkflows.ForEach(x => x.HasChild = x.CtmsSiteContractWorkflowApprovalId != null);
            return ctmsWorkflows;
        }

        /// <summary>
        /// Gets approval records by approver.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>A list of approval records for the approver.</returns>
        public List<CtmsSiteContractWorkflowApprovalGridDto> GetApprovalByApprover(int siteContractId, int projectId, TriggerType triggerType)
        {
            var userId = _jwtTokenAccesser.UserId;
            var roleId = _jwtTokenAccesser.RoleId;
            return FilteredWorkflows(siteContractId, projectId, triggerType)
                   .Where(x => x.UserId == userId && x.RoleId == roleId)
                   .OrderByDescending(x => x.Id)
                   .ProjectTo<CtmsSiteContractWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                   .ToList();
        }

        /// <summary>
        /// Checks if all users have approved the workflow.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>True if all users have approved, otherwise false.</returns>
        public bool GetApprovalStatus(int siteContractId, int projectId, TriggerType triggerType)
        {
            return FilteredWorkflows(siteContractId, projectId, triggerType)
                   .GroupBy(x => x.UserId)
                   .All(group => group.Any(c => c.IsApprove == true));
        }

        /// <summary>
        /// Checks if the current user is the sender of the workflow.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="siteId">The Site ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>True if the current user is the sender, otherwise false.</returns>
        public bool CheckSender(int siteContractId, int projectId, int siteId, TriggerType triggerType)
        {
            var senderId = _jwtTokenAccesser.UserId;
            return FilteredWorkflows(siteContractId, projectId, triggerType)
                   .Any(x => x.SiteId == siteId && x.SenderId == senderId);
        }

        /// <summary>
        /// Gets project roles and users associated with those roles for a project.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="siteId">The Site ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>A list of project rights including roles and users.</returns>
        public List<ProjectRightDto> GetProjectRightByProjectId(int siteContractId, int projectId, int siteId, TriggerType triggerType)
        {
            return _context.CtmsApprovalRoles
                   .Include(i => i.SecurityRole)
                   .Where(x => x.DeletedDate == null && x.SecurityRole.Id != 2 && x.ProjectId == projectId && x.SiteId == siteId && x.TriggerType == triggerType)
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
                               IsSelected = All.Any(b => b.SiteContractId == siteContractId && b.ProjectId == projectId && b.RoleId == c.SecurityRoleId && b.UserId == r.UserId && b.DeletedDate == null && b.TriggerType == triggerType)
                           })
                           .Where(x => !x.IsSelected)
                           .ToList()
                   })
                   .Where(x => x.users.Any())
                   .ToList();
        }

        /// <summary>
        /// Gets new comments for the approver.
        /// </summary>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>A list of approval records with new comments for the approver.</returns>
        public List<CtmsSiteContractWorkflowApprovalGridDto> GetApproverNewComment(TriggerType triggerType)
        {
            var userId = _jwtTokenAccesser.UserId;
            var roleId = _jwtTokenAccesser.RoleId;
            return FilteredWorkflows(0, 0, triggerType) // `siteContractId` and `projectId` are not used here
                   .Where(x => x.IsApprove == null && string.IsNullOrEmpty(x.ApproverComment) && x.UserId == userId && x.RoleId == roleId)
                   .ProjectTo<CtmsSiteContractWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                   .ToList();
        }

        /// <summary>
        /// Gets new comments for the sender.
        /// </summary>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>A list of approval records with new comments for the sender.</returns>
        public List<CtmsSiteContractWorkflowApprovalGridDto> GetSenderNewComment(TriggerType triggerType)
        {
            var senderId = _jwtTokenAccesser.UserId;
            return FilteredWorkflows(0, 0, triggerType) // `siteContractId` and `projectId` are not used here
                   .Where(x => x.IsApprove == false && x.SenderId == senderId)
                   .ProjectTo<CtmsSiteContractWorkflowApprovalGridDto>(_mapper.ConfigurationProvider)
                   .ToList();
        }

        /// <summary>
        /// Gets the current user's approval tasks for a project.
        /// </summary>
        /// <param name="projectId">The Project ID.</param>
        /// <returns>A list of dashboard DTOs representing the user's approval tasks.</returns>
        public List<DashboardDto> GetCtmsApprovalMyTask(int projectId)
        {
            var myTaskList = new List<DashboardDto>();

            var ctmsMyTasks = GetCtmsTasks(projectId, senderId: _jwtTokenAccesser.UserId, isApprove: false);
            var ctmsMyTasksApprover = GetCtmsTasks(projectId, userId: _jwtTokenAccesser.UserId, roleId: _jwtTokenAccesser.RoleId, isApprove: false);

            myTaskList.AddRange(ctmsMyTasks);
            myTaskList.AddRange(ctmsMyTasksApprover);

            return myTaskList;
        }

        /// <summary>
        /// Helper method to get CTMS tasks based on various parameters.
        /// </summary>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="senderId">The Sender ID.</param>
        /// <param name="userId">The User ID.</param>
        /// <param name="roleId">The Role ID.</param>
        /// <param name="isApprove">The approval status.</param>
        /// <returns>A list of dashboard DTOs representing CTMS tasks.</returns>
        private List<DashboardDto> GetCtmsTasks(int projectId, int? senderId = null, int? userId = null, int? roleId = null, bool? isApprove = null)
        {
            var query = FilteredWorkflows(0, projectId, 0); // `siteContractId` and `triggerType` are not used here

            if (senderId.HasValue) query = query.Where(x => x.SenderId == senderId.Value);
            if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
            if (roleId.HasValue) query = query.Where(x => x.RoleId == roleId.Value);
            if (isApprove.HasValue) query = query.Where(x => x.IsApprove == isApprove.Value);

            return query.Select(s => new DashboardDto
            {
                Id = s.Id,
                TaskInformation = $"Sender Comment : {s.SenderComment}, Approver Comment:{s.ApproverComment}",
                ExtraData = s.SiteContractId,
                CreatedDate = s.SendDate,
                CreatedByUser = _context.Users.FirstOrDefault(x => x.Id == s.UserId).UserName,
                Module = "CTMS",
                DataType = s.TriggerType.ToString(),
                ControlType = DashboardMyTaskType.CTMSApprovalSystem
            }).ToList();
        }

        /// <summary>
        /// Gets approval users for a site contract.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <returns>A list of approval users.</returns>
        public List<ApprovalUser> GetApprovalUsers(int siteContractId)
        {
            return FilteredWorkflows(siteContractId, 0, 0) // `projectId` and `triggerType` are not used here
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

        /// <summary>
        /// Checks if there are new comments for the current user.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>True if there are new comments, otherwise false.</returns>
        public bool IsNewComment(int siteContractId, int projectId, TriggerType triggerType)
        {
            var userId = _jwtTokenAccesser.UserId;
            var roleId = _jwtTokenAccesser.RoleId;
            return FilteredWorkflows(siteContractId, projectId, triggerType)
                   .Any(x => x.UserId == userId && x.RoleId == roleId && x.IsApprove == null);
        }

        /// <summary>
        /// Checks if there is a comment reply for the specified user and role.
        /// </summary>
        /// <param name="siteContractId">The Site Contract ID.</param>
        /// <param name="projectId">The Project ID.</param>
        /// <param name="userId">The User ID.</param>
        /// <param name="roleId">The Role ID.</param>
        /// <param name="triggerType">The Trigger Type.</param>
        /// <returns>True if there is a comment reply, otherwise false.</returns>
        public bool IsCommentReply(int siteContractId, int projectId, int userId, int roleId, TriggerType triggerType)
        {
            return FilteredWorkflows(siteContractId, projectId, triggerType)
                   .Any(x => x.UserId == userId && x.RoleId == roleId && x.IsApprove == false);
        }
    }
}
