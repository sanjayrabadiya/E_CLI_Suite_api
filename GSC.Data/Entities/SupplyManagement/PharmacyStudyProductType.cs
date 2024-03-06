using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class PharmacyStudyProductType : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ProductTypeId { get; set; }
        public ProductUnitType ProductUnitType { get; set; }
        public Entities.Master.Project Project { get; set; }
        public ProductType ProductType { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }


    }
}
