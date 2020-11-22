using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class VariableValue : BaseEntity
    {
        public int VariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public int SeqNo { get; set; }
        public bool IsDefault { get; set; }
    }
}