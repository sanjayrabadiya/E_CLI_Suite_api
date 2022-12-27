using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using GSC.Common.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSubSecArtificatedocument : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public string DocumentName { get; set; }
        public string DocPath { get; set; }
        public int CompanyId { get; set; }
        public ArtifactDocStatusType Status { get; set; }
        public string Version { get; set; }
        public bool? IsAccepted { get; set; }
        public int? ParentDocumentId { get; set; }
        public bool? IsReplyAllComment { get; set; }

        [ForeignKey("ProjectWorkplaceSubSectionArtifactId")]
        public EtmfProjectWorkPlace ProjectWorkplaceSubSectionArtifact { get; set; }
        public List<ProjectSubSecArtificateDocumentReview> ProjectSubSecArtificateDocumentReview { get; set; }
        public List<ProjectSubSecArtificateDocumentApprover> ProjectSubSecArtificateDocumentApprover { get; set; }
        public List<ProjectSubSecArtificateDocumentComment> ProjectSubSecArtificateDocumentComment { get; set; }
        public List<ProjectSubSecArtificateDocumentHistory> ProjectSubSecArtificateDocumentHistory { get; set; }
    }
}
