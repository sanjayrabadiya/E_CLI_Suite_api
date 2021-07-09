using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class PharmacyStudyProductTypeDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int ProductTypeId { get; set; }
        public ProductUnitType ProductUnitType { get; set; }
    }

    public class PharmacyStudyProductTypeGridDto : BaseAuditDto
    {
        public string Project { get; set; }
        public string ProductType { get; set; }
        public string ProductUnitType { get; set; }
    }
}
