using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectSubSecArtificateDocumentApprover : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int UserId { get; set; }
        public bool? IsApproved { get; set; }
        public string Comment { get; set; }
        public int CompanyId { get; set; }
        public ProjectWorkplaceSubSecArtificatedocument ProjectWorkplaceSubSecArtificateDocument { get; set; }
        public List<ProjectSubSecArtificateDocumentHistory> ProjectSubSecArtificateDocumentHistory { get; set; }
    }
}
