using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableTemplateRight : BaseEntity, ICommonAduit
    {
        public int VariableTemplateId { get; set; }
        public int SecurityRoleId { get; set; }
    }
}