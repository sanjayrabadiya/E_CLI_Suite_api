using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class VariableCategory : BaseEntity
    {
        public string CategoryCode { get; set; }

        public string CategoryName { get; set; }
        public int? CompanyId { get; set; }
        public VariableCategoryType? SystemType { get; set; }
    }
}