using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Etmf
{
    public class ProjectSubSecArtificateDocumentApproverDto : BaseDto
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int UserId { get; set; }
        public bool? IsApproved { get; set; }
        public string Comment { get; set; }
        public int CompanyId { get; set; }
    }

    public class ProjectSubSecArtificateDocumentApproverHistory : BaseAuditDto
    {
        public int ProjectWorkplaceSubSecArtificateDocumentId { get; set; }
        public int ProjectArtificateDocumentHistoryId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string DocumentName { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public bool? IsApproved { get; set; }
    }
}
