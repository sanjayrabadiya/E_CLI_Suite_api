using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Client
{
    public class ClientAddress : BaseEntity, ICommonAduit
    {
        public int ClientId { get; set; }

        public bool IsDefault { get; set; }

        [ForeignKey("LocationId")] public Location.Location Location { get; set; }
    }
}