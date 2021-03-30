using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System.Collections.Generic;

namespace GSC.Respository.Etmf
{
    public interface IProjectSubSecArtificateDocumentReviewRepository : IGenericRepository<ProjectSubSecArtificateDocumentReview>
    {
        void SaveByDocumentIdInReview(int ProjectWorkplaceSubSecArtificateDocumentId);
        void SaveDocumentReview(List<ProjectSubSecArtificateDocumentReviewDto> ProjectSubSecArtificateDocumentReviewDto);
        void SendMailToReviewer(ProjectSubSecArtificateDocumentReviewDto ReviewDto);
        List<ProjectSubSecArtificateDocumentReviewDto> UserRoles(int Id, int ProjectId);
        List<ProjectSubSecArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id);
        void SendMailToSendBack(ProjectSubSecArtificateDocumentReview ReviewDto);
        List<DashboardDto> GetSendDocumentList(int ProjectId);
        List<DashboardDto> GetSendBackDocumentList(int ProjectId);
    }
}