using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class VariableGroup : BaseEntity, ICommonAduit
    {
        public string VariableGroupName { get; set; }

        public string ShortName { get; set; }

        public int DomainId { get; set; }

        public string CDISCValue { get; set; }

        public string CDISCSubValue { get; set; }

        public short CoreVariableType { get; set; }

        public short RoleVariableType { get; set; }

        public int? VariableGroupId { get; set; }
        public int? CompanyId { get; set; }
    }
}