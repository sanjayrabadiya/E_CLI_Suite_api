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
    public interface IStudyLevelFormRepository : IGenericRepository<StudyLevelForm>
    {
        List<StudyLevelFormGridDto> GetStudyLevelFormList(bool isDeleted);
        string Duplicate(StudyLevelForm objSave);
        IList<DropDownDto> GetTemplateDropDown(int projectId);
    }
}
