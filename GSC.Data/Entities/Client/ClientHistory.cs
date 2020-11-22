using GSC.Common.Base;

namespace GSC.Data.Entities.Client
{
    public class ClientHistory : BaseEntity
    {
        public int ClientId { get; set; }

        public string Note { get; set; }
    }
}