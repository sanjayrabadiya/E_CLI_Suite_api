using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Text;

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
