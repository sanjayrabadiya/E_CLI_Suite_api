using GSC.Common.Base;
using GSC.Helper;


namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKitDosePriority : BaseEntity
    {
        public int ProjectId { get; set; }

        public decimal Dose { get; set; }


        public DosePriority DosePriority { get; set; }

       
    }
}
