using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Workflow;
using System.Collections.Generic;
using System;

namespace GSC.Respository.Project.Workflow
{
    public interface IWorkflowVisitRepository : IGenericRepository<WorkflowVisit>
    {
        List<int> GetDetailById(WorkflowVisitDto workflowVisitDto);
    }
}