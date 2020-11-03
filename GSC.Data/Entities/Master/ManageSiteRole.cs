using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class ManageSiteRole : BaseEntity
    {
        public int TrialTypeId { get; set; }
        [ForeignKey("TrialTypeId")] public TrialType TrialType { get; set; }
        public int ManageSiteId { get; set; }
        [ForeignKey("ManageSiteId")]
        public ManageSite ManageSite { get; set; }
    }
}
