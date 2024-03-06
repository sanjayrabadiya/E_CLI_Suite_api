using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;

namespace GSC.Respository.ProjectRight
{
    public interface IProjectRightRepository : IGenericRepository<Data.Entities.ProjectRight.ProjectRight>
    {
        List<ProjectRightListDto> GetProjectRights();
        List<ProjectRightDto> GetProjectRightByProjectId(int projectId);
        void UpdateIsReviewDone(int projectId);
       
        void SaveProjectAccessRight(List<ProjectRightDto> projectRightDto, int projectId);
        void SaveProjectRollbackRight(List<ProjectRightDto> projectRightDto, int projectId, int[] ids);
        List<int> GetProjectRightIdList();
        List<ProjectDocumentReviewDto> GetProjectRightDetailsByProjectId(int projectId);
        ProjectDocumentHistory GetProjectRightHistory(int projectId, int userId, int roleId);
        IList<ProjectTrainingDto> GetRoles(int ProjectId);
        IList<ProjectTrainingDto> GetUsers(int ProjectId);
        IList<ProjectAccessDto> GetProjectAccessReportList(ProjectTrainigAccessSearchDto filters);
        IList<ProjectTrainingDto> GetProjectTrainingReportList(ProjectTrainigAccessSearchDto filters);
        IList<UserReportDto> GetUserReportList(UserReportSearchDto filters);
        IList<UserReportDto> GetLoginLogoutReportList(UserReportSearchDto filters);
        List<ProjectDocumentReviewDto> EtmfUserDropDown(int projectId, int? userId);
        List<int> GetChildProjectRightIdList();
        List<int> GetParentProjectRightIdList();
        List<int> GetProjectCTMSRightIdList();
        List<int> GetProjectChildCTMSRightIdList();

        List<int> GetEtmfProjectRightIdList();

        List<int> GetEtmfChildProjectRightIdList();

    }
}