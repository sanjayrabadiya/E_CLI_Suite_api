using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Client
{
    public class ClientContact : BaseEntity
    {
        public int ClientId { get; set; }

        public string ContactNo { get; set; }

        public string ContactName { get; set; }

        public bool IsDefault { get; set; }

        public int ContactTypeId { get; set; }

        [ForeignKey("ContactTypeId")] public ContactType ContactType { get; set; }
    }
}