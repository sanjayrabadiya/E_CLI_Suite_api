using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class CentralDepotDto : BaseDto
    {
      
        public DepotType DepotType { get; set; }
        public int? SupplyLocationId { get; set; }
        public int? CountryId { get; set; }
        public int? ProjectId { get; set; }
        public string StorageArea { get; set; }
        public int? MinTemp { get; set; }
        public int? MaxTemp { get; set; }
        public int? MinHumidity { get; set; }
        public int? MaxHumidity { get; set; }
        public int? CompanyId { get; set; }
    }

    public class CentralDepotGridDto : BaseAuditDto
    {
      
        public string DepotType { get; set; }
        public string SupplyLocation { get; set; }
        public string Country { get; set; }
        public string Project { get; set; }
        public string StorageArea { get; set; }
        public int? MinTemp { get; set; }
        public int? MaxTemp { get; set; }
        public int? MinHumidity { get; set; }
        public int? MaxHumidity { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
    }
}
