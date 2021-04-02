using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVariableValueRepository : IGenericRepository<ProjectDesignVariableValue>
    {
        IList<DropDownDto> GetProjectDesignVariableValueDropDown(int projectDesignVariableId);
        FileStreamResult GetDesignReport(ProjectDatabaseSearchDto search);
    }
}