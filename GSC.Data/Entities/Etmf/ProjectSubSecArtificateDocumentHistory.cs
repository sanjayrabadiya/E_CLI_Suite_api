using GSC.Common.Base;
using GSC.Common.Common;
using System;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectSubSecArtificateDocumentHistory : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int? ProjectSubSecArtificateDocumentReviewId { get; set; }
        public int? ProjectSubSecArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProjectWorkplaceSubSecArtificatedocument ProjectWorkplaceSubSecArtificateDocument { get; set; }
    }
}
