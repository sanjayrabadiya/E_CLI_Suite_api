using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public interface IScheduleRuleRespository : IGenericRepository<ScreeningTemplate>
    {
        List<ScheduleCheckValidateDto> ValidateByTemplate(List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic, bool isQuery);
        List<ScheduleCheckValidateDto> ValidateByVariable(int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, bool isQuery);
        List<EditCheckTargetValidationList> VariableResultProcess(List<EditCheckTargetValidationList> editCheckResult, List<ScheduleCheckValidateDto> scheduleResult);
    }
}
