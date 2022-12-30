using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectArtificateDocumentApproverDto : BaseDto
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int UserId { get; set; }
        public bool? IsApproved { get; set; }
        public string Comment { get; set; }
        public int CompanyId { get; set; }
        public int? SequenceNo { get; set; }
    }

    public class ProjectArtificateDocumentApproverHistory : BaseAuditDto
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int ProjectArtificateDocumentHistoryId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string DocumentName { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public bool? IsApproved { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
    }
}
