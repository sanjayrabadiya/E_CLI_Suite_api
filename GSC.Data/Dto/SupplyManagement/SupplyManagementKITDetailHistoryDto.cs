using GSC.Data.Entities.Common;
using GSC.Helper;


namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementKITDetailHistoryDto : BaseAuditDto
    {
        public int SupplyManagementKITDetailId { get; set; }
        public KitStatus? Status { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public string StatusName { get; set; }

        public string KitNo { get; set; }

        public string FromProjectCode { get; set; }

        public string ToProjectCode { get; set; }

        public int? SupplyManagementShipmentId { get; set; }

    }
   
}
