using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignRepository : IGenericRepository<ProjectDesign>
    {
        IList<DropDownDto> GetProjectByDesignDropDown();
        bool IsCompleteExist(int projectDesignId, string moduleName, bool isComplete);
        bool IsWorkFlowOrEditCheck(int projectDesignid);

        bool CheckPeriodWithProjectPeriod(int projectDesignid, int projectId);
    }
}