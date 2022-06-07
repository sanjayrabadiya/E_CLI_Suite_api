using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;

namespace GSC.Respository.Project.Schedule
{
    public interface IScheduleTerminateDetailRepository : IGenericRepository<ScheduleTerminateDetail>
    {
        ScheduleTerminateDetailDto GetDetailById(int ProjectScheduleTemplateId);
    }
}