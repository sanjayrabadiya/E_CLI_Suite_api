﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Workflow;

namespace GSC.Respository.Project.Workflow
{
    public interface IProjectWorkflowRepository : IGenericRepository<ProjectWorkflow>
    {
        WorkFlowLevelDto GetProjectWorkLevel(int projectDesignId);
        int GetMaxWorkFlowLevel(int projectDesignId);
        bool IsElectronicsSignatureComplete(int ProjectDesignId);
        short GetNoCRFLevel(int projectDesignId, short levelNo);
        short GetMaxLevelWorkBreak(int projectDesignId);
        short GetVisitLevel(int projectDesignVisitId, int projectDesignId, short levelNo);
        short GetTemplateWorkFlow(int projectDesignTemplateId, int projectDesignId, short levelNo);

    }
}