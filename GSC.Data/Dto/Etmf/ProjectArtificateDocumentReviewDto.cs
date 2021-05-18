using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectArtificateDocumentReviewDto : BaseDto
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public bool IsRights { get; set; }
        public DateTime? SendBackDate { get; set; }
        public string Message { get; set; }
        public List<ProjectArtificateDocumentReviewDto> users { get; set; }
    }

    public class ProjectArtificateDocumentReviewHistory : BaseAuditDto
    {
        public int ProjectWorkplaceArtificatedDocumentId { get; set; }
        public int ProjectArtificateDocumentHistoryId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public DateTime? SendBackDate { get; set; }
        public string DocumentName { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public List<ProjectArtificateDocumentReviewDto> users { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
    }
}
