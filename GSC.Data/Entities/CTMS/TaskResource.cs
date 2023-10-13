using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.CTMS
{
    public class TaskResource : BaseEntity, ICommonAduit
    {
        public int TaskMasterId { get; set; }
        public int ResourceTypeId { get; set; }
        public TaskMaster TaskMaster { get; set; }
        public ResourceType ResourceType { get; set; }
    }
}
