using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class VariableCategory : BaseEntity, ICommonAduit
    {
        public string CategoryCode { get; set; }

        public string CategoryName { get; set; }
        public int? CompanyId { get; set; }
        public VariableCategoryType? SystemType { get; set; }
    }
}