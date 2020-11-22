using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class FoodType : BaseEntity
    {
        public string TypeName { get; set; }
        public int? CompanyId { get; set; }
    }
}