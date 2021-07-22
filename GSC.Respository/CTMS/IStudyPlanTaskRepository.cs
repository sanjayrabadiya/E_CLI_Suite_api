using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public interface IStudyPlanTaskRepository : IGenericRepository<StudyPlanTask>
    {
        StudyPlanTaskGridDto GetStudyPlanTaskList(bool isDeleted, int StudyPlanId, int ProjectId);
        int UpdateTaskOrder(StudyPlantaskParameterDto taskmasterDto);
        string ValidateTask(StudyPlanTask taskmasterDto);
        void UpdateParentDate(int? ParentId);
        void InsertDependentTask(List<DependentTaskParameterDto> dependentTasks, int StudyPlanTaskId);
        void UpdateTaskOrderSequence(int StudyPlanId);
        string UpdateDependentTask(int StudyPlanTaskId);
        StudyPlanTask UpdateDependentTaskDate(StudyPlanTask StudyPlanTask);
        DateTime GetNextWorkingDate(NextWorkingDateParameterDto parameterDto);
        string ValidateweekEnd(NextWorkingDateParameterDto parameterDto);
        List<StudyPlanTask> Save(StudyPlanTask taskData);
        List<AuditTrailCommonDto> GetStudyPlanTaskHistory(int id);
    }
}
