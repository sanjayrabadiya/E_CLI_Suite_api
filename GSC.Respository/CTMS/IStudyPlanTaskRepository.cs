using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public interface IStudyPlanTaskRepository : IGenericRepository<StudyPlanTask>
    {
        StudyPlanTaskGridDto GetStudyPlanTaskList(bool isDeleted, int StudyPlanId);
        int UpdateTaskOrder(StudyPlantaskParameterDto taskmasterDto);
        string ValidateTask(StudyPlanTask taskmasterDto);
        void UpdateParentDate(int? ParentId);
        void InsertDependentTask(List<DependentTaskParameterDto> dependentTasks, int StudyPlanTaskId);
        void UpdateTaskOrderSequence(int StudyPlanId);
        string UpdateDependentTask(int StudyPlanTaskId);
        StudyPlanTask UpdateDependentTaskDate(StudyPlanTask StudyPlanTask);
    }
}
