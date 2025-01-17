﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ITaskMasterRepository : IGenericRepository<TaskMaster>
    {
        List<TaskMasterGridDto> GetTasklist(bool isDeleted, int templateId);
        int UpdateTaskOrder(TaskmasterDto taskmasterDto);
        List<AuditTrailDto> GetTaskHistory(int id);
        string AddTaskToSTudyPlan(TaskmasterDto taskmasterDto);
        void AddRefrenceTypes(TaskmasterDto taskmasterDto);
    }
}
