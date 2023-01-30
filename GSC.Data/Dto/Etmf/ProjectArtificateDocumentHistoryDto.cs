using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectArtificateDocumentHistoryDto : BaseDto
    {
        public int ProjectWorkplaceArtificateDocumentId { get; set; }
        public int? ProjectArtificateDocumentReviewId { get; set; }
        public int? ProjectArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class ProjectArtificateDocumentExpiryHistoryDto : BaseDto
    {
        public int ProjectWorkplaceArtificateDocumentId { get; set; }
        public int? ProjectArtificateDocumentReviewId { get; set; }
        public int? ProjectArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
    }
}
