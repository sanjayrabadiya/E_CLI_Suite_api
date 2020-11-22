﻿using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentHistory : BaseEntity
    {
        public int ProjectWorkplaceArtificateDocumentId { get; set; }
        public int? ProjectArtificateDocumentReviewId { get; set; }
        public int? ProjectArtificateDocumentApproverId { get; set; }
        public string DocumentName { get; set; }
        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
    }
}
