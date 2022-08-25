using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementUploadFileDetail : BaseEntity, ICommonAduit
    {
        public int SupplyManagementUploadFileId { get; set; }
        public int RandomizationNo { get; set; }
        public string TreatmentType { get; set; }

        public int? RandomizationId { get; set; }
        public IList<SupplyManagementUploadFileVisit> Visits { get; set; } = null;
        public SupplyManagementUploadFile SupplyManagementUploadFile { get; set; }

        //[ForeignKey("RandomizationId")]
        public Randomization Randomization { get; set; }
    }
}
