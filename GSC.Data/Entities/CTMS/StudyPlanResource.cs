using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlanResource : BaseEntity, ICommonAduit
    {
        public int StudyPlanTaskId { get; set; }
        public int ResourceTypeId { get; set; }
        public int? NoOfUnit { get; set; }
        public int? TotalCost { get; set; }
        public int? ConvertTotalCost { get; set; }
        public decimal? Discount { get; set; }
        public StudyPlanTask StudyPlanTask { get; set; }
        public ResourceType ResourceType { get; set; }
    }
}
