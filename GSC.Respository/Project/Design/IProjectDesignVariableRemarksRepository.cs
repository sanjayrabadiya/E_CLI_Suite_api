using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVariableRemarksRepository : IGenericRepository<ProjectDesignVariableRemarks>
    {
        IList<DropDownDto> GetProjectDesignVariableRemarksDropDown(int projectDesignVariableId);
    }
}