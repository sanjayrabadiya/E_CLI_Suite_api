using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementUploadFileVisit : BaseEntity, ICommonAduit
    {
        public int SupplyManagementUploadFileDetailId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string Value { get; set; }
        public SupplyManagementUploadFileDetail SupplyManagementUploadFileDetail { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
    }
}
