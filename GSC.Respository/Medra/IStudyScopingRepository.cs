using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using System.Collections.Generic;

namespace GSC.Respository.Medra
{
    public interface IStudyScopingRepository : IGenericRepository<StudyScoping>
    {
        string Duplicate(StudyScoping objSave);
        List<StudyScopingDto> GetStudyScopingList(int projectId, bool isDeleted);
        bool checkForScopingEdit(int ProjectDesignVariableId);
        //  StudyScoping GetData(int MeddraCodingId);
    }
}