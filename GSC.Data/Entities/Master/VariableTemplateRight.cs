using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateRight : BaseEntity
    {
        public int VariableTemplateId { get; set; }
        public int SecurityRoleId { get; set; }
    }
}