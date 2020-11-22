using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSubSecArtificatedocument : BaseEntity
    {
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public string DocumentName { get; set; }
        public string DocPath { get; set; }
        public int CompanyId { get; set; }
        public ArtifactDocStatusType Status { get; set; }
        public string Version { get; set; }
        public bool? IsAccepted { get; set; }
        public int? ParentDocumentId { get; set; }
        public ProjectWorkplaceSubSectionArtifact ProjectWorkplaceSubSectionArtifact { get; set; }
        public List<ProjectSubSecArtificateDocumentReview> ProjectSubSecArtificateDocumentReview { get; set; }
        public List<ProjectSubSecArtificateDocumentApprover> ProjectSubSecArtificateDocumentApprover { get; set; }
        public List<ProjectSubSecArtificateDocumentComment> ProjectSubSecArtificateDocumentComment { get; set; }
        public List<ProjectSubSecArtificateDocumentHistory> ProjectSubSecArtificateDocumentHistory { get; set; }
    }
}
