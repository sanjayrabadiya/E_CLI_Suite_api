using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceSubSecArtificatedocumentRepository : IGenericRepository<ProjectWorkplaceSubSecArtificatedocument>
    {
        string getArtifactSectionDetail(ProjectWorkplaceSubSecArtificatedocumentDto projectWorkplaceSubSectionDto);
        int deleteSubsectionArtifactfile(int id);
        List<CommonArtifactDocumentModel> GetSubSecDocumentList(int Id);
        ProjectWorkplaceSubSecArtificatedocument AddDocument(ProjectWorkplaceSubSecArtificatedocumentDto projectWorkplaceSubSecArtificatedocumentDto);
        string ImportData(int Id);
        void UpdateApproveDocument(int documentId, bool IsAccepted);
        string SaveDocumentInFolder(ProjectWorkplaceSubSecArtificatedocument projectWorkplaceSubSecArtificatedocument, CustomParameter param);
        ProjectWorkplaceSubSecArtificatedocument WordToPdf(int Id);
        string GetDocumentHistory(int Id);
        CommonArtifactDocumentDto GetDocument(int id);
        CommonArtifactDocumentDto GetDocumentForPdfHistory(int Id);
        void UpdateSubDocumentComment(int documentId, bool? isComment);
        void UpdateDocumentExpiryStatus();
        List<ProjectSubSecArtificateDocumentExpiryHistoryDto> GetSubSectionDocumentHistory(int documentId);
        List<CommonArtifactDocumentDto> GetExpiredDocumentReports(int projectId);
        void IsApproveDocument(int Id);
    }
}
