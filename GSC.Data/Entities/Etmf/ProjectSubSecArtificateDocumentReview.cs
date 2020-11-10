using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectSubSecArtificateDocumentReview : BaseEntity
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public int CompanyId { get; set; }
        public DateTime? SendBackDate { get; set; }
        public string Message { get; set; }
        public ProjectWorkplaceSubSecArtificatedocument ProjectWorkplaceSubSecArtificateDocument { get; set; }
        public List<ProjectSubSecArtificateDocumentHistory> ProjectSubSecArtificateDocumentHistory { get; set; }
    }
}
