using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectWorkplaceArtificateDto : BaseDto
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public int? ParentArtificateId { get; set; }
        public bool IsNotRequired { get; set; }
    }
    public class WorkplaceFolderDto
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public int? ParentArtificateId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public string FolderName { get; set; }
        public int FolderId { get; set; }
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
        public int AllArtificate { get; set; }
        public int CoreArtificate { get; set; }
        public int RecommendedArtificate { get; set; }
        public int NotRequired { get; set; }
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
}
