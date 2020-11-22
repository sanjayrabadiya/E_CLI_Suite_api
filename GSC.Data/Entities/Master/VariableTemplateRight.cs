using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateRight : BaseEntity
    {
        public int VariableTemplateId { get; set; }
        public int SecurityRoleId { get; set; }
    }
}