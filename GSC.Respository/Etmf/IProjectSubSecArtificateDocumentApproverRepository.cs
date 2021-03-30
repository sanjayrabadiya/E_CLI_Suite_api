using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System.Collections.Generic;

namespace GSC.Respository.Etmf
{
    public interface IProjectSubSecArtificateDocumentApproverRepository : IGenericRepository<ProjectSubSecArtificateDocumentApprover>
    {
        List<ProjectSubSecArtificateDocumentApproverHistory> GetArtificateDocumentApproverHistory(int Id);
        List<ProjectSubSecArtificateDocumentReviewDto> UserNameForApproval(int Id, int ProjectId);
        void SendMailForApprover(ProjectSubSecArtificateDocumentApproverDto ProjectSubSecArtificateDocumentApproverDto);
        void IsApproveDocument(int Id);
        List<DashboardDto> GetEtmfMyTaskList(int ProjectId);
    }
}