using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class FoodType : BaseEntity
    {
        public string TypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}