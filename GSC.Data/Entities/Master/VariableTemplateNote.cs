using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateNote : BaseEntity
    {
        public int VariableTemplateId { get; set; }
        public string Note { get; set; }
        public bool IsPreview { get; set; }
    }
}