using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace GSC.Respository.CTMS
{
    public class CtmsWorkflowApprovalRepository : GenericRespository<CtmsWorkflowApproval>, ICtmsWorkflowApprovalRepository
    {
        private readonly IMapper _mapper;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public CtmsWorkflowApprovalRepository(IGSCContext context, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _mapper = mapper;
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
    }
}
