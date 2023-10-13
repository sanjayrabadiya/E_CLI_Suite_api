using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public interface ITaskResourceRepository : IGenericRepository<TaskResource>
    {
        dynamic GetTaskResourceList(bool isDeleted, int PlanTaskId);
        string Duplicate(TaskResource objSave);
    }
}
