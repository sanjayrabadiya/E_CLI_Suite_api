using GSC.Data.Entities.Common;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Dto.Etmf
{
    public class EtmfUserPermissionDto : BaseAuditDto
    {
        public int UserId { get; set; }
        public int ProjectWorkplaceDetailId { get; set; }
        public int ProjectWorkplaceId { get; set; }
        public int WorkPlaceFolderId { get; set; }
        public string WorkPlaceFolder { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int? ParentWorksplaceFolderId { get; set; }
        public int? EtmfUserPermissionId { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public bool IsAll { get; set; }
        public bool hasChild { get; set; }
        public string UserName { get; set; }
        public string RollbackReason { get; set; }
        public int? AuditReasonId { get; set; }
        public string AuditReason { get; set; }
        public bool IsRevoke { get; set; }
        public bool IsRevoked { get; set; }
    }
}