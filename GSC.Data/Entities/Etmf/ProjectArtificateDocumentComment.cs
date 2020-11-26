using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentComment : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public string Comment { get; set; }
        public string Response { get; set; }
        public int? ResponseBy { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool IsClose { get; set; }
        public int? CloseBy { get; set; }
        public DateTime? CloseDate { get; set; }
        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
    }
}
