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

        public List<CtmsWorkflowApprovalDto> GetApprovalBySender(int studyPlanId, int projectId)
        {
            var ctmsWorkflows = All.Where(x => x.SenderId == _jwtTokenAccesser.UserId && x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId)
                 .ProjectTo<CtmsWorkflowApprovalDto>(_mapper.ConfigurationProvider).ToList();
            return ctmsWorkflows;
        }

        public List<CtmsWorkflowApprovalDto> GetApprovalByApprover(int studyPlanId, int projectId)
        {
            var ctmsWorkflows = All.Where(x => x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId)
                 .ProjectTo<CtmsWorkflowApprovalDto>(_mapper.ConfigurationProvider).ToList();
            return ctmsWorkflows;
        }

        public bool GetApprovalStatus(int studyPlanId, int projectId)
        {
            var status = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.StudyPlanId == studyPlanId)
                .GroupBy(x => x.UserId).All(m => m.Any(c => c.IsApprove == true));
            return status;
        }
        public List<ProjectRightDto> GetProjectRightByProjectId(int projectId, TriggerType triggerType)
        {

            var roles = _context.CtmsApprovalWorkFlow.Include(i => i.SecurityRole).Where(x => x.DeletedDate == null && x.SecurityRole.Id != 2 && x.ProjectId == projectId && x.TriggerType == triggerType).
                Select(c => new ProjectRightDto
                {
                    RoleId = c.SecurityRole.Id,
                    Name = c.SecurityRole.RoleName,
                    users = _context.CtmsApprovalWorkFlowDetail.Include(i => i.Users).Where(a => a.CtmsApprovalWorkFlowId == c.Id && a.Users.DeletedDate == null
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
