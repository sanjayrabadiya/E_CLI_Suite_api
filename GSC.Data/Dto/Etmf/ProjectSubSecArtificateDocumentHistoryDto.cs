﻿using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectSubSecArtificateDocumentHistoryDto : BaseDto
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int? ProjectSubSecArtificateDocumentReviewId { get; set; }
        public int? ProjectSubSecArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class ProjectSubSecArtificateDocumentExpiryHistoryDto : BaseDto
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int? ProjectSubSecArtificateDocumentReviewId { get; set; }
        public int? ProjectSubSecArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
    }
}
