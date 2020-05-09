using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVariableValueRepository : IGenericRepository<ProjectDesignVariableValue>
    {
        IList<DropDownDto> GetProjectDesignVariableValueDropDown(int projectDesignVariableId);
    }
}