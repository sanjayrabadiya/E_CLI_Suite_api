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
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

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
                 .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider).ToList();

            ctmsWorkflows.ForEach(x =>
            {
                x.HasChild = x.CtmsWorkflowApprovalId != null;
            });

            return ctmsWorkflows;
        }

        public List<CtmsWorkflowApprovalGridDto> GetApprovalByApprover(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var ctmsWorkflows = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId && x.TriggerType == triggerType)
                 .ProjectTo<CtmsWorkflowApprovalGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return ctmsWorkflows;
        }

        public bool GetApprovalStatus(int studyPlanId, int projectId)
        {
            var status = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId)
                .GroupBy(x => x.UserId).All(m => m.Any(c => c.IsApprove == true));
            return status;
        }

        public bool CheckSender(int studyPlanId, int projectId, TriggerType triggerType)
        {
            var status = All.Any(x => x.DeletedDate == null && x.ProjectId == projectId && x
            .StudyPlanId == studyPlanId && x.SenderId == _jwtTokenAccesser.UserId && x.TriggerType == triggerType);
            return status;
        }
        public List<ProjectRightDto> GetProjectRightByProjectId(int projectId, TriggerType triggerType)
        {
            var roles = _context.CtmsApprovalRoles.Include(i => i.SecurityRole).Where(x => x.DeletedDate == null && x.SecurityRole.Id != 2
            && x.ProjectId == projectId && x.TriggerType == triggerType).
                Select(c => new ProjectRightDto
                {
                    RoleId = c.SecurityRole.Id,
                    Name = c.SecurityRole.RoleName,
                    users = _context.CtmsApprovalUsers.Include(i => i.Users).Where(a => a.CtmsApprovalRolesId == c.Id && a.Users.DeletedDate == null
                                                                          && a.DeletedDate == null).Select(r =>
                        new ProjectRightDto
                        {
                            RoleId = c.SecurityRole.Id,
                            UserId = r.UserId,
                            Name = r.Users.UserName,
                            IsSelected = All.Any(b => b.ProjectId == projectId && b.RoleId == c.SecurityRoleId && b.UserId == r.UserId && b.DeletedDate == null)
                        }).Where(x => !x.IsSelected).ToList()
                }).ToList();

            return roles.Where(x => x.users.Count != 0).ToList();
        }
    }
}
