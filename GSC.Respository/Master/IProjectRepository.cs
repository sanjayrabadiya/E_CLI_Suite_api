using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.ProjectRight;

namespace GSC.Respository.Master
{
    public interface IProjectRepository : IGenericRepository<Data.Entities.Master.Project>
    {
        List<ProjectDropDown> GetParentProjectDropDown();
        IList<DashboardProject> GetProjectList(bool isDeleted);
        void Save(Data.Entities.Master.Project project);
        string Duplicate(Data.Entities.Master.Project objSave);
        string CheckAttendanceLimitPost(Data.Entities.Master.Project objSave);
        string CheckAttendanceLimitPut(Data.Entities.Master.Project objSave);

        IList<ProjectDropDown> GetProjectsForDataEntry();
        List<ProjectDropDown> GetChildProjectDropDown(int parentProjectId);

        string CheckChildProjectExists(int id);
        string CheckParentProjectExists(int id);
        int GetNoOfSite(int id);
        List<ProjectDropDown> GetChildProjectWithParentProjectDropDown(int parentProjectId);
        ProjectDetailsDto GetProjectDetails(int projectId);

        IList<ProjectGridDto> GetSitesList(int projectId, bool isDeleted);

        string GetAutoNumber();
        string GetAutoNumberForSites(int Id);
        int? GetParentProjectId(int id);

        List<ProjectDropDown> GetChildProjectRightsDropDown();
        List<ProjectDropDown> GetParentProjectDropDownwithoutRights();

        void UpdateProject(Data.Entities.Master.Project details);
        List<ProjectDropDown> GetParentStaticProjectDropDown();
        ProjectGridDto GetProjectDetailForDashboard(int ProjectId);
        List<ProjectDropDown> GetParentProjectDropDownforAE();
        List<ProjectDropDown> GetChildProjectDropDownforAE(int parentProjectId);
        IList<ProjectDropDown> GetAllProjectsForDataEntry();
        List<ProjectDropDown> GetParentProjectDropDownEtmf();
        List<ProjectDropDown> GetParentProjectDropDownStudyReport();
        IList<ProjectDropDown> GetProjectForAttendance(bool isStatic);
        string GetStudyCode(int ProjectId);
        string GetParentProjectCode(int ProjectId);
        string GetProjectSitesCode(Data.Entities.Master.Project project);

        void CloneStudy(CloneProjectDto cloneProject, Data.Entities.Master.Project project);//Clone Study Tinku Mahato (01-04-2022)

        List<ProjectDropDown> GetParentProjectDropDownForAddProjectNo();//prakash for volunteer screening project no add

        List<LockUnlockProject> GetParentStaticProject();

        List<ProjectDropDown> GetParentProjectCTMSDropDown();

        List<ProjectDropDown> GetParentStaticProjectDropDownIWRS();

        List<ProjectDropDown> GetProjectDropDownIWRS();

        List<ProjectDropDown> GetProjectDropDownIWRSUnblind();

        List<ProjectDropDown> GetChildProjectDropDownIWRS(int parentProjectId);
    }
}