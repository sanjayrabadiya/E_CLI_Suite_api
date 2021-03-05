using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Etmf
{
    public class EtmfUserPermission : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }

        public int ProjectWorkplaceDetailId { get; set; }

        public bool IsView { get; set; }

        public bool IsAdd { get; set; }

        public bool IsEdit { get; set; }

        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public bool IsRevoked { get; set; }
        public string RollbackReason { get; set; }
        public int? AuditReasonId { get; set; }
        public ProjectWorkplaceDetail ProjectWorkplaceDetail { get; set; }
        public User User { get; set; }
        public AuditReason AuditReason { get; set; }
    }
}