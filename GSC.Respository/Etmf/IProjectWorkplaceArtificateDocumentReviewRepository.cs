using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Dto.Etmf;
using System;
using System.Collections.Generic;
using System.Text;
using GSC.Data.Dto.Master;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificateDocumentReviewRepository : IGenericRepository<ProjectArtificateDocumentReview>
    {
        List<ProjectArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId, int ProjectDetailsId);
        void SaveDocumentReview(List<ProjectArtificateDocumentReviewDto> pojectArtificateDocumentReviewDto);
        void SaveByDocumentIdInReview(int projectWorkplaceArtificateDocumentId);
        void SendMailToSendBack(ProjectArtificateDocumentReview ReviewDto);
        List<ProjectArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id);
        List<DashboardDto> GetSendDocumentList(int ProjectId);
        List<DashboardDto> GetSendBackDocumentList(int ProjectId);
        bool GetReviewPending(int documentId);
        void SendMailToReviewer(ProjectArtificateDocumentReviewDto ReviewDto);
        List<ProjectArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId);
        int ReplaceUser(int documentId, int actualUserId, int replaceUserId);
    }
}
