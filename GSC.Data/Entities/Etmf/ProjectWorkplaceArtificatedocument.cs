﻿using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

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
        public int? ParentDocumentId { get; set; }
        public ProjectWorkplaceArtificate ProjectWorkplaceArtificate { get; set; }
        public List<ProjectArtificateDocumentReview> ProjectArtificateDocumentReview { get; set; }
        public List<ProjectArtificateDocumentApprover> ProjectArtificateDocumentApprover { get; set; }
        public List<ProjectArtificateDocumentComment> ProjectArtificateDocumentComment { get; set; }
        public List<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }
    }
}
