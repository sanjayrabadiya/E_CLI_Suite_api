using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableValue : BaseEntity, ICommonAduit
    {
        public int VariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public bool IsDefault { get; set; }
        public Variable Variable { get; set; }

        public string Style { get; set; }
    }
}