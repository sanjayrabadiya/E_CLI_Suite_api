using GSC.Data.Entities.Common;
using System.Collections.Generic;


namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementUploadFileDetailDto : BaseDto
    {
        public int SupplyManagementUploadFileId { get; set; }
        public int RandomizationNo { get; set; }
        public string TreatmentType { get; set; }
        public string KitNo { get; set; }
        public IList<SupplyManagementUploadFileVisitDto> Visits { get; set; } = null;

        public string ScreeningNumber { get; set; }

        public string SiteName { get; set; }
    }
}
