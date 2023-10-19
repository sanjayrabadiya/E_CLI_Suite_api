using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IStudyPlanTaskResourceRepository : IGenericRepository<StudyPlanTaskResource>
    {
        List<StudyPlanTaskResourceGridDto> GetStudyPlanTaskResourceList(bool isDeleted, int studyPlanTaskId);
        string Duplicate(StudyPlanTaskResource objSave);
    }
}
