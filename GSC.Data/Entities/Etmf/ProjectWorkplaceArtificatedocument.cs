using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using GSC.Common.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceArtificatedocument : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public string DocumentName { get; set; }
        public ArtifactDocStatusType Status { get; set; }
        public string Version { get; set; }
        public string DocPath { get; set; }
        public int CompanyId { get; set; }
        public bool? IsAccepted { get; set; }
        public bool IsMoved { get; set; }
        public bool? IsReplyAllComment { get; set; }
        public int? ParentDocumentId { get; set; }
        [ForeignKey("ProjectWorkplaceArtificateId")]
        public EtmfProjectWorkPlace ProjectWorkplaceArtificate { get; set; }
        public List<ProjectArtificateDocumentReview> ProjectArtificateDocumentReview { get; set; }
        public List<ProjectArtificateDocumentApprover> ProjectArtificateDocumentApprover { get; set; }
        public List<ProjectArtificateDocumentComment> ProjectArtificateDocumentComment { get; set; }
        public List<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }
    }
}
