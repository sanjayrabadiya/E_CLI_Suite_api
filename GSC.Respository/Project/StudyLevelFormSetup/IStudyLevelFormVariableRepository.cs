using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.StudyLevelFormSetup
{
    public interface IStudyLevelFormVariableRepository : IGenericRepository<StudyLevelFormVariable>
    {
        string Duplicate(StudyLevelFormVariable objSave);
        IList<StudyLevelFormVariableBasicDto> GetVariabeBasic(int studyLevelFormId);
        IList<DropDownDto> GetVariableDropDown(int studyLevelFormId);
        StudyLevelFormVariableRelationDto GetStudyLevelFormVariableRelation(int id);
    }
}
