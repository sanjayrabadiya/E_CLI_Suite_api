using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableRemarks : BaseEntity, ICommonAduit
    {
        public int VariableId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
        public int SeqNo { get; set; }
    }
}
