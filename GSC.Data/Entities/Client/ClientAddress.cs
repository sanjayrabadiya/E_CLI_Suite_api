using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Client
{
    public class ClientAddress : BaseEntity
    {
        public int ClientId { get; set; }

        public bool IsDefault { get; set; }

        [ForeignKey("LocationId")] public Location.Location Location { get; set; }
    }
}