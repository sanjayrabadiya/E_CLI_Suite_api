using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Client
{
    public class ClientAddressDto : BaseDto
    {
        public int ClientId { get; set; }

        public bool IsDefault { get; set; }

        public Entities.Location.Location Location { get; set; }
    }
}