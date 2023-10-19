using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;


namespace GSC.Respository.CTMS
{
    public interface IStudyPlanResourceRepository : IGenericRepository<StudyPlanResource>
    {
        dynamic GetTaskResourceList(bool isDeleted, int PlanTaskId);
        string Duplicate(StudyPlanResource objSave);
        dynamic ResourceById( int id);
    }
}
