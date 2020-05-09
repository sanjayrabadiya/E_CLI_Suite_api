using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Configuration
{
    public class Company : BaseEntity
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }

        [ForeignKey("LocationId")] public Location.Location Location { get; set; }

        public string Logo { get; set; }
    }
}