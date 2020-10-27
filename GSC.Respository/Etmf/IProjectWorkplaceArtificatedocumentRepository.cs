using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificatedocumentRepository : IGenericRepository<ProjectWorkplaceArtificatedocument>
    {
        int deleteFile(int id);

        void UpdateApproveDocument(int documentId, bool IsAccepted);

        List<CommonArtifactDocumentDto> GetDocumentList(int id);
        string Duplicate(ProjectWorkplaceArtificatedocument objSave, ProjectWorkplaceArtificatedocumentDto objSaveDto);
        ProjectWorkplaceArtificatedocument AddDocument(ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto);
        ProjectWorkplaceArtificatedocument AddMovedDocument(WorkplaceFolderDto item);
        List<DropDownDto> GetEtmfZoneDropdown();
        List<DropDownDto> GetEtmfSectionDropdown(int zoneId);
        List<DropDownDto> GetEtmfArtificateDropdown(int sectionId);
        IList<EtmfAuditLogReportDto> GetEtmfAuditLogReport(EtmfAuditLogReportSearchDto filters);
    }
}
