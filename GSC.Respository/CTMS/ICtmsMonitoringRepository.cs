using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ICtmsMonitoringRepository : IGenericRepository<CtmsMonitoring>
    {
        List<CtmsMonitoringGridDto> GetMonitoringForm(int projectId, int siteId, int activityId);
        string StudyLevelFormAlreadyUse(int StudyLevelFormId);
        CtmsMonitoringGridDto GetMonitoringFormforDashboard(int ctmsMonitoringId, int activityId);
        string AddStudyPlanTask(CtmsMonitoringDto ctmsMonitoringDto);
        string UpdateStudyPlanTask(CtmsMonitoringDto ctmsMonitoringDto);
        void CloneForm(int ctmsMonitoringId, int noOfClones);
        void AddReSchedule(CtmsMonitoring record);
    }
}