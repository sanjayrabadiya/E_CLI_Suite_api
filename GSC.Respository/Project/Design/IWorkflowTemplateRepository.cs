using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IWorkflowTemplateRepository : IGenericRepository<WorkflowTemplate>
    {
        List<int> GetDetailById(WorkflowTemplateDto workflowTemplateDto);
        void updatePermission(WorkflowTemplateDto workflowTemplateDto);
        bool CheckforTemplateisExists(int projectDesignVisitId);
    }
}