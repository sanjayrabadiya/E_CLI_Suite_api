using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Project.Workflow
{
    public class ProjectWorkflowLevelRepository : GenericRespository<ProjectWorkflowLevel, GscContext>,
        IProjectWorkflowLevelRepository
    {
        public ProjectWorkflowLevelRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) : base(
            uow, jwtTokenAccesser)
        {
        }
    }
}