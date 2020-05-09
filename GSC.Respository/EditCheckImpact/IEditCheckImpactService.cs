using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public interface IEditCheckImpactService
    {
        List<EditCheckValidateDto> GetEditCheck(ScreeningTemplate screeningTemplate, int projectDesignId, int domainId);
        ScreeningTemplate GetScreeningTemplate(int projectDesignTemplateId, int screeningEntryId);
        string GetVariableValue(EditCheckValidateDto editCheckValidateDto);
        string CollectionValueAnnotation(string collectionValue);
        string ScreeningValueAnnotation(string value, EditCheckRuleBy checkBy);
    }
}
