using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class CentralDepot : BaseEntity, ICommonAduit
    {
        public bool IsCompanyLevel { get; set; }
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
        public Data.Entities.Master.Project Project { get; set; }
        public SupplyLocation SupplyLocation { get; set; }
        public Country Country { get; set; }
    }
}
