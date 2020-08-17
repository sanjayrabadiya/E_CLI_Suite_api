using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;

namespace GSC.Respository.Master
{
    public interface IProjectRepository : IGenericRepository<Data.Entities.Master.Project>
    {
        List<DropDownDto> GetProjectDropDown();
        List<ProjectDropDown> GetParentProjectDropDown();
        IList<ProjectDto> GetProjectList(bool isDeleted);
        void Save(Data.Entities.Master.Project project);
        string Duplicate(Data.Entities.Master.Project objSave);
        List<DropDownDto> GetProjectNumberDropDown();
        Task<ProjectDetailDto> GetProjectDetailWithPeriod(int projectId);
        IList<ProjectDropDown> GetProjectForAttendance(bool isStatic);
        IList<ProjectDropDown> GetProjectsForDataEntry();
        List<ProjectDropDown> GetChildProjectDropDown(int parentProjectId);
        string CheckChildProjectExists(int id);
        string CheckParentProjectExists(int id);
        int GetNoOfSite(int id);
        List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int parentProjectId);
        IList<ProjectDropDown> GetProjectsByLock(bool isLock);
        ProjectDetailsDto GetProjectDetails(int projectId);

        IList<ProjectDto> GetSitesList(int projectId, bool isDeleted);

        string GetAutoNumber();
        string GetAutoNumberForSites();
        int? GetParentProjectId(int id);

        List<ProjectDropDown> GetChildProjectRightsDropDown();
    }
}