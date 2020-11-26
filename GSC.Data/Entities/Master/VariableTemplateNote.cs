using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateNote : BaseEntity, ICommonAduit
    {
        public int VariableTemplateId { get; set; }
        public string Note { get; set; }
        public bool IsPreview { get; set; }
    }
}