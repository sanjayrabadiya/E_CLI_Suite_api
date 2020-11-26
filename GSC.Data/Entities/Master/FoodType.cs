using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class FoodType : BaseEntity, ICommonAduit
    {
        public string TypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}