using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Workflow
{
    public class ProjectWorkflowIndependentRepository : GenericRespository<ProjectWorkflowIndependent>,
        IProjectWorkflowIndependentRepository
    {
        public ProjectWorkflowIndependentRepository(IGSCContext context) :
            base(context)
        {
        }
    }
}