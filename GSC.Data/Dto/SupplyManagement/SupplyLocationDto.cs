using GSC.Data.Entities.Common;


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
        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
    }
}
