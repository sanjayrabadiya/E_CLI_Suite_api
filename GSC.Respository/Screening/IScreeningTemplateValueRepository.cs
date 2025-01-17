﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;

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

        Task GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters);
        List<TemplateTotalQueryDto> GetQueryStatusBySubject(int screeningEntryId);

        List<VariableQueryDto> GetTemplateQueryList(int screeningTemplateId);

        List<ScreeningVariableValueDto> GetScreeningRelation(int projectDesignVariableId, int screeningEntryId);
        DesignScreeningVariableDto GetQueryVariableDetail(int id, int screeningEntryId);
        void UpdateDefaultValue(IList<DesignScreeningVariableDto> variableList, int screeningTemplateId);
        void DeleteRepeatTemplateValue(int Id);
        void UpdateTemplateConfigurationUploadRandomizationValue(DesignScreeningTemplateDto designScreeningTemplateDto, int screeningTemplateId);

        bool IsEligible(int VolunteerId);

        void UpdateDefaultValueForDosing(IList<DesignScreeningVariableDto> variableList, int screeningTemplateId, bool IsDosing);

        bool CheckOldValue(string originalString, CollectionSources? collectionSource);
    }
}