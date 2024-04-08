using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IStudyPlanRepository : IGenericRepository<StudyPlan>
    {
        List<StudyPlanGridDto> GetStudyplanList(bool isDeleted);
        string ImportTaskMasterData(StudyPlan studyplan);
        string Duplicate(StudyPlan objSave);
        string ValidateTask(StudyPlanTask taskmasterDto, List<StudyPlanTask> tasklist, StudyPlan studyplan);
        void PlanUpdate(int ProjectId);
        void CurrencyRateAdd(StudyPlanDto objSave);
        void CurrencyRateUpdate(StudyPlanDto objSave);
        string ImportTaskMasterDataFromTaskMaster(StudyPlan studyplan, int id);
        bool UpdateApprovalPlan(int id, bool ifPlanApproval);
        List<ApprovalPlanHistory> GetApprovalPlanHistory(int id, string columnName);
        string SendMail(int id, bool ifPlanApproval, TriggerType triggerType);
    }
}
