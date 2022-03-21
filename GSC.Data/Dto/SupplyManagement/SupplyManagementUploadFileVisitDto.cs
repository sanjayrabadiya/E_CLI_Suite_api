using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementUploadFileVisitDto : BaseDto
    {
        public int SupplyManagementUploadFileDetailId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string Value { get; set; }

        public ProjectDesignVisit ProjectDesignVisit { get; set; }
    }
}
