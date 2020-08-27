using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentComment : BaseEntity
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public string Comment { get; set; }
        public string Response { get; set; }
        public int? ResponseBy { get; set; }
        public DateTime? ResponseDate { get; set; }
        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
    }
}
