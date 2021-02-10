using GSC.Common.Base;
using GSC.Common.Common;

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
    }
}