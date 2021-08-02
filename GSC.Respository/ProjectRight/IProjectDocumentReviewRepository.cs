using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;

namespace GSC.Respository.ProjectRight
{
    public interface IProjectDocumentReviewRepository : IGenericRepository<ProjectDocumentReview>
    {
        void SaveByUserId(int projectId, int userId);
        void SaveByDocumentId(int documnetId, int projectId);
        void DeleteByUserId(int projectId, int userId);
        void DeleteByDocumentId(int documnetId, int projectId);
        ProjectDashBoardDto GetProjectDashboard();

        ProjectDashBoardDto GetProjectDashboardbyId(int id);
        List<DropDownDto> GetProjectDropDownProjectRight();
        ProjectDashBoardDto GetCompleteTrainingDashboard(int id);
        List<ProjectDropDown> GetChildProjectDropDownProjectRight(int ParentProjectId);
        List<DropDownDto> GetParentProjectDropDownProjectRight();
        int GetPendingProjectTrainingCount(int id);
    }
}