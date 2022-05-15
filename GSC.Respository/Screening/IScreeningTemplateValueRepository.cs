﻿using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueRepository : IGenericRepository<ScreeningTemplateValue>
    {
        void UpdateVariableOnSubmit(int projectDesignTemplateId, int screeningTemplateId);

        int GetQueryStatusCount(int screeningTemplateId);

        void DeleteChild(int screeningTemplateValueId);
        void UpdateChild(List<ScreeningTemplateValueChild> children);
        string CheckCloseQueries(int screeningTemplateId);
        string GetValueForAudit(ScreeningTemplateValueDto screeningTemplateValueDto);
        bool IsFitness(int screeningTemplateId);
        bool IsDiscontinued(int screeningTemplateId);

        void GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters);
        List<TemplateTotalQueryDto> GetQueryStatusBySubject(int screeningEntryId);

        List<VariableQueryDto> GetTemplateQueryList(int screeningTemplateId);

        List<ScreeningVariableValueDto> GetScreeningRelation(int projectDesignVariableId, int screeningEntryId);
        DesignScreeningVariableDto GetQueryVariableDetail(int id, int screeningEntryId);
        void UpdateDefaultValue(IList<DesignScreeningVariableDto> variableList, int screeningTemplateId);
    }
}