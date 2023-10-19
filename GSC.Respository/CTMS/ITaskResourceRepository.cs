using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.CTMS
{
    public interface ITaskResourceRepository : IGenericRepository<TaskResource>
    {
        dynamic GetTaskResourceList(bool isDeleted, int PlanTaskId);
        string Duplicate(TaskResource objSave);
    }
}
