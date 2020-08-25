using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableRemarks : BaseEntity
    {
        public int VariableId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
    }
}
