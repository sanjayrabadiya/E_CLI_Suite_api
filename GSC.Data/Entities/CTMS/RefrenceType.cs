using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.CTMS
{
    public class RefrenceTypes : BaseEntity, ICommonAduit
    {
        public RefrenceType? RefrenceType { get; set; }
        public int TaskMasterId { get; set; }
        [ForeignKey("TaskMasterId")]
        public TaskMaster TaskMaster { get; set; }
    }
}
