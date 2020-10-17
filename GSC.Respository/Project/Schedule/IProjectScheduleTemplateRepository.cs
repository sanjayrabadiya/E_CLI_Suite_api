using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Schedule;

namespace GSC.Respository.Project.Schedule
{
    public interface IProjectScheduleTemplateRepository : IGenericRepository<ProjectScheduleTemplate>
    {
        void UpdateTemplates(ProjectSchedule projectSchedule);
        void UpdateDesignTemplatesOrder(ProjectSchedule projectSchedule);
        void UpdateDesignTemplatesSchedule(int projectDesignPeriodId);
    }
}