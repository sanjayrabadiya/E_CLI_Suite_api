using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementUploadFileDetail : BaseEntity, ICommonAduit
    {
        public int SupplyManagementUploadFileId { get; set; }
        public int RandomizationNo { get; set; }
        public string TreatmentType { get; set; }
        public IList<SupplyManagementUploadFileVisit> Visits { get; set; } = null;
        public SupplyManagementUploadFile SupplyManagementUploadFile { get; set; }
    }
}
