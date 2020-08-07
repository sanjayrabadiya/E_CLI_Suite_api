using GSC.Data.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Master
{
    public class ScopeName : BaseEntity
    {
        [Column("ScopeName")]
        public string Name { get; set; }

        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }
}