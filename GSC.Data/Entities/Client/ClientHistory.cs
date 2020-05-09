using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Client
{
    public class ClientHistory : BaseEntity
    {
        public int ClientId { get; set; }

        public string Note { get; set; }
    }
}