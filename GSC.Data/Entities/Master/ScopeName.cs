using GSC.Common.Base;
using GSC.Common.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Master
{
    public class ScopeName : BaseEntity, ICommonAduit
    {
        [Column("ScopeName")]
        public string Name { get; set; }

        public string Notes { get; set; }
        public int? CompanyId { get; set; }
    }
}