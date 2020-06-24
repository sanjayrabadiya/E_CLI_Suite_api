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
        Task<ProjectDetailDto> GetProjectDesignDetail(int projectId);
        IList<DropDownDto> GetProjectByDesignDropDown();
        bool IsScreeningStarted(int projectId);
        string CheckCompleteDesign(int id);

        string Duplicate(ProjectDesign objSave);        
        bool IsCompleteExist(int projectDesignId, string moduleName, bool isComplete);
    }
}