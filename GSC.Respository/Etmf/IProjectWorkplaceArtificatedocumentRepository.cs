using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificatedocumentRepository : IGenericRepository<ProjectWorkplaceArtificatedocument>
    {
        int deleteFile(int id);

        void UpdateApproveDocument(int documentId, bool IsAccepted);

        List<CommonArtifactDocumentModel> GetDocumentList(int id);
        string Duplicate(ProjectWorkplaceArtificatedocument objSave, ProjectWorkplaceArtificatedocumentDto objSaveDto);
        ProjectWorkplaceArtificatedocument AddDocument(ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto);
        ProjectWorkplaceArtificatedocument AddMovedDocument(WorkplaceFolderDto data);
        List<DropDownDto> GetEtmfCountrySiteDropdown(int projectId, int folderId);
        List<DropDownDto> GetEtmfZoneDropdown(int projectId);
        List<DropDownDto> GetEtmfSectionDropdown(int zoneId);
        List<DropDownDto> GetEtmfArtificateDropdown(int sectionId);
        IList<EtmfAuditLogReportDto> GetEtmfAuditLogReport(EtmfAuditLogReportSearchDto filters);
        CommonArtifactDocumentDto GetDocument(int id);
        string ImportWordDocument(Stream stream, string FullPath);
        string SaveDocumentInFolder(ProjectWorkplaceArtificatedocument projectWorkplaceArtificatedocument, CustomParameter param);
        string ImportData(int Id);
        ProjectWorkplaceArtificatedocument WordToPdf(int Id);
        IList<EtmfStudyReportDto> GetEtmfStudyReport(StudyReportSearchDto filters);
        void UpdateDocumentComment(int documentId, bool? isComment);
        List<DropDownDto> GetEtmfSubSectionDropdown(int sectionId);
        List<DropDownDto> GetEtmfSubSectionArtificateDropdown(int subSectionId);
        void UpdateDocumentExpiryStatus();
        List<ProjectArtificateDocumentExpiryHistoryDto> GetDocumentHistory(int documentId);
        List<CommonArtifactDocumentDto> GetExpiredDocumentReports(int projectId);
        void IsApproveDocument(int Id);

        public DownloadFile DownloadDocument(int id);
    }
}
