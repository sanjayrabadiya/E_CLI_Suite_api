using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentApprover : BaseEntity
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int UserId { get; set; }
        public bool? IsApproved { get; set; }
        public string Comment { get; set; }
        public int CompanyId { get; set; }
        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
        public List<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }
    }
}
