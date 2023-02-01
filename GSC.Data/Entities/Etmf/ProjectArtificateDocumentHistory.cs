using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentHistory : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceArtificateDocumentId { get; set; }
        public int? ProjectArtificateDocumentReviewId { get; set; }
        public int? ProjectArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
    }
}
