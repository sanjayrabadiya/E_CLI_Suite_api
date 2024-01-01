using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;


namespace GSC.Respository.CTMS
{
    public interface IStudyPlanResourceRepository : IGenericRepository<StudyPlanResource>
    {
        dynamic GetTaskResourceList(bool isDeleted, int PlanTaskId);
        string Duplicate(StudyPlanResource objSave);
        string ValidationCurrency(int resourceId,int studyplanId);
        dynamic ResourceById( int id);
        dynamic GetResourceInf(int studyPlantaskId ,int resourceId);
        void TotalCostUpdate(StudyPlanResource studyPlanResource);
    }
}
