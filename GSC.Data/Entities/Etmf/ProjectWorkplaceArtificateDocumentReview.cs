using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectArtificateDocumentReview : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public int CompanyId { get; set; }
        public DateTime? SendBackDate { get; set; }
        public string Message { get; set; }
        public int? SequenceNo { get; set; }
        public ProjectWorkplaceArtificatedocument ProjectWorkplaceArtificatedDocument { get; set; }
        public List<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }
    }
}
