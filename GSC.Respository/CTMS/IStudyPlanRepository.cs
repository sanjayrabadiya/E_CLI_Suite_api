using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public interface IStudyPlanRepository : IGenericRepository<StudyPlan>
    {
        List<StudyPlanGridDto> GetStudyplanList(bool isDeleted);
        string ImportTaskMasterData(StudyPlan studyplan);
        string Duplicate(StudyPlan objSave);
        string ValidateTask(StudyPlanTask taskmasterDto, List<StudyPlanTask> tasklist, StudyPlan studyplan);
        void PlanUpdate(int ProjectId);

        string ImportTaskMasterDataFromTaskMaster(StudyPlan studyplan, int id);
    }
}
