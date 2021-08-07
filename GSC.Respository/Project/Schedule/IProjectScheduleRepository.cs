using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;

namespace GSC.Respository.Project.Schedule
{
    public interface IProjectScheduleRepository : IGenericRepository<ProjectSchedule>
    {
        IList<ProjectScheduleTemplateDto> GetDataByPeriod(long periodId, long projectId);
        IList<ProjectScheduleDto> GetData(int id);
        int GetRefVariableValuefromTargetVariable(int projectDesignVariableId);
    }
}