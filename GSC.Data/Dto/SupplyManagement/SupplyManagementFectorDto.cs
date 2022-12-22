using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementFectorDto : BaseDto
    {
        public int ProjectId { get; set; }
        public string Formula { get; set; }
        public string ProjectCode { get; set; }
        
        public List<SupplyManagementFectorDetailDto> Children { get; set; }
    }

    public class SupplyManagementFectorGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public string Formula { get; set; }
        public string ProjectCode { get; set; }
        
    }
}
