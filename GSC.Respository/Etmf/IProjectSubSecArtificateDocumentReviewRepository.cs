using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;

namespace GSC.Respository.Etmf
{
    public interface IProjectSubSecArtificateDocumentReviewRepository : IGenericRepository<ProjectSubSecArtificateDocumentReview>
    {
        void SaveByDocumentIdInReview(int ProjectWorkplaceSubSecArtificateDocumentId);
        void SaveDocumentReview(List<ProjectSubSecArtificateDocumentReviewDto> ProjectSubSecArtificateDocumentReviewDto);
        void SendMailToReviewer(ProjectSubSecArtificateDocumentReviewDto ReviewDto);
        List<ProjectSubSecArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId, int ProjectDetailsId);
        List<ProjectSubSecArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id);
        void SendMailToSendBack(ProjectSubSecArtificateDocumentReview ReviewDto);
        List<DashboardDto> GetSendDocumentList(int ProjectId);
        List<DashboardDto> GetSendBackDocumentList(int ProjectId);
        bool GetReviewPending(int documentId);
        List<ProjectSubSecArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId);
        int ReplaceUser(int documentId, int actualUserId, int replaceUserId);
        DateTime? GetMaxDueDate(int documentId);
    }
}