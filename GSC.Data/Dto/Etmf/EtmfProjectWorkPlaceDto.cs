using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class EtmfProjectWorkPlaceDto : BaseDto
    {
        public int ProjectId { get; set; }
        public Data.Entities.Master.Project Project { get; set; }


        public int ProjectWorkplaceId { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }



        public List<EtmfProjectWorkPlaceDto> ProjectWorkplaceCountry { get; set; }
        public List<EtmfProjectWorkPlaceDto> ProjectWorkplaceSite { get; set; }
        public List<EtmfProjectWorkPlaceDto> ProjectWorkplaceTrial { get; set; }



        public int ProjectWorkplaceDetailId { get; set; }
        public int EtmfZoneMasterLibraryId { get; set; }
        //public int ProjectWorkPlaceZoneId { get; set; }
        public int EtmfSectionMasterLibraryId { get; set; }
        public string SectionName { get; set; }



        public int ProjectWorkplaceSectionId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public int? ParentArtificateId { get; set; }
        public bool IsNotRequired { get; set; }



        public string SubSectionName { get; set; }
        public int ProjectWorkplaceZoneId { get; set; }
        public string ZonName { get; set; }
        public string ChildName { get; set; }
        public string ProjectName { get; set; }
        //public int projectWorkplaceDetailId { get; set; }
        public string SubSectionArtifactName { get; set; }
        public int CompanyId { get; set; }


        public int ProjectWorkplaceSubSectionId { get; set; }

        public string ArtifactCodeName { get; set; }
        public string ArtifactName { get; set; }
    }
    public class WorkplaceFolderDto
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public int? ParentArtificateId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public string FolderName { get; set; }
        public int FolderId { get; set; }
        public int ItemId { get; set; }
        public int DocumentId { get; set; }
    }

    public class WorkplaceChartDto
    {
        public int All { get; set; }
        public int Missing { get; set; }
        public int PendingApprove { get; set; }
        public int AllPendingApprove { get; set; }
        public int AllDocument { get; set; }
        public int Final { get; set; }
        public int InComplete { get; set; }
        public int PendingReview { get; set; }
        public int AllPendingReview { get; set; }
        public int PendingFinal { get; set; }
        public int AllArtificate { get; set; }
        public int CoreArtificate { get; set; }
        public int RecommendedArtificate { get; set; }
        public int NotRequired { get; set; }
        public int Expired { get; set; }
    }

    public class WorkplaceChartFilterDto
    {
        public int ProjectId { get; set; }
        public int? WorkPlaceFolderId { get; set; }
        public int? ZoneId { get; set; }
        public int? SectionId { get; set; }
        public int? ArtificateId { get; set; }
        public int? SubSectionId { get; set; }
        public int? SubSectionArtificateId { get; set; }

    }

    public class ChartReport
    {
        public string ProjectCode { get; set; }
        public string WorkPlaceFolderType { get; set; }
        public string WorkPlaceFolderName { get; set; }
        public string ZoneName { get; set; }
        public string SectionName { get; set; }
        public string ArtificateName { get; set; }
        public string SubSectionName { get; set; }
        public string SubSectionArtificateName { get; set; }
        public string DocumentName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ReviewerApproverName { get; set; }
    }

    public class EtmfGroupSearchModel
    {
        public dynamic FolderData { get; set; }
        public dynamic ZoneData { get; set; }
        public dynamic SectionData { get; set; }
        public dynamic SubSectionData { get; set; }
        public List<EtmfSearchModel> SearchData { get; set; }
    }
    public class EtmfSearchModel
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string WorkPlaceFolderName { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public string ZoneName { get; set; }
        public int ZoneId { get; set; }
        public string SectionName { get; set; }
        public int SectionId { get; set; }
        public string ArtificateName { get; set; }
        public string SubSectionName { get; set; }
        public int SubSectionId { get; set; }
        public string SubSectionArtificateName { get; set; }
        public string SiteName { get; set; }
        public int TableTag { get; set; }
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public List<ProjectWorkplaceArtificatedocumentDto> DocumentList { get; set; }
    }

    public class DownloadFile
    {
        public string DocumentName { get; set; }
        public byte[] FileBytes { get; set; }
        public string MIMEType { get; set; }
    }
}
