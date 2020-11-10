using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectSubSecArtificateDocumentHistory : BaseEntity
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int? ProjectSubSecArtificateDocumentReviewId { get; set; }
        public int? ProjectSubSecArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public ProjectWorkplaceSubSecArtificatedocument ProjectWorkplaceSubSecArtificateDocument { get; set; }
    }
}
