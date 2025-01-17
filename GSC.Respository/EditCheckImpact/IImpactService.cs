﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public interface IImpactService : IGenericRepository<ScreeningTemplate>
    {
        List<EditCheckValidateDto> GetEditCheck(ScreeningTemplateBasic screeningTemplateBasic);
        ScreeningTemplate GetScreeningTemplate(int projectDesignTemplateId, int screeningEntryId, int? screeningVisitId);      
        string CollectionValueAnnotation(string collectionValue, CollectionSources? collectionSource);
        List<EditCheckValidateDto> GetEditCheckByVaiableId(int projectDesignTemplateId, int projectDesignVariableId, List<EditCheckIds> editCheckIds);
        string ScreeningValueAnnotation(string value, EditCheckRuleBy checkBy, CollectionSources? collectionSource);
        List<ScheduleCheckValidateDto> GetTargetSchedule(int projectDesignTemplateId, bool isQuery);
        List<ScheduleCheckValidateDto> GetReferenceSchedule(List<int> projectScheduleId);
        string GetVariableValue(int screeningTemplateId, int projectDesignVariableId);
        string GetVariableValue(EditCheckValidateDto editCheckValidateDto, out bool isNa);
        List<ScheduleCheckValidateDto> GetTargetScheduleByVariableId(int ProjectDesignVariableId);
        ScheduleTemplateDto GetScreeningTemplateId(int projectDesignTemplateId, int screeningEntryId);
        bool CheckReferenceVariable(int projectDesignVariableId);
        decimal NumericCollectionValue(string id);
        string GetProjectDesignVariableId(int projectDesignVariableId, string collectionSource);
        int CheckVisitRefAndTarget(int projectDesignVisitId, List<int> startedProjectDesignVisitId);
        string GetHardSoftValue(int screeningTemplateId, int projectDesignVariableId, int soruceProjectDesignVariableId, CollectionSources? collectionSource);
    }
}
