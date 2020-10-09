using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Dto.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificateDocumentReviewRepository : IGenericRepository<ProjectArtificateDocumentReview>
    {
        List<ProjectArtificateDocumentReviewDto> UserRoles(int Id);
        void SaveDocumentReview(List<ProjectArtificateDocumentReviewDto> pojectArtificateDocumentReviewDto);
        //List<int> GetProjectArtificateDocumentReviewList();
        void SaveByDocumentIdInReview(int projectWorkplaceArtificateDocumentId);

        void SendMailToSendBack(ProjectArtificateDocumentReview ReviewDto);
        List<ProjectArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id);
    }
}
