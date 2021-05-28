using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyLocationDto : BaseDto
    {
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
    }

    public class SupplyLocationGridDto : BaseAuditDto
    {
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
    }
}
