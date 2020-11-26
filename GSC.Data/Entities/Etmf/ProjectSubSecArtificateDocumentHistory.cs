using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectSubSecArtificateDocumentHistory : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int? ProjectSubSecArtificateDocumentReviewId { get; set; }
        public int? ProjectSubSecArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public ProjectWorkplaceSubSecArtificatedocument ProjectWorkplaceSubSecArtificateDocument { get; set; }
    }
}
