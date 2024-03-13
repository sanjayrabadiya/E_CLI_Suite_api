using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;

using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignRepository : IGenericRepository<ProjectDesign>
    {
        IList<DesignDropDownDto> GetProjectByDesignDropDown();
        bool IsCompleteExist(int projectDesignId, string moduleName, bool isComplete);
        bool IsWorkFlowOrEditCheck(int projectDesignid);

        bool CheckPeriodWithProjectPeriod(int projectDesignid, int projectId);
    }
}