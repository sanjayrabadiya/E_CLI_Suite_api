using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Schedule;

namespace GSC.Respository.Project.Schedule
{
    public interface ISchedulerRuleRespository : IGenericRepository<ProjectScheduleTemplate>
    {
        void ValidateRuleByTemplate(int screeningTemplateId, int projectDesignTemplateId, int screeningEntryId,
            bool isFromQuery, ref List<int> projectDesignVariableId);

        void SchedulerRuleByVariable(VariableEditCheckDto variableEditCheckDto, ref List<int> projectDesignVariableId);
    }
}