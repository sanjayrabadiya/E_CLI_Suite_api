using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface ITaskTemplateRepository : IGenericRepository<TaskTemplate>
    {
        List<TaskTemplateGridDto> GetStudyTrackerList(bool isDeleted);
        string Duplicate(TaskTemplate objSave);
        List<DropDownDto> GetTaskTemplateDropDown();
        string AlreadyUSed(int id);
    }
}
